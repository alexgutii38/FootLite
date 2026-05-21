using CartoonFX;
using UnityEngine;

public class EnemigoCaminante : MonoBehaviour, IDamageable
{
    [Header("Drops (Botín)")]
    public GameObject prefabGemaExperiencia;
    public GameObject prefabCofre;
    [Range(0f, 1f)] public float probabilidadGema  = 0.8f;
    [Range(0f, 1f)] public float probabilidadCofre = 0.05f;

    [Header("Atributos escalables")]
    public float vida = 50f;
    public float velocidadMovimiento = 3f;
    public int   experienciaAlMorir  = 20;
    public int   experienciaPorGolpe = 5;

    public int   danoPorContacto  = 10;
    public float tiempoEntreDanos = 0.5f;
    public float rangoDeteccion   = 50f;
    [Tooltip("Distancia a la que el enemigo se planta para atacar en lugar de atravesar al jugador.")]
    public float distanciaAtaque  = 1.2f;
    [Tooltip("Radio de separación con otros enemigos para que no se monten encima.")]
    public float radioSeparacion  = 0.75f;
    [Tooltip("Fuerza con la que se separan entre sí.")]
    public float fuerzaSeparacion = 0.25f;

    [Header("Futbol FX")]
    public ParticleSystem efectoMuerte;
    public ParticleSystem prefabTextoDaño;

    private Transform jugador;
    private float     tiempoUltimoDano;
    private Rigidbody rb;
    public  Animator  animator;

    // Altura inicial bloqueada: el juego es 2.5D top-down, los enemigos
    // jamás deben subir ni bajar (ni atravesar al jugador en vertical).
    private float yInicial;
    private bool  yInicialCapturada = false;

    // Perf: layer cacheado, timer para separación
    private LayerMask enemiesLayer;
    private float     separacionTimer;

    // Buffer estático reutilizable: evita asignar un array en cada OverlapSphere
    private static readonly Collider[] bufferVecinos = new Collider[16];

    // Efecto de impacto (se añade solo en runtime)
    private ParpadeoDano parpadeo;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) jugador = p.transform;

        rb = GetComponent<Rigidbody>();
        enemiesLayer = LayerMask.GetMask("Enemies");

        if (rb != null)
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

        if (animator != null)
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

        parpadeo = GetComponent<ParpadeoDano>();
        if (parpadeo == null) parpadeo = gameObject.AddComponent<ParpadeoDano>();

        yInicial = transform.position.y;
        yInicialCapturada = true;
    }

    void LateUpdate()
    {
        if (!yInicialCapturada) return;
        if (transform.position.y == yInicial) return;
        Vector3 p = transform.position;
        p.y = yInicial;
        transform.position = p;
    }

    void Update()
    {
        if (jugador == null) return;

        float distancia = Vector3.Distance(transform.position, jugador.position);
        bool  enRango   = distancia <= rangoDeteccion;

        bool enContacto = distancia <= distanciaAtaque;

        if (enRango)
        {
            Vector3 direccion = (jugador.position - transform.position).normalized;

            if (direccion != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(direccion), Time.deltaTime * 5f);

            // Si ya está pegado al jugador, se planta a atacar (no atraviesa).
            if (enContacto)
            {
                if (rb != null)
                    rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            }
            else if (rb != null)
                rb.linearVelocity = new Vector3(direccion.x * velocidadMovimiento,
                    rb.linearVelocity.y, direccion.z * velocidadMovimiento);
            else
                transform.position += direccion * velocidadMovimiento * Time.deltaTime;
        }
        else
        {
            if (rb != null)
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }

        if (animator != null)
            animator.SetFloat("speed", (enRango && !enContacto) ? velocidadMovimiento : 0f, 0.1f, Time.deltaTime);

        // Separación: solo cada 0.1 s (10 veces/seg en vez de 60)
        separacionTimer -= Time.deltaTime;
        if (separacionTimer <= 0f)
        {
            separacionTimer = 0.1f;

            int numVecinos = Physics.OverlapSphereNonAlloc(transform.position, radioSeparacion, bufferVecinos, enemiesLayer);
            for (int i = 0; i < numVecinos; i++)
            {
                Collider col = bufferVecinos[i];
                if (col == null || col.gameObject == gameObject) continue;
                // sep.y = 0 obligatorio: si no, los enemigos se empujan
                // verticalmente y acaban flotando uno encima del otro.
                Vector3 sep = transform.position - col.transform.position;
                sep.y = 0f;
                if (sep.sqrMagnitude < 0.0001f) continue;
                sep.Normalize();
                transform.position += sep * fuerzaSeparacion;
            }

            // Empuje extra cuando está pegado al jugador, sin componente vertical.
            if (distancia < distanciaAtaque)
            {
                Vector3 dir = transform.position - jugador.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.0001f)
                {
                    dir.Normalize();
                    transform.position += dir * 0.15f;
                }
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        PlayerStats ps = other.GetComponent<PlayerStats>();
        if (ps != null && Time.time > tiempoUltimoDano + tiempoEntreDanos)
        {
            ps.RecibirDano(danoPorContacto);
            tiempoUltimoDano = Time.time;
        }
    }

    public void RecibirDano(float cantidad)
    {
        if (prefabTextoDaño != null)
        {
            ParticleSystem textObj = Instantiate(prefabTextoDaño,
                transform.position + Vector3.up * 2f, Quaternion.identity);
            CFXR_ParticleText script = textObj.GetComponent<CFXR_ParticleText>();
            if (script != null) script.MostrarValorDaño(cantidad);
        }

        vida -= cantidad;
        if (parpadeo != null) parpadeo.Flash();

        if (jugador != null)
        {
            Vector3 dir = (transform.position - jugador.position).normalized;
            dir.y = 0f;
            transform.position += dir * 0.25f;
        }

        if (vida <= 0f)
        {
            if (GameManager.Instancia != null) GameManager.Instancia.SumarEliminado();
            AudioManager.Instancia?.SonarMuerteEnemigo();

            if (prefabCofre != null && Random.value <= probabilidadCofre)
                Instantiate(prefabCofre, transform.position + Vector3.up * 0.3f, Quaternion.identity);
            else if (prefabGemaExperiencia != null && Random.value <= probabilidadGema)
            {
                GameObject gema = ObjectPool.Instancia.Obtener(prefabGemaExperiencia,
                    transform.position + Vector3.up * 0.3f, Quaternion.identity);
                GemaExperiencia sg = gema != null ? gema.GetComponent<GemaExperiencia>() : null;
                if (sg != null) sg.cantidadExperiencia = experienciaAlMorir;
            }

            if (efectoMuerte != null)
                Instantiate(efectoMuerte, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}
