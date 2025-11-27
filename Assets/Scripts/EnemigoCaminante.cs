using UnityEngine;


public class EnemigoCaminante : MonoBehaviour
{
    // ==== NUEVO ====
    [Header("Atributos escalables")]
    public float vida = 50f;
    public float velocidadMovimiento = 3f;
    public int experienciaAlMorir = 20;

    public int experienciaPorGolpe = 5;
    // ===============

    public int danoPorContacto = 10;
    public float tiempoEntreDanos = 0.5f;
    public float rangoDeteccion = 50f;


    private float alturaInicial;
    private Transform jugador;
    private float tiempoUltimoDano;
    private Rigidbody rb;

    void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody>();
        alturaInicial = transform.position.y;
    }

    void Update()
    {
        if (jugador == null) return;

        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia <= rangoDeteccion)
        {
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
            if (rb != null)
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }

        // Separación y altura mantienen igual...
        Collider[] vecinos = Physics.OverlapSphere(transform.position, 0.25f, LayerMask.GetMask("Enemies"));
        foreach (Collider col in vecinos)
        {
            if (col.gameObject != this.gameObject)
            {
                Vector3 separacion = (transform.position - col.transform.position).normalized;
                transform.position += separacion * 0.15f;
            }
        }

        if (jugador != null)
        {
            float distaciaJugador = Vector3.Distance(transform.position, jugador.position);
            float distanciaMinima = 1f;
            if (distaciaJugador < distanciaMinima)
            {
                Vector3 direccionSeparacion = (transform.position - jugador.position).normalized;
                transform.position += direccionSeparacion * 0.15f;
            }
        }

        // Mantener altura constante
        Vector3 posicionY = transform.position;
        posicionY.y = alturaInicial;
        transform.position = posicionY;
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

    // ==== NUEVO ====
    // Recibir daño y muerte
    public void RecibirDano(float cantidad)
    {
        // 1. XP por golpe
        PlayerStats ps = FindFirstObjectByType<PlayerStats>();
        if (ps != null)
        {
            ps.GanarExperiencia(experienciaPorGolpe);
        }

        // 2. Aplicar daño a la vida del enemigo
        vida -= cantidad;

        // 3. Si muere, XP extra por kill
        if (vida <= 0f)
        {
            GameManager.Instancia.SumarEliminado();

            if (ps != null)
            {
                ps.GanarExperiencia(experienciaAlMorir);
            }

            Destroy(gameObject);
        }
    }
}
