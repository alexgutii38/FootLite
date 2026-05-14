using UnityEngine;
using CartoonFX; // Para los textos de daño

public class EnemigoBoss : MonoBehaviour, IDamageable
{
    [Header("Stats del Jefe")]
    public float vida = 400f; // Sincronizado con lo que me dijiste
    public float velocidadMovimiento = 1.5f;
    public int danoPorContacto = 25;
    public float tiempoEntreDanos = 1f;

    [Header("Habilidades del Jefe")]
    public float tiempoEntreSprints = 8f; 
    public float multiplicadorSprint = 4f; 
    private float velocidadActual;
    private float tiempoParaSprint = 0f;
    private float tiempoFinSprint = 0f;
    private bool sprintando = false;

    [Header("Ataque de Tarjetas")]
    public GameObject prefabTarjeta; 
    public float tiempoEntreAtaquesCartas = 6f; 
    public int cantidadCartas = 8; 
    private float tiempoParaCartas = 0f;

    [Header("Animación")]
    public Animator animator; 

    [Header("Drops Épicos")]
    public GameObject prefabCofre;

    [Header("FX")]
    public ParticleSystem prefabTextoDaño;
    public ParticleSystem efectoMuerte;

    private Transform jugador;
    private float tiempoUltimoDano;
    private ParpadeoDano parpadeo;

    private void Start()
    {
        // 1. Buscamos al jugador
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) jugador = p.transform;

        // 2. Inicializamos los tiempos y la velocidad
        velocidadActual = velocidadMovimiento;
        tiempoParaSprint = Time.time + tiempoEntreSprints;
        tiempoParaCartas = Time.time + tiempoEntreAtaquesCartas;

        // 3. --- CONEXIÓN CON LA INTERFAZ ---
        // Le decimos a la barra que aparezca y se llene con nuestra vida máxima
        if (BossUIManager.Instancia != null)
        {
            BossUIManager.Instancia.MostrarBarraVida(vida);
        }

        parpadeo = GetComponent<ParpadeoDano>();
        if (parpadeo == null) parpadeo = gameObject.AddComponent<ParpadeoDano>();
    }

    private void Update()
    {
        if (jugador == null) return;

        // --- HABILIDAD 1: SPRINT ---
        if (!sprintando && Time.time >= tiempoParaSprint)
        {
            sprintando = true;
            velocidadActual = velocidadMovimiento * multiplicadorSprint;
            tiempoFinSprint = Time.time + 2f; // El sprint dura 2 segundos
        }

        if (sprintando && Time.time >= tiempoFinSprint)
        {
            sprintando = false;
            velocidadActual = velocidadMovimiento;
            tiempoParaSprint = Time.time + tiempoEntreSprints;
        }

        // --- HABILIDAD 2: DISPARAR TARJETAS ---
        if (Time.time >= tiempoParaCartas && prefabTarjeta != null)
        {
            DispararTarjetas();
            tiempoParaCartas = Time.time + tiempoEntreAtaquesCartas;
        }

        // --- MOVIMIENTO BÁSICO ---
        Vector3 direccion = (jugador.position - transform.position).normalized;
        direccion.y = 0f;

        if (direccion != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direccion), Time.deltaTime * 5f);
        }

        transform.position += direccion * velocidadActual * Time.deltaTime;

        // --- ANIMACIÓN ---
        if (animator != null)
        {
            // Si sprinta mueve las piernas al doble de velocidad
            animator.SetFloat("speed", sprintando ? 2f : 1f); 
        }
    }

    private void DispararTarjetas()
    {
        float anguloPaso = 360f / cantidadCartas;
        float radioAparicion = 3f; 

        Collider[] misColliders = GetComponentsInChildren<Collider>();

        for (int i = 0; i < cantidadCartas; i++)
        {
            float anguloActual = (anguloPaso * i);
            Quaternion rotacionCarta = Quaternion.Euler(0f, anguloActual, 0f);
            
            Vector3 direccionVuelo = rotacionCarta * Vector3.forward;
            Vector3 posicionSpawn = transform.position + (Vector3.up * 0.5f) + (direccionVuelo * radioAparicion);
            
            GameObject obj = Instantiate(prefabTarjeta, posicionSpawn, rotacionCarta);

            // Evitar que choquen con el jefe
            Collider colTarjeta = obj.GetComponent<Collider>();
            if (colTarjeta != null && misColliders != null)
            {
                for (int j = 0; j < misColliders.Length; j++)
                {
                    Physics.IgnoreCollision(colTarjeta, misColliders[j], true);
                }
            }

            // Iniciar la tarjeta y ponerla de color Rojo
            TarjetaProjectile tarjeta = obj.GetComponent<TarjetaProjectile>();
            if (tarjeta != null)
            {
                tarjeta.Inicializar(direccionVuelo);
                tarjeta.Configurar(danoPorContacto, Color.red); 
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats ps = other.GetComponent<PlayerStats>();
            if (ps != null && Time.time > tiempoUltimoDano + tiempoEntreDanos)
            {
                ps.RecibirDano(danoPorContacto);
                tiempoUltimoDano = Time.time;
            }
        }
    }

    public void RecibirDano(float cantidad)
    {
        // 1. Mostrar numerito de daño
        if (prefabTextoDaño != null)
        {
            ParticleSystem textObj = Instantiate(prefabTextoDaño, transform.position + Vector3.up * 4f, Quaternion.identity);
            CFXR_ParticleText scriptTexto = textObj.GetComponent<CFXR_ParticleText>();
            if (scriptTexto != null) scriptTexto.MostrarValorDaño(cantidad);
        }

        // 2. Empujar un poquitito al jefe
        if (jugador != null)
        {
            Vector3 direccionEmpuje = (transform.position - jugador.position).normalized;
            direccionEmpuje.y = 0f;
            transform.position += direccionEmpuje * 0.05f;
        }

        // 3. Restar vida real
        vida -= cantidad;
        if (parpadeo != null) parpadeo.Flash();

        // 4. --- CONEXIÓN CON LA INTERFAZ ---
        // Le avisamos al Slider de que nuestra vida ha bajado
        if (BossUIManager.Instancia != null) 
        {
            BossUIManager.Instancia.ActualizarBarraVida(vida);
        }

        // 5. Comprobar si muere
        if (vida <= 0f)
        {
            // Ocultamos la barra porque el jefe ya no existe
            if (BossUIManager.Instancia != null) 
            {
                BossUIManager.Instancia.OcultarBarraVida();
            }

            if (GameManager.Instancia != null) GameManager.Instancia.SumarEliminado();
            AudioManager.Instancia?.SonarMuerteEnemigo();

            if (prefabCofre != null)
            {
                Instantiate(prefabCofre, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            }

            if (efectoMuerte != null) Instantiate(efectoMuerte, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}