using UnityEngine;

public class GeneradorEnemigos : MonoBehaviour
{
    [Header("Prefabs y parámetros base")]
    public GameObject enemigoPrefab;
    public float radioGeneracion = 12f;

    [Header("Spawn y progresión")]
    public int cantidadBase = 2;
    public float intervaloBase = 12f;
    public float intervaloMin = 2.3f;
    public int incrementoEnemigos = 1;
    public float reduccionIntervalo = 0.55f;

    [Header("Escalado vida y velocidad")]
    public float vidaBase = 50f;
    public float vidaPorMinuto = 30f;
    public float velocidadBase = 3f;
    public float velocidadPorMinuto = 1f;

    private float intervaloActual;
    private int cantidadActual;
    private Transform jugador;
    private float tiempoUltimaGeneracion;

    void Start()
    {
        
        jugador = GameObject.FindGameObjectWithTag("Player").transform;
        cantidadActual = cantidadBase;
        intervaloActual = intervaloBase;
        GenerarOleada();
        tiempoUltimaGeneracion = Time.time;
    }

    void Update()
    {
        if (Time.time > tiempoUltimaGeneracion + intervaloActual)
        {
            GenerarOleada();
            tiempoUltimaGeneracion = Time.time;
            cantidadActual += incrementoEnemigos;
            intervaloActual -= reduccionIntervalo;
            if (intervaloActual < intervaloMin)
                intervaloActual = intervaloMin;
        }
    }

    void GenerarOleada()
    {
        float minutos = Time.timeSinceLevelLoad / 60f;
        float vidaEscalada = vidaBase + vidaPorMinuto * minutos;
        float velocidadEscalada = velocidadBase + velocidadPorMinuto * minutos;

        for (int i = 0; i < cantidadActual; i++)
        {
            float angulo = Random.Range(0, 360f) * Mathf.Deg2Rad;
            float x = jugador.position.x + radioGeneracion * Mathf.Cos(angulo);
            float z = jugador.position.z + radioGeneracion * Mathf.Sin(angulo);
            Vector3 posicion = new Vector3(x, 1.2f, z);

            GameObject enemigo = Instantiate(enemigoPrefab, posicion, Quaternion.identity);

            // Pasa stats al componente EnemigoCaminante
            EnemigoCaminante script = enemigo.GetComponent<EnemigoCaminante>();
            if (script != null)
            {
                script.vida = vidaEscalada;
                script.velocidadMovimiento = velocidadEscalada;
            }
        }
    }
}