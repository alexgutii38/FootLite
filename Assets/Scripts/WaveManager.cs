using UnityEngine;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [Header("FX")]
    public ParticleSystem particulaSpawn;

    [Header("Límites de spawn")]
    public Renderer rendererSuelo;     // Arrastra aquí el suelo/campo (MeshRenderer o cualquier Renderer)
    public float margenBorde = 1f;     // Para que no aparezca pegado a la pared

    [Header("Suelo")]
    public LayerMask groundLayer;      // Layer del suelo (Ground)
    public float rayAltura = 50f;      // Desde cuánta altura lanzamos el rayo
    public float offsetSuelo = 0.05f;  // Pequeño offset para que no “clave” el collider

    [Header("Base de datos")]
    public List<WaveConfig> oleadas;

    [Header("Referencias")]
    public Transform jugador;
    public float radioSpawn = 12f;

    [Header("Seguridad")]
    public int limiteMaximoEnemigos = 100;

    private int indiceOleadaActual = 0;
    private float tiempoTranscurridoOleada = 0f;
    private float tiempoSiguienteSpawn = 0f;
    private WaveConfig oleadaActual;

    // --- VARIABLES DE ESCALADO ---
    private int cicloCompleto = 0;
    private float multiplicadorDificultad = 1.0f;

    void Start()
    {
        if (jugador == null)
            jugador = GameObject.FindGameObjectWithTag("Player").transform;

        IniciarOleada(0);
    }

    void Update()
    {
        if (oleadaActual == null) return;

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
        indice = indice % oleadas.Count;

        if (indice == 0 && indiceOleadaActual != 0)
        {
            cicloCompleto++;
            multiplicadorDificultad = Mathf.Pow(1.15f, cicloCompleto);
            Debug.Log($"=== CICLO {cicloCompleto + 1} === Multiplicador: {multiplicadorDificultad:F2}x");
        }

        indiceOleadaActual = indice;
        oleadaActual = oleadas[indice];
        tiempoTranscurridoOleada = 0f;

        Debug.Log($"Oleada {indice + 1}/{oleadas.Count} (Ciclo {cicloCompleto + 1})");
    }

    void PasarSiguienteOleada()
    {
        IniciarOleada(indiceOleadaActual + 1);
    }

    void SpawnEnemigos()
    {
        if (GameObject.FindGameObjectsWithTag("Enemigo").Length >= limiteMaximoEnemigos)
            return;

        if (jugador == null || oleadaActual == null || oleadaActual.prefabsEnemigos.Length == 0)
            return;

        float dificultadTotal = oleadaActual.multiplicadorVida * multiplicadorDificultad;
        int cantidad = oleadaActual.enemigosPorSpawn;

        // Bounds del suelo (en mundo). Si no hay suelo asignado, spawnea relativo al jugador sin clamp.
        Bounds boundsSuelo = new Bounds();
        bool tieneBounds = false;

        if (rendererSuelo != null)
        {
            boundsSuelo = rendererSuelo.bounds;
            tieneBounds = true;
        }

        for (int i = 0; i < cantidad; i++)
        {
            // 1) Posición candidata en XZ alrededor del jugador
            Vector2 puntoRandom = Random.insideUnitCircle.normalized * radioSpawn;
            Vector3 spawnPos = jugador.position + new Vector3(puntoRandom.x, 0f, puntoRandom.y);

            // 2) Clamp dentro del suelo (solo X/Z)
            if (tieneBounds)
            {
                spawnPos.x = Mathf.Clamp(spawnPos.x, boundsSuelo.min.x + margenBorde, boundsSuelo.max.x - margenBorde);
                spawnPos.z = Mathf.Clamp(spawnPos.z, boundsSuelo.min.z + margenBorde, boundsSuelo.max.z - margenBorde);
            }

            // 3) Raycast para calcular la Y real del suelo en ese X/Z
            float startY = tieneBounds ? (boundsSuelo.max.y + rayAltura) : (jugador.position.y + rayAltura);
            Vector3 rayStart = new Vector3(spawnPos.x, startY, spawnPos.z);

            RaycastHit hit;
            bool hitOk = false;

            // Si groundLayer está en 0 (Nothing), raycastea contra todo.
            if (groundLayer.value == 0)
                hitOk = Physics.Raycast(rayStart, Vector3.down, out hit, rayAltura * 2f);
            else
                hitOk = Physics.Raycast(rayStart, Vector3.down, out hit, rayAltura * 2f, groundLayer);

            if (hitOk)
                spawnPos.y = hit.point.y + offsetSuelo;
            else
                spawnPos.y = jugador.position.y; // fallback si no encuentra suelo

            // 4) Instanciar enemigo
            GameObject prefab = oleadaActual.prefabsEnemigos[Random.Range(0, oleadaActual.prefabsEnemigos.Length)];
            GameObject enemigo = Instantiate(prefab, spawnPos, Quaternion.identity);

            // Partícula de spawn
            if (particulaSpawn != null)
            {
                ParticleSystem particulaObj = Instantiate(particulaSpawn, spawnPos, Quaternion.identity);
                float tiempoVidaParticula = particulaObj.main.startLifetime.constantMax;
                Destroy(particulaObj.gameObject, tiempoVidaParticula);

                var colorOverLifetime = particulaObj.colorOverLifetime;
                Gradient grad = new Gradient();
                grad.colorKeys = new GradientColorKey[] {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(new Color(1f, 1f, 1f, 0f), 1f)
                };
                grad.alphaKeys = new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                };
                colorOverLifetime.color = grad;
            }

            // Escalado stats
            EnemigoCaminante script = enemigo.GetComponent<EnemigoCaminante>();
            if (script != null)
            {
                script.vida *= dificultadTotal;
                script.danoPorContacto = Mathf.RoundToInt(script.danoPorContacto * dificultadTotal * 0.8f);
                script.velocidadMovimiento *= oleadaActual.multiplicadorVelocidad;
                script.velocidadMovimiento = Mathf.Min(script.velocidadMovimiento, 5.5f);
                script.experienciaAlMorir = Mathf.RoundToInt(script.experienciaAlMorir * dificultadTotal);
            }
        }
    }
}
