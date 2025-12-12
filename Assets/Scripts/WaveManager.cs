using UnityEngine;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [Header("Secuencia de Oleadas")]
    public List<WaveConfig> oleadas; // Arrastra aquí Oleada_Minuto1, Oleada_Minuto2...
   
    [Header("Referencias")]
    public Transform jugador;
    public float radioSpawn = 12f;

    private int indiceOleadaActual = 0;
    private float tiempoTranscurridoOleada = 0f;
    private float tiempoSiguienteSpawn = 0f;

    // Estado actual (copia de la config para no machacar el asset)
    private WaveConfig oleadaActual;

    [Header("Seguridad")]
    public int limiteMaximoEnemigos = 100;

    void Start()
    {
        if (jugador == null)
            jugador = GameObject.FindGameObjectWithTag("Player").transform;

        IniciarOleada(0);
    }

    void Update()
    {
        if (oleadaActual == null) return;

        // Control de tiempo de oleada
        tiempoTranscurridoOleada += Time.deltaTime;
       
        if (tiempoTranscurridoOleada >= oleadaActual.duracionOleada)
        {
            PasarSiguienteOleada();
        }

        // Lógica de Spawn
        if (Time.time >= tiempoSiguienteSpawn)
        {
            SpawnEnemigos();
            tiempoSiguienteSpawn = Time.time + oleadaActual.intervaloSpawn;
        }
    }

    void IniciarOleada(int indice)
    {
        if (indice >= oleadas.Count)
        {
            Debug.Log("Fin de las oleadas - Bucle de la última o evento de victoria");
            // Opción: Repetir la última oleada infinitamente pero más difícil
            indice = oleadas.Count - 1;
        }

        indiceOleadaActual = indice;
        oleadaActual = oleadas[indice];
        tiempoTranscurridoOleada = 0f;
       
        Debug.Log($"Iniciando Oleada {indice + 1}");
    }

    void PasarSiguienteOleada()
    {
        IniciarOleada(indiceOleadaActual + 1);
    }

    void SpawnEnemigos()
    {
        // Control de poblacion
        int enemigosActuales = GameObject.FindGameObjectsWithTag("Enemigo").Length;
        if (enemigosActuales >= limiteMaximoEnemigos)
        {
            return;
        }

        if (jugador == null || oleadaActual.prefabsEnemigos.Length == 0) return;

        for (int i = 0; i < oleadaActual.enemigosPorSpawn; i++)
        {
            // 1. Posición aleatoria alrededor del jugador
            Vector2 puntoRandom = Random.insideUnitCircle.normalized * radioSpawn;
            Vector3 spawnPos = jugador.position + new Vector3(puntoRandom.x, -0.5f, puntoRandom.y);

            // 2. Elegir enemigo al azar de la lista de esta oleada
            GameObject prefab = oleadaActual.prefabsEnemigos[Random.Range(0, oleadaActual.prefabsEnemigos.Length)];

            // 3. Instanciar
            GameObject enemigo = Instantiate(prefab, spawnPos, Quaternion.identity);

            // 4. Aplicar Dificultad (Balanceo)
            EnemigoCaminante script = enemigo.GetComponent<EnemigoCaminante>();
            if (script != null)
            {
                script.vida *= oleadaActual.multiplicadorVida;
                script.velocidadMovimiento *= oleadaActual.multiplicadorVelocidad;
                script.experienciaAlMorir = oleadaActual.experienciaBase;
                // Ajustar XP por golpe proporcionalmente si quieres
                script.experienciaPorGolpe = Mathf.Max(1, oleadaActual.experienciaBase / 5);
            }
        }
    }
}


