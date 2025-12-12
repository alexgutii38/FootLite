using UnityEngine;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [Header("FX")]
    public ParticleSystem particulaSpawn;


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
    private int cicloCompleto = 0; // Cuántas veces hemos pasado por todas las oleadas
    private float multiplicadorDificultad = 1.0f; // Aumenta cada ciclo

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

        // Cuando termina una oleada, pasar a la siguiente
        if (tiempoTranscurridoOleada >= oleadaActual.duracionOleada)
        {
            PasarSiguienteOleada();
        }

        // Lógica de spawn
        if (Time.time >= tiempoSiguienteSpawn)
        {
            SpawnEnemigos();
            tiempoSiguienteSpawn = Time.time + oleadaActual.intervaloSpawn;
        }
    }

    void IniciarOleada(int indice)
    {
        // BUCLE: Si llegamos al final, volvemos al principio
        indice = indice % oleadas.Count;
       
        // Si pasamos de oleada y volvemos a la 0, aumentamos el ciclo
        if (indice == 0 && indiceOleadaActual != 0)
        {
            cicloCompleto++;
            multiplicadorDificultad = Mathf.Pow(1.15f, cicloCompleto); // +15% dificultad cada ciclo completo
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
        // LÍMITE DE SEGURIDAD
        if (GameObject.FindGameObjectsWithTag("Enemigo").Length >= limiteMaximoEnemigos)
            return;

        if (jugador == null || oleadaActual.prefabsEnemigos.Length == 0) return;

        // ESCALADO: Cada ciclo completo suma un +15%
        float dificultadTotal = oleadaActual.multiplicadorVida * multiplicadorDificultad;

        int cantidad = oleadaActual.enemigosPorSpawn;

        for (int i = 0; i < cantidad; i++)
        {
            Vector2 puntoRandom = Random.insideUnitCircle.normalized * radioSpawn;
            Vector3 spawnPos = jugador.position + new Vector3(puntoRandom.x, -0.5f, puntoRandom.y);

            GameObject prefab = oleadaActual.prefabsEnemigos[Random.Range(0, oleadaActual.prefabsEnemigos.Length)];
            GameObject enemigo = Instantiate(prefab, spawnPos, Quaternion.identity);

            if(particulaSpawn != null)
            {
                ParticleSystem particulaObj = Instantiate(particulaSpawn, spawnPos, Quaternion.identity);
                Destroy(particulaObj, particulaObj.main.duration);
            }

            EnemigoCaminante script = enemigo.GetComponent<EnemigoCaminante>();
            if (script != null)
            {
                // VIDA: Escala con ciclo
                script.vida *= dificultadTotal;

                // DAÑO: Escala un poco menos
                script.danoPorContacto = Mathf.RoundToInt(script.danoPorContacto * dificultadTotal * 0.8f);

                // VELOCIDAD: Tope para no ser injusto
                script.velocidadMovimiento *= oleadaActual.multiplicadorVelocidad;
                script.velocidadMovimiento = Mathf.Min(script.velocidadMovimiento, 5.5f);

                // XP: Más dura = más recompensa
                script.experienciaAlMorir = Mathf.RoundToInt(script.experienciaAlMorir * dificultadTotal);
            }
        }
    }
}


