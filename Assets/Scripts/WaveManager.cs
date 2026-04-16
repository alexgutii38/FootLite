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
    public float tiempoAparicionBoss = 60f; // Aparece a los 60 segundos
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

        NivelConfig config = nivelesDisponibles.Find(n => n.mundoIndex == mundo && n.nivelIndex == nivel);

        dificultadBaseDelNivel = 1f + ((mundo - 1) * 0.5f) + ((nivel - 1) * 0.2f);
        multiplicadorDificultad = dificultadBaseDelNivel; 

        if (config != null)
        {
            oleadas = new List<WaveConfig>(config.oleadas);
            Debug.Log($"Cargando Mundo {mundo} - Nivel {nivel}: {config.nombreNivel} | Dificultad Base: {dificultadBaseDelNivel}x");
        }
        else
        {
            Debug.LogWarning($"No se encontró NivelConfig para Mundo {mundo} Nivel {nivel}");
            if (nivelesDisponibles.Count > 0) oleadas = new List<WaveConfig>(nivelesDisponibles[0].oleadas);
        }
    }

void Update()
    {
        // --- CONTROL DEL JEFE ---
        if (!bossAparecido && prefabBoss != null)
        {
            float tiempoRestante = tiempoAparicionBoss - Time.timeSinceLevelLoad;
            
            // Actualizamos el texto en pantalla
            if (BossUIManager.Instancia != null) 
                BossUIManager.Instancia.ActualizarTemporizador(tiempoRestante);

            if (tiempoRestante <= 0)
            {
                SpawnearBoss();
                bossAparecido = true;
            }
        }
        // ------------------------

        if (oleadaActual == null) return;
        // ... (El resto de tu Update sigue igual)

        tiempoTranscurridoOleada += Time.deltaTime;

        if (tiempoTranscurridoOleada >= oleadaActual.duracionOleada)
            PasarSiguienteOleada();

        if (Time.time >= tiempoSiguienteSpawn)
        {
            SpawnEnemigos();
            tiempoSiguienteSpawn = Time.time + oleadaActual.intervaloSpawn;
        }
    }

    void IniciarOleada(int indice)
    {
        if (oleadas.Count == 0) return;

        indice = indice % oleadas.Count;

        if (indice == 0 && indiceOleadaActual != 0)
        {
            cicloCompleto++;
            multiplicadorDificultad = dificultadBaseDelNivel * Mathf.Pow(1.15f, cicloCompleto);
            Debug.Log($"=== CICLO {cicloCompleto + 1} (INFINITO) === Multiplicador Total: {multiplicadorDificultad:F2}x");
        }

        indiceOleadaActual = indice;
        oleadaActual = oleadas[indice];
        tiempoTranscurridoOleada = 0f;

        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.ActualizarTextoOleada(indiceOleadaActual + 1, oleadas.Count, cicloCompleto);
        }
    }

    void PasarSiguienteOleada()
    {
        IniciarOleada(indiceOleadaActual + 1);
    }

    // --- NUEVO: MÉTODO PARA GENERAR AL JEFE ---
private void SpawnearBoss()
{
    Vector3 posicionSpawn = transform.position; 
    
    if (rendererSuelo != null)
    {
        Bounds limites = rendererSuelo.bounds;
        float x = Random.Range(limites.min.x + margenBorde, limites.max.x - margenBorde);
        float z = Random.Range(limites.min.z + margenBorde, limites.max.z - margenBorde);
        
        // --- AQUÍ ESTÁ EL TRUCO ---
        // Subimos la Y un poco (ajusta el 2.5f según veas que queda mejor)
        float alturaSuelo = transform.position.y + 0.4f; 
        posicionSpawn = new Vector3(x, alturaSuelo, z);
    }

    Instantiate(prefabBoss, posicionSpawn, Quaternion.identity);
}
    // ------------------------------------------

    void SpawnEnemigos()
    {
        if (GameManager.Instancia != null && GameManager.Instancia.enemigosActivos >= limiteMaximoEnemigos) return;
        if (jugador == null || oleadaActual == null || oleadaActual.prefabsEnemigos.Length == 0) return;

        float dificultadTotal = oleadaActual.multiplicadorVida * multiplicadorDificultad;
        int cantidad = oleadaActual.enemigosPorSpawn;

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

            float startY = tieneBounds ? (boundsSuelo.max.y + rayAltura) : (jugador.position.y + rayAltura);
            Vector3 rayStart = new Vector3(spawnPos.x, startY, spawnPos.z);

            RaycastHit hit;
            bool hitOk = false;

            if (groundLayer.value == 0) hitOk = Physics.Raycast(rayStart, Vector3.down, out hit, rayAltura * 2f);
            else hitOk = Physics.Raycast(rayStart, Vector3.down, out hit, rayAltura * 2f, groundLayer);

            if (hitOk) spawnPos.y = hit.point.y + offsetSuelo;
            else spawnPos.y = jugador.position.y;

            GameObject prefab = oleadaActual.prefabsEnemigos[Random.Range(0, oleadaActual.prefabsEnemigos.Length)];
            GameObject enemigo = Instantiate(prefab, spawnPos, Quaternion.identity);

            if (GameManager.Instancia != null) GameManager.Instancia.AgregarEnemigoActivo();

            if (particulaSpawn != null)
            {
                ParticleSystem particulaObj = Instantiate(particulaSpawn, spawnPos, Quaternion.identity);
                float tiempoVidaParticula = particulaObj.main.startLifetime.constantMax;
                Destroy(particulaObj.gameObject, tiempoVidaParticula);

                var colorOverLifetime = particulaObj.colorOverLifetime;
                Gradient grad = new Gradient();
                grad.colorKeys = new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 1f, 1f, 0f), 1f) };
                grad.alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) };
                colorOverLifetime.color = grad;
            }

            // 1. Intentamos escalar si es un Caminante
            EnemigoCaminante scriptCam = enemigo.GetComponent<EnemigoCaminante>();
            if (scriptCam != null)
            {
                scriptCam.vida *= dificultadTotal;
                scriptCam.danoPorContacto = Mathf.RoundToInt(scriptCam.danoPorContacto * dificultadTotal * 0.8f);
                scriptCam.velocidadMovimiento *= oleadaActual.multiplicadorVelocidad;
                scriptCam.velocidadMovimiento = Mathf.Min(scriptCam.velocidadMovimiento, 5.5f);
                scriptCam.experienciaAlMorir = Mathf.RoundToInt(scriptCam.experienciaAlMorir * dificultadTotal);
            }

            // 2. Intentamos escalar si es un Delantero
            EnemigoDelantero scriptDel = enemigo.GetComponent<EnemigoDelantero>();
            if (scriptDel != null)
            {
                scriptDel.vida *= dificultadTotal;
                scriptDel.danoPorContacto = Mathf.RoundToInt(scriptDel.danoPorContacto * dificultadTotal * 0.8f);
                scriptDel.velocidadMovimiento *= oleadaActual.multiplicadorVelocidad;
                scriptDel.velocidadMovimiento = Mathf.Min(scriptDel.velocidadMovimiento, 5.5f);
                scriptDel.experienciaAlMorir = Mathf.RoundToInt(scriptDel.experienciaAlMorir * dificultadTotal);
            }

            // 3. Intentamos escalar si es un Árbitro
            EnemigoArbitro scriptArb = enemigo.GetComponent<EnemigoArbitro>();
            if (scriptArb != null)
            {
                scriptArb.vida *= dificultadTotal;
                scriptArb.danoPorContacto = Mathf.RoundToInt(scriptArb.danoPorContacto * dificultadTotal * 0.8f);
                scriptArb.velocidadMovimiento *= oleadaActual.multiplicadorVelocidad;
                scriptArb.velocidadMovimiento = Mathf.Min(scriptArb.velocidadMovimiento, 5.5f);
                scriptArb.experienciaAlMorir = Mathf.RoundToInt(scriptArb.experienciaAlMorir * dificultadTotal);
                
                scriptArb.danoAmarilla = Mathf.RoundToInt(scriptArb.danoAmarilla * dificultadTotal * 0.8f);
                scriptArb.danoRoja = Mathf.RoundToInt(scriptArb.danoRoja * dificultadTotal * 0.8f);
            }
        } 
    } 
}