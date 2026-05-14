using UnityEngine;
using CartoonFX;

public class EnemigoArbitro : MonoBehaviour, IDamageable
{
    [Header("Drops (Botín)")]
    public GameObject prefabGemaExperiencia;
    public GameObject prefabCofre;
    [Range(0f, 1f)] public float probabilidadGema  = 0.8f;
    [Range(0f, 1f)] public float probabilidadCofre = 0.05f;

    [Header("Stats")]
    public float vida = 40f;
    public float velocidadMovimiento = 2.4f;
    public int   danoPorContacto     = 6;
    public float tiempoEntreDanos    = 0.6f;
    public float rangoDeteccion      = 50f;
    public int   experienciaAlMorir  = 25;

    [Header("Distancia de seguridad")]
    public float distanciaSeguridad = 4f;
    public float histeresis         = 0.25f;

    [Header("Disparo tarjeta (no guiada)")]
    public GameObject prefabTarjeta;
    public Transform  puntoDisparo;
    public float      rangoDisparo         = 8f;
    public float      cooldownDisparo      = 1.5f;
    public float      distanciaMinimaDisparo = 2.5f;

    [Header("Tarjetas amarilla/roja")]
    [Range(0f, 1f)] public float probabilidadRoja = 0.35f;
    public int   danoAmarilla  = 7;
    public int   danoRoja      = 12;
    public Color colorAmarilla = Color.yellow;
    public Color colorRoja     = Color.red;

    [Header("Movimiento inteligente")]
    public bool  orbitarEnRango      = true;
    public float velocidadOrbita     = 1.1f;
    public float fuerzaCorreccionRadio = 2.0f;
    public float velocidadRetroceso  = 1.2f;

    [Header("Separación entre árbitros")]
    public LayerMask layerEnemigos;
    public float radioSeparacion  = 1.2f;
    public float fuerzaSeparacion = 2.5f;

    [Header("Variación ángulos")]
    public float         cambioLadoCada  = 2.5f;
    [Range(0f, 1f)] public float probCambioLado = 0.35f;

    [Header("Animación")]
    public Animator animator;

    [Header("FX opcional")]
    public ParticleSystem efectoMuerte;
    public ParticleSystem prefabTextoDaño;

    private Transform jugador;
    private float     tiempoUltimoDano;
    private float     tiempoUltimoDisparo;
    private Rigidbody rb;
    private int       strafeSign = 1;
    private float     tCambioLado;

    // Perf: separación cacheada, timer, alturaInicial eliminada (Rigidbody FreezeY)
    private Vector3 separacionCacheada = Vector3.zero;
    private float   separacionTimer;

    // Buffer estático reutilizable: evita asignar un array en cada OverlapSphere
    private static readonly Collider[] bufferSeparacion = new Collider[32];

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) jugador = p.transform;

        rb = GetComponent<Rigidbody>();

        if (rb != null)
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

        if (animator != null)
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

        strafeSign  = (Random.value < 0.5f) ? -1 : 1;
        tCambioLado = Time.time + Random.Range(0f, cambioLadoCada);
    }

    private void Update()
    {
        if (jugador == null) return;

        if (orbitarEnRango && cambioLadoCada > 0f && Time.time >= tCambioLado)
        {
            if (Random.value < probCambioLado) strafeSign *= -1;
            tCambioLado = Time.time + cambioLadoCada;
        }

        Vector3 toPlayer = jugador.position - transform.position;
        toPlayer.y = 0f;
        float dist = toPlayer.magnitude;
        if (dist < 0.001f) return;

        Vector3 dirToPlayer    = toPlayer / dist;
        bool    enRangoDeteccion = dist <= rangoDeteccion;

        transform.rotation = Quaternion.Slerp(transform.rotation,
            Quaternion.LookRotation(dirToPlayer), Time.deltaTime * 8f);

        Vector3 velDeseada = Vector3.zero;

        if (enRangoDeteccion && dist > (distanciaSeguridad + histeresis))
            velDeseada = dirToPlayer * velocidadMovimiento;
        else if (enRangoDeteccion && dist < (distanciaSeguridad - histeresis))
            velDeseada = (-dirToPlayer) * velocidadRetroceso;
        else if (enRangoDeteccion && orbitarEnRango)
        {
            Vector3 tangent   = Vector3.Cross(Vector3.up, dirToPlayer).normalized * strafeSign;
            float   errorRadio = dist - distanciaSeguridad;
            Vector3 correccion = dirToPlayer * (-errorRadio * fuerzaCorreccionRadio);
            velDeseada = tangent * (velocidadMovimiento * velocidadOrbita) + correccion;
        }

        // Separación: recalcular solo cada 0.1 s
        separacionTimer -= Time.deltaTime;
        if (separacionTimer <= 0f)
        {
            separacionTimer    = 0.1f;
            separacionCacheada = CalcularSeparacion();
        }
        velDeseada += separacionCacheada;

        Vector3 velPlano = new Vector3(velDeseada.x, 0f, velDeseada.z);
        float maxSpeed   = Mathf.Max(0.1f, velocidadMovimiento * 1.35f);
        if (velPlano.magnitude > maxSpeed) velPlano = velPlano.normalized * maxSpeed;

        if (rb != null)
            rb.linearVelocity = new Vector3(velPlano.x, rb.linearVelocity.y, velPlano.z);
        else
            transform.position += velPlano * Time.deltaTime;

        if (animator != null)
            animator.SetFloat("speed", enRangoDeteccion ? velPlano.magnitude : 0f, 0.1f, Time.deltaTime);

        bool enRangoDisparo = dist <= rangoDisparo && dist >= distanciaMinimaDisparo;
        if (enRangoDisparo && Time.time >= tiempoUltimoDisparo + cooldownDisparo)
        {
            DispararTarjeta();
            tiempoUltimoDisparo = Time.time;
        }
    }

    private Vector3 CalcularSeparacion()
    {
        if (radioSeparacion <= 0f || fuerzaSeparacion <= 0f) return Vector3.zero;

        int numCols = (layerEnemigos.value == 0)
            ? Physics.OverlapSphereNonAlloc(transform.position, radioSeparacion, bufferSeparacion)
            : Physics.OverlapSphereNonAlloc(transform.position, radioSeparacion, bufferSeparacion, layerEnemigos);

        Vector3 separacion = Vector3.zero;
        int     count      = 0;

        for (int i = 0; i < numCols; i++)
        {
            Collider col = bufferSeparacion[i];
            if (col == null || col.transform == transform) continue;

            EnemigoArbitro otro = col.GetComponentInParent<EnemigoArbitro>();
            if (otro == null || otro == this) continue;

            Vector3 away = transform.position - otro.transform.position;
            away.y = 0f;
            float d = away.magnitude;
            if (d < 0.001f) continue;

            separacion += (away / d) * (1f / d);
            count++;
        }

        if (count == 0) return Vector3.zero;
        return (separacion / count).normalized * fuerzaSeparacion;
    }

    private void DispararTarjeta()
    {
        if (prefabTarjeta == null || jugador == null) return;

        Vector3 origen  = (puntoDisparo != null) ? puntoDisparo.position : (transform.position + Vector3.up * 1.2f);
        Vector3 dir     = (jugador.position + Vector3.up * 1.0f - origen).normalized;
        if (dir == Vector3.zero) dir = transform.forward;

        GameObject obj = Instantiate(prefabTarjeta, origen, Quaternion.LookRotation(dir));

        Collider colTarjeta = obj.GetComponent<Collider>();
        if (colTarjeta != null)
        {
            Collider[] misColliders = GetComponentsInChildren<Collider>();
            for (int i = 0; i < misColliders.Length; i++)
                Physics.IgnoreCollision(colTarjeta, misColliders[i], true);
        }

        bool  roja  = Random.value < probabilidadRoja;
        int   dano  = roja ? danoRoja : danoAmarilla;
        Color color = roja ? colorRoja : colorAmarilla;

        TarjetaProjectile tarjeta = obj.GetComponent<TarjetaProjectile>();
        if (tarjeta != null) { tarjeta.Inicializar(dir); tarjeta.Configurar(dano, color); }
    }

    private void OnTriggerStay(Collider other)
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

        // Knockback usando jugador cacheado (no FindGameObjectWithTag)
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
