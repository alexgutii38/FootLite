using UnityEngine;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [Header("FX")]
    public ParticleSystem particulaSpawn;

    [Header("Límites de spawn")]
    public Renderer rendererSuelo;
    public float margenBorde = 1f;

    [Header("Suelo")]
    public LayerMask groundLayer;
    public float rayAltura = 50f;
    public float offsetSuelo = 0.05f;

    [Header("Base de datos de niveles")]
    public List<NivelConfig> nivelesDisponibles;

    [Header("Referencias")]
    public Transform jugador;
    public float radioSpawn = 12f;

    [Header("Jefe Final")]
    public GameObject prefabBoss;
    public float tiempoAparicionBoss = 60f;
    private bool bossAparecido = false;

    [Header("Seguridad")]
    public int limiteMaximoEnemigos = 100;

    private List<WaveConfig> oleadas = new List<WaveConfig>();
    private int indiceOleadaActual = 0;
    private float tiempoTranscurridoOleada = 0f;
    private float tiempoSiguienteSpawn = 0f;
    private WaveConfig oleadaActual;

    private int cicloCompleto = 0;
    private float multiplicadorDificultad = 1.0f;
    private float dificultadBaseDelNivel = 1.0f;

    // Multiplicador de velocidad de spawn (más alto = más enemigos más rápido)
    private float multiplicadorSpawnRate = 1.0f;

    // Cap: dificultad no supera 8x la base del nivel (ciclos infinitos no se vuelven imposibles)
    private const float CAP_CICLOS = 8f;

    void Start()
    {
        if (jugador == null)
            jugador = GameObject.FindGameObjectWithTag("Player").transform;

        CargarNivel();
        IniciarOleada(0);
    }

    void CargarNivel()
    {
        if (NivelManager.Instancia == null)
        {
            if (nivelesDisponibles.Count > 0) oleadas = new List<WaveConfig>(nivelesDisponibles[0].oleadas);
            return;
        }

        int mundo = NivelManager.Instancia.mundoSeleccionado;
        int nivel = NivelManager.Instancia.nivelSeleccionado;

        // ─── BALANCEO 3x3 ───────────────────────────────────────────────────────
        // Fórmula: M1N1=1.0 … M3N3=2.7  (mundo tiene más peso que nivel)
        dificultadBaseDelNivel = 1f + (mundo - 1) * 0.6f + (nivel - 1) * 0.25f;
        multiplicadorDificultad = dificultadBaseDelNivel;

        // Spawn rate: en M3N3 spawnan 1.8x más enemigos y más rápido
        multiplicadorSpawnRate = 1f + (mundo - 1) * 0.3f + (nivel - 1) * 0.1f;

        // Boss aparece antes en mundos más altos: M1→60s, M2→45s, M3→30s
        tiempoAparicionBoss = Mathf.Max(30f, 75f - mundo * 15f);

        // Tiempo para ganar: progresivo según mundo/nivel (300s … 480s)
        if (GameManager.Instancia != null)
        {
            float tiempoBase = 300f;
            float tiempoExtra = (mundo - 1) * 60f + (nivel - 1) * 30f;
            GameManager.Instancia.tiempoParaGanar = tiempoBase + tiempoExtra;
        }
        // ────────────────────────────────────────────────────────────────────────

        NivelConfig config = nivelesDisponibles.Find(n => n.mundoIndex == mundo && n.nivelIndex == nivel);

        if (config != null)
        {
            oleadas = new List<WaveConfig>(config.oleadas);
            Debug.Log($"Mundo {mundo} Nivel {nivel}: {config.nombreNivel} | Base {dificultadBaseDelNivel:F2}x | SpawnRate {multiplicadorSpawnRate:F2}x | Boss a {tiempoAparicionBoss}s");
        }
        else
        {
            Debug.LogWarning($"No se encontró NivelConfig para Mundo {mundo} Nivel {nivel}");
            if (nivelesDisponibles.Count > 0) oleadas = new List<WaveConfig>(nivelesDisponibles[0].oleadas);
        }
    }

    void Update()
    {
        if (!bossAparecido && prefabBoss != null)
        {
            float tiempoRestante = tiempoAparicionBoss - Time.timeSinceLevelLoad;

            if (BossUIManager.Instancia != null)
                BossUIManager.Instancia.ActualizarTemporizador(tiempoRestante);

            if (tiempoRestante <= 0)
            {
                SpawnearBoss();
                bossAparecido = true;
            }
        }

        if (oleadaActual == null) return;

        tiempoTranscurridoOleada += Time.deltaTime;

        if (tiempoTranscurridoOleada >= oleadaActual.duracionOleada)
            PasarSiguienteOleada();

        if (Time.time >= tiempoSiguienteSpawn)
        {
            SpawnEnemigos();
            // Intervalo reducido por spawn rate (más rápido en mundos altos)
            tiempoSiguienteSpawn = Time.time + oleadaActual.intervaloSpawn / multiplicadorSpawnRate;
        }
    }

    void IniciarOleada(int indice)
    {
        if (oleadas.Count == 0) return;

        indice = indice % oleadas.Count;

        if (indice == 0 && indiceOleadaActual != 0)
        {
            cicloCompleto++;
            float sinCap = dificultadBaseDelNivel * Mathf.Pow(1.15f, cicloCompleto);
            float capAbsoluto = dificultadBaseDelNivel * CAP_CICLOS;
            multiplicadorDificultad = Mathf.Min(sinCap, capAbsoluto);
            Debug.Log($"=== CICLO {cicloCompleto + 1} === Multiplicador: {multiplicadorDificultad:F2}x");
        }

        indiceOleadaActual = indice;
        oleadaActual = oleadas[indice];
        tiempoTranscurridoOleada = 0f;

        if (GameManager.Instancia != null)
            GameManager.Instancia.ActualizarTextoOleada(indiceOleadaActual + 1, oleadas.Count, cicloCompleto);
    }

    void PasarSiguienteOleada()
    {
        IniciarOleada(indiceOleadaActual + 1);
    }

    private void SpawnearBoss()
    {
        Vector3 posicionSpawn = transform.position;

        if (rendererSuelo != null)
        {
            Bounds limites = rendererSuelo.bounds;
            float x = Random.Range(limites.min.x + margenBorde, limites.max.x - margenBorde);
            float z = Random.Range(limites.min.z + margenBorde, limites.max.z - margenBorde);

            // Raycast desde encima del jugador para encontrar el suelo exacto
            Vector3 rayStart = new Vector3(x, jugador.position.y + rayAltura, z);
            RaycastHit hit;
            float groundY = jugador.position.y;
            if (Physics.Raycast(rayStart, Vector3.down, out hit, rayAltura * 2f))
                groundY = hit.point.y + offsetSuelo;

            posicionSpawn = new Vector3(x, groundY, z);
        }
        else
        {
            posicionSpawn = new Vector3(posicionSpawn.x, jugador.position.y, posicionSpawn.z);
        }

        GameObject bossObj = Instantiate(prefabBoss, posicionSpawn, Quaternion.identity);

        // Escalar boss según dificultad del mundo/nivel
        EnemigoBoss boss = bossObj.GetComponent<EnemigoBoss>();
        if (boss != null)
        {
            boss.vida = Mathf.Round(400f * dificultadBaseDelNivel);
            boss.danoPorContacto = Mathf.RoundToInt(25 * dificultadBaseDelNivel * 0.8f);
            boss.velocidadMovimiento = Mathf.Min(boss.velocidadMovimiento * (1f + (dificultadBaseDelNivel - 1f) * 0.3f), 3.5f);
        }
    }

    void SpawnEnemigos()
    {
        if (GameManager.Instancia != null && GameManager.Instancia.enemigosActivos >= limiteMaximoEnemigos) return;
        if (jugador == null || oleadaActual == null || oleadaActual.prefabsEnemigos.Length == 0) return;

        float dificultadTotal = oleadaActual.multiplicadorVida * multiplicadorDificultad;
        // Cantidad escalada por spawn rate
        int cantidad = Mathf.Max(1, Mathf.RoundToInt(oleadaActual.enemigosPorSpawn * multiplicadorSpawnRate));

        Bounds boundsSuelo = new Bounds();
        bool tieneBounds = false;

        if (rendererSuelo != null)
        {
            boundsSuelo = rendererSuelo.bounds;
            tieneBounds = true;
        }

        for (int i = 0; i < cantidad; i++)
        {
            Vector2 puntoRandom = Random.insideUnitCircle.normalized * radioSpawn;
            Vector3 spawnPos = jugador.position + new Vector3(puntoRandom.x, 0f, puntoRandom.y);

            if (tieneBounds)
            {
                spawnPos.x = Mathf.Clamp(spawnPos.x, boundsSuelo.min.x + margenBorde, boundsSuelo.max.x - margenBorde);
                spawnPos.z = Mathf.Clamp(spawnPos.z, boundsSuelo.min.z + margenBorde, boundsSuelo.max.z - margenBorde);
            }

            // Siempre desde encima del jugador, no desde los bounds del estadio
            // (los bounds del estadio incluyen stands altos que desvían el rayo)
            Vector3 rayStart = new Vector3(spawnPos.x, jugador.position.y + rayAltura, spawnPos.z);

            RaycastHit hit;
            bool hitOk = groundLayer.value == 0
                ? Physics.Raycast(rayStart, Vector3.down, out hit, rayAltura * 2f)
                : Physics.Raycast(rayStart, Vector3.down, out hit, rayAltura * 2f, groundLayer);

            if (hitOk) spawnPos.y = hit.point.y + offsetSuelo;
            else spawnPos.y = jugador.position.y;

            GameObject prefab = oleadaActual.prefabsEnemigos[Random.Range(0, oleadaActual.prefabsEnemigos.Length)];
            GameObject enemigo = Instantiate(prefab, spawnPos, Quaternion.identity);

            if (GameManager.Instancia != null) GameManager.Instancia.AgregarEnemigoActivo();

            if (particulaSpawn != null)
            {
                ParticleSystem particulaObj = Instantiate(particulaSpawn, spawnPos, Quaternion.identity);
                Destroy(particulaObj.gameObject, particulaObj.main.startLifetime.constantMax);
            }

            EscalarEnemigo(enemigo, dificultadTotal);
        }
    }

    private void EscalarEnemigo(GameObject enemigo, float dificultadTotal)
    {
        EnemigoCaminante scriptCam = enemigo.GetComponent<EnemigoCaminante>();
        if (scriptCam != null)
        {
            scriptCam.vida *= dificultadTotal;
            scriptCam.danoPorContacto = Mathf.RoundToInt(scriptCam.danoPorContacto * dificultadTotal * 0.8f);
            scriptCam.velocidadMovimiento = Mathf.Min(scriptCam.velocidadMovimiento * oleadaActual.multiplicadorVelocidad, 5.5f);
            scriptCam.experienciaAlMorir = Mathf.RoundToInt(scriptCam.experienciaAlMorir * dificultadTotal);
            return;
        }

        EnemigoDelantero scriptDel = enemigo.GetComponent<EnemigoDelantero>();
        if (scriptDel != null)
        {
            scriptDel.vida *= dificultadTotal;
            scriptDel.danoPorContacto = Mathf.RoundToInt(scriptDel.danoPorContacto * dificultadTotal * 0.8f);
            scriptDel.velocidadMovimiento = Mathf.Min(scriptDel.velocidadMovimiento * oleadaActual.multiplicadorVelocidad, 5.5f);
            scriptDel.experienciaAlMorir = Mathf.RoundToInt(scriptDel.experienciaAlMorir * dificultadTotal);
            return;
        }

        EnemigoArbitro scriptArb = enemigo.GetComponent<EnemigoArbitro>();
        if (scriptArb != null)
        {
            scriptArb.vida *= dificultadTotal;
            scriptArb.danoPorContacto = Mathf.RoundToInt(scriptArb.danoPorContacto * dificultadTotal * 0.8f);
            scriptArb.velocidadMovimiento = Mathf.Min(scriptArb.velocidadMovimiento * oleadaActual.multiplicadorVelocidad, 5.5f);
            scriptArb.experienciaAlMorir = Mathf.RoundToInt(scriptArb.experienciaAlMorir * dificultadTotal);
            scriptArb.danoAmarilla = Mathf.RoundToInt(scriptArb.danoAmarilla * dificultadTotal * 0.8f);
            scriptArb.danoRoja = Mathf.RoundToInt(scriptArb.danoRoja * dificultadTotal * 0.8f);
        }
    }
}
