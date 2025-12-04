using UnityEngine;

public class Bala : MonoBehaviour
{
    private float velocidad;
    private float dano;
    public float tiempoVida = 2f; // La bala vive 2 segundos

    private float tiempoActual = 0f;
    private bool colisionDetectada = false;

    void Start()
    {
        ActualizarStats();
        
    }
    void Update()
    {
        ActualizarStats();
        transform.position += transform.forward * velocidad * Time.deltaTime;
        // Solo mover si no hemos colisionado
        if (!colisionDetectada)
        {
            transform.Translate(Vector3.forward * velocidad * Time.deltaTime);
        }

        // Aumentar tiempo actual
        tiempoActual += Time.deltaTime;

        // Si pasa el tiempo de vida, destruir la bala
        if (tiempoActual >= tiempoVida)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemies"))
        {
            EnemigoCaminante enemigo = other.GetComponent<EnemigoCaminante>();
            if (enemigo != null)
            {
                enemigo.RecibirDano(dano);
                
            }
            Destroy(gameObject); // Destruye normalmente al impactar con un enemigo
        }
        else if (!other.CompareTag("Player"))
        {
            // Solo marca la colisión, pero no destruye la bala hasta que el timer expire
            colisionDetectada = true;
        }
    }

    void ActualizarStats()
    {
        PlayerStats stats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
        if (stats != null)
        {
            velocidad = stats.velocidadProyectil;
            dano = stats.danoProyectil;
        }
    }
}
