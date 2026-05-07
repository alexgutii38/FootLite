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

    [Header("Futbol FX")]
    public ParticleSystem efectoMuerte;
    public ParticleSystem prefabTextoDaño;

    private Transform jugador;
    private float     tiempoUltimoDano;
    private Rigidbody rb;
    public  Animator  animator;

    // Perf: layer cacheado, timer para separación
    private LayerMask enemiesLayer;
    private float     separacionTimer;

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
    }

    void Update()
    {
        if (jugador == null) return;

        float distancia = Vector3.Distance(transform.position, jugador.position);
        bool  enRango   = distancia <= rangoDeteccion;

        if (enRango)
        {
            Vector3 direccion = (jugador.position - transform.position).normalized;

            if (direccion != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(direccion), Time.deltaTime * 5f);

            if (rb != null)
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
            animator.SetFloat("speed", enRango ? velocidadMovimiento : 0f, 0.1f, Time.deltaTime);

        // Separación: solo cada 0.1 s (10 veces/seg en vez de 60)
        separacionTimer -= Time.deltaTime;
        if (separacionTimer <= 0f)
        {
            separacionTimer = 0.1f;

            Collider[] vecinos = Physics.OverlapSphere(transform.position, 0.25f, enemiesLayer);
            foreach (Collider col in vecinos)
            {
                if (col.gameObject == gameObject) continue;
                Vector3 sep = (transform.position - col.transform.position).normalized;
                transform.position += sep * 0.15f;
            }

            if (distancia < 1f)
            {
                Vector3 dir = (transform.position - jugador.position).normalized;
                transform.position += dir * 0.15f;
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

        if (jugador != null)
        {
            Vector3 dir = (transform.position - jugador.position).normalized;
            dir.y = 0f;
            transform.position += dir * 0.25f;
        }

        if (vida <= 0f)
        {
            if (GameManager.Instancia != null) GameManager.Instancia.SumarEliminado();

            if (prefabCofre != null && Random.value <= probabilidadCofre)
                Instantiate(prefabCofre, transform.position + Vector3.up * 0.3f, Quaternion.identity);
            else if (prefabGemaExperiencia != null && Random.value <= probabilidadGema)
            {
                GameObject gema = Instantiate(prefabGemaExperiencia,
                    transform.position + Vector3.up * 0.3f, Quaternion.identity);
                GemaExperiencia sg = gema.GetComponent<GemaExperiencia>();
                if (sg != null) sg.cantidadExperiencia = experienciaAlMorir;
            }

            if (efectoMuerte != null)
                Instantiate(efectoMuerte, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}
