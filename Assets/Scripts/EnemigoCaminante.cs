using UnityEngine;

public class EnemigoCaminante : MonoBehaviour
{
    public int danoPorContacto = 10;           // Daño que causa al jugador
    public float tiempoEntreDanos = 1f;        // Tiempo entre daños para que no quite vida continuamente sin pausa
    public float velocidadMovimiento = 3f;    // Velocidad a la que se mueve el enemigo
    public float rangoDeteccion = 50f;        // Rango de detección para empezar a perseguir

    private Transform jugador;
    private float tiempoUltimoDano;
    private Rigidbody rb;

    void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody>();
        // Opcional: Si no quieres usar física para el movimiento
        // Puedes hacerlo con transform.position += ...
    }

    void Update()
    {
        if (jugador == null) return;

        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia <= rangoDeteccion)
        {
            // Movimiento hacia el jugador
            Vector3 direccion = (jugador.position - transform.position).normalized;

            // Rotar para mirar al jugador
            if (direccion != Vector3.zero)
            {
                Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, Time.deltaTime * 5f);
            }

            // Mover
            if (rb != null)
                rb.linearVelocity = new Vector3(direccion.x * velocidadMovimiento, rb.linearVelocity.y, direccion.z * velocidadMovimiento);
            else
                transform.position += direccion * velocidadMovimiento * Time.deltaTime;
        }
        else
        {
            // Detenerse si no está en rango
            if (rb != null)
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null && Time.time > tiempoUltimoDano + tiempoEntreDanos)
            {
                playerStats.RecibirDano(danoPorContacto);
                tiempoUltimoDano = Time.time;
            }
        }
    }
}
