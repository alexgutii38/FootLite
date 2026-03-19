using CartoonFX;
using UnityEngine;


public class EnemigoCaminante : MonoBehaviour, IDamageable
{

    [Header("Drops (Botín)")]
    public GameObject prefabGemaExperiencia;
    public GameObject prefabCofre;
    [Range(0f, 1f)] public float probabilidadGema = 0.8f;   // 80% de soltar gema
    [Range(0f, 1f)] public float probabilidadCofre = 0.05f; // 5% de soltar cofre
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

    [Header("Futbol FX")]

    public ParticleSystem efectoMuerte;
    public ParticleSystem prefabTextoDaño;

    //public AudioClip sonidoGolpe; Para el audio

    private float alturaInicial;
    private Transform jugador;
    private float tiempoUltimoDano;
    private Rigidbody rb;
    public Animator animator;



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

        animator.SetFloat("speed", velocidadMovimiento, 0.1f, Time.deltaTime);

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
        animator.SetFloat("speed", velocidadMovimiento, 0.1f, Time.deltaTime);
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
        // 1. Mostrar texto de daño
        if (prefabTextoDaño != null)
        {
            ParticleSystem textObj = Instantiate(prefabTextoDaño, transform.position + Vector3.up * 2f, Quaternion.identity);
            CFXR_ParticleText scriptTexto = textObj.GetComponent<CFXR_ParticleText>();
            if (scriptTexto != null)
                scriptTexto.MostrarValorDaño(cantidad);
        }

        // 2. Restar vida
        vida -= cantidad;

        // --- EFECTO KNOCKBACK (Peso de las balas) ---
        GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
        if (jugadorObj != null)
        {
            Vector3 direccionEmpuje = (transform.position - jugadorObj.transform.position).normalized;
            direccionEmpuje.y = 0f; // Evitamos que salgan volando hacia arriba
            
            // Les damos un pequeño empujón de 25 centímetros hacia atrás
            transform.position += direccionEmpuje * 0.25f; 
        }
        // --------------------------------------------

        // 3. Morir y soltar objetos
        if (vida <= 0f)
        {
            if (GameManager.Instancia != null)
                GameManager.Instancia.SumarEliminado();

            // Soltar Cofre o Gema a la altura correcta (0.3f)
            if (prefabCofre != null && Random.value <= probabilidadCofre)
            {
                Instantiate(prefabCofre, transform.position + Vector3.up * 0.3f, Quaternion.identity);
            }
            else if (prefabGemaExperiencia != null && Random.value <= probabilidadGema)
            {
                GameObject gema = Instantiate(prefabGemaExperiencia, transform.position + Vector3.up * 0.3f, Quaternion.identity);
                GemaExperiencia scriptGema = gema.GetComponent<GemaExperiencia>();
                if (scriptGema != null)
                    scriptGema.cantidadExperiencia = experienciaAlMorir;
            }

            if (efectoMuerte != null)
                Instantiate(efectoMuerte, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }
    private void OnDestroy()
    {
        // Al destruirse (por morir o al cambiar de escena), se resta del contador global
        if (GameManager.Instancia != null)
        {
            GemaManager.Instance.GenerarGemas(transform.position);
        }
    }
}
