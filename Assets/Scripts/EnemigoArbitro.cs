using UnityEngine;

public class EnemigoArbitro : MonoBehaviour, IDamageable

{
    [Header("Stats")]
    public float vida = 40f;
    public float velocidadMovimiento = 2.4f;
    public int danoPorContacto = 6;
    public float tiempoEntreDanos = 0.6f;
    public float rangoDeteccion = 50f;

    [Header("Distancia de seguridad")]
    public float distanciaSeguridad = 4f;
    public float histeresis = 0.25f;

    [Header("Disparo tarjeta (no guiada)")]
    public GameObject prefabTarjeta;
    public Transform puntoDisparo;
    public float rangoDisparo = 8f;
    public float cooldownDisparo = 1.5f;
    public float distanciaMinimaDisparo = 2.5f;

    [Header("Tarjetas amarilla/roja")]
    [Range(0f, 1f)] public float probabilidadRoja = 0.35f;
    public int danoAmarilla = 7;
    public int danoRoja = 12;
    public Color colorAmarilla = Color.yellow;
    public Color colorRoja = Color.red;

    [Header("Movimiento inteligente")]
    public bool orbitarEnRango = true;
    public float velocidadOrbita = 1.1f;
    public float fuerzaCorreccionRadio = 2.0f;
    public float velocidadRetroceso = 1.2f;

    [Header("Separación entre árbitros")]
    public LayerMask layerEnemigos;     // Marca la layer Enemies en el inspector
    public float radioSeparacion = 1.2f;
    public float fuerzaSeparacion = 2.5f;

    [Header("Variación ángulos")]
    public float cambioLadoCada = 2.5f;
    [Range(0f, 1f)] public float probCambioLado = 0.35f;

    [Header("FX opcional")]
    public ParticleSystem efectoMuerte;

    private Transform jugador;
    private float alturaInicial;
    private float tiempoUltimoDano;
    private float tiempoUltimoDisparo;
    private Rigidbody rb;

    private int strafeSign = 1;
    private float tCambioLado;

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) jugador = p.transform;

        rb = GetComponent<Rigidbody>();
        alturaInicial = transform.position.y;

        strafeSign = (Random.value < 0.5f) ? -1 : 1; // Random.value ∈ [0..1] [web:192]
        tCambioLado = Time.time + Random.Range(0f, cambioLadoCada);
    }

    private void Update()
    {
        if (jugador == null) return;

        if (orbitarEnRango && cambioLadoCada > 0f && Time.time >= tCambioLado)
        {
            if (Random.value < probCambioLado) strafeSign *= -1; // Random.value ∈ [0..1] [web:192]
            tCambioLado = Time.time + cambioLadoCada;
        }

        Vector3 toPlayer = jugador.position - transform.position;
        toPlayer.y = 0f;

        float dist = toPlayer.magnitude;
        if (dist < 0.001f) return;

        Vector3 dirToPlayer = toPlayer / dist;

        bool enRangoDeteccion = dist <= rangoDeteccion;

        // Mirar al jugador para disparar
        Quaternion rotObjetivo = Quaternion.LookRotation(dirToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotObjetivo, Time.deltaTime * 8f);

        Vector3 velDeseada = Vector3.zero;

        if (enRangoDeteccion && dist > (distanciaSeguridad + histeresis))
        {
            // Acercarse hasta la distancia de seguridad
            velDeseada = dirToPlayer * velocidadMovimiento;
        }
        else if (enRangoDeteccion && dist < (distanciaSeguridad - histeresis))
        {
            // Si estás demasiado cerca, se aparta un poco
            velDeseada = (-dirToPlayer) * velocidadRetroceso;
        }
        else if (enRangoDeteccion)
        {
            // Banda: orbitar para buscar otros ángulos
            if (orbitarEnRango)
            {
                Vector3 tangent = Vector3.Cross(Vector3.up, dirToPlayer).normalized * strafeSign;

                float errorRadio = (dist - distanciaSeguridad);
                Vector3 correccion = dirToPlayer * (-errorRadio * fuerzaCorreccionRadio);

                velDeseada = tangent * (velocidadMovimiento * velocidadOrbita) + correccion;
            }
            else
            {
                velDeseada = Vector3.zero;
            }
        }

        // Separación entre árbitros
        velDeseada += CalcularSeparacion();

        // Limitar velocidad horizontal
        Vector3 velPlano = new Vector3(velDeseada.x, 0f, velDeseada.z);
        float maxSpeed = Mathf.Max(0.1f, velocidadMovimiento * 1.35f);
        if (velPlano.magnitude > maxSpeed) velPlano = velPlano.normalized * maxSpeed;

        if (rb != null)
            rb.linearVelocity = new Vector3(velPlano.x, rb.linearVelocity.y, velPlano.z);
        else
            transform.position += velPlano * Time.deltaTime;

        // Mantener altura constante
        Vector3 pos = transform.position;
        pos.y = alturaInicial;
        transform.position = pos;

        // Disparo
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

        Collider[] cols = (layerEnemigos.value == 0)
            ? Physics.OverlapSphere(transform.position, radioSeparacion)
            : Physics.OverlapSphere(transform.position, radioSeparacion, layerEnemigos);

        Vector3 separacion = Vector3.zero;
        int count = 0;

        for (int i = 0; i < cols.Length; i++)
        {
            if (cols[i] == null) continue;
            if (cols[i].transform == transform) continue;

            EnemigoArbitro otro = cols[i].GetComponentInParent<EnemigoArbitro>();
            if (otro == null || otro == this) continue;

            Vector3 away = transform.position - otro.transform.position;
            away.y = 0f;

            float d = away.magnitude;
            if (d < 0.001f) continue;

            separacion += (away / d) * (1f / d);
            count++;
        }

        if (count == 0) return Vector3.zero;
        separacion /= count;

        return separacion.normalized * fuerzaSeparacion;
    }

    private void DispararTarjeta()
    {
        if (prefabTarjeta == null || jugador == null) return;

        Vector3 origen = (puntoDisparo != null) ? puntoDisparo.position : (transform.position + Vector3.up * 1.2f);
        Vector3 objetivo = jugador.position + Vector3.up * 1.0f;
        Vector3 dir = (objetivo - origen).normalized;
        if (dir == Vector3.zero) dir = transform.forward;

        GameObject obj = Instantiate(prefabTarjeta, origen, Quaternion.LookRotation(dir));

        // Evitar que la tarjeta choque con el propio árbitro
        Collider colTarjeta = obj.GetComponent<Collider>();
        if (colTarjeta != null)
        {
            Collider[] misColliders = GetComponentsInChildren<Collider>();
            for (int i = 0; i < misColliders.Length; i++)
                Physics.IgnoreCollision(colTarjeta, misColliders[i], true);
        }

        // Elegir tipo de tarjeta (roja/amarilla)
        bool roja = Random.value < probabilidadRoja; // Random.value ∈ [0..1] [web:192]
        int dano = roja ? danoRoja : danoAmarilla;
        Color color = roja ? colorRoja : colorAmarilla;

        TarjetaProjectile tarjeta = obj.GetComponent<TarjetaProjectile>();
        if (tarjeta != null)
        {
            tarjeta.Inicializar(dir);
            tarjeta.Configurar(dano, color);
        }
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
        vida -= cantidad;
        if (vida <= 0f)
        {
            if (efectoMuerte != null)
                Instantiate(efectoMuerte, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}



