using UnityEngine;
using System.Collections;
using CartoonFX; 

public class EnemigoBoss : MonoBehaviour, IDamageable
{
    [Header("Stats del Jefe")]
    public float vida = 1500f;
    public float velocidadMovimiento = 1.5f;
    public int danoPorContacto = 25;
    public float tiempoEntreDanos = 1f;

    [Header("Habilidades del Jefe")]
    public float tiempoEntreSprints = 8f; // Cada cuántos segundos sprinta
    public float multiplicadorSprint = 2.5f; // Cuánto se multiplica su velocidad al sprintar
    private float velocidadActual;
    private float tiempoParaSprint = 0f;
    private float tiempoFinSprint = 0f;
    private bool sprintando = false;

    [Header("Ataque de Tarjetas")]
    public GameObject prefabTarjeta; // Arrastra aquí el prefab de la bala/tarjeta del árbitro
    public float tiempoEntreAtaquesCartas = 6f; // Cada cuánto dispara
    public int cantidadCartas = 8; // Cuántas tarjetas dispara a la vez
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

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) jugador = p.transform;

        // Inicializamos los tiempos y la velocidad
        velocidadActual = velocidadMovimiento;
        tiempoParaSprint = Time.time + tiempoEntreSprints;
        tiempoParaCartas = Time.time + tiempoEntreAtaquesCartas;
    }

    private void Update()
    {
        if (jugador == null) return;

        // --- 1. LÓGICA DE HABILIDADES ---

        // Control del Sprint
        if (!sprintando && Time.time >= tiempoParaSprint)
        {
            sprintando = true;
            velocidadActual = velocidadMovimiento * multiplicadorSprint;
            tiempoFinSprint = Time.time + 2f; // El sprint dura 2 segundos exactos
        }

        if (sprintando && Time.time >= tiempoFinSprint)
        {
            sprintando = false;
            velocidadActual = velocidadMovimiento;
            tiempoParaSprint = Time.time + tiempoEntreSprints;
        }

        // Control de disparar tarjetas
        if (Time.time >= tiempoParaCartas && prefabTarjeta != null)
        {
            DispararTarjetas();
            tiempoParaCartas = Time.time + tiempoEntreAtaquesCartas;
        }

        // --- 2. MOVIMIENTO ---
        Vector3 direccion = (jugador.position - transform.position).normalized;
        direccion.y = 0f;

        if (direccion != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direccion), Time.deltaTime * 5f);
        }

        // Usamos velocidadActual en lugar de velocidadMovimiento para aplicar el sprint
        transform.position += direccion * velocidadActual * Time.deltaTime;

        // Animación: Si sprinta, hacemos que la animación de correr vaya el doble de rápido
        if (animator != null)
        {
            animator.SetFloat("speed", sprintando ? 2f : 1f); 
        }
    }

    private void DispararTarjetas()
    {
        float anguloPaso = 360f / cantidadCartas;
        float radioAparicion = 3f; // Distancia para que salgan fuera del cuerpo del jefe

        // Recogemos todos los colliders del jefe para evitar que las tarjetas choquen con él
        Collider[] misColliders = GetComponentsInChildren<Collider>();

        for (int i = 0; i < cantidadCartas; i++)
        {
            float anguloActual = (anguloPaso * i);
            Quaternion rotacionCarta = Quaternion.Euler(0f, anguloActual, 0f);
            
            // Dirección en la que saldrá volando esta tarjeta
            Vector3 direccionVuelo = rotacionCarta * Vector3.forward;
            
            // Busca esta línea dentro de DispararTarjetas()
            Vector3 posicionSpawn = transform.position + (Vector3.up * 0.5f) + (direccionVuelo * radioAparicion);
            
            // Creamos la tarjeta
            GameObject obj = Instantiate(prefabTarjeta, posicionSpawn, rotacionCarta);

            // 1. Evitamos que la tarjeta choque con el propio jefe al nacer
            Collider colTarjeta = obj.GetComponent<Collider>();
            if (colTarjeta != null && misColliders != null)
            {
                for (int j = 0; j < misColliders.Length; j++)
                {
                    Physics.IgnoreCollision(colTarjeta, misColliders[j], true);
                }
            }

            // 2. MAGIA: Le damos el empujón y el color (El secreto del Árbitro)
            TarjetaProjectile tarjeta = obj.GetComponent<TarjetaProjectile>();
            if (tarjeta != null)
            {
                // Le decimos que vuele en su dirección
                tarjeta.Inicializar(direccionVuelo);
                
                // Le ponemos el daño del jefe y color ROJO (o Color.yellow)
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
        if (prefabTextoDaño != null)
        {
            ParticleSystem textObj = Instantiate(prefabTextoDaño, transform.position + Vector3.up * 4f, Quaternion.identity);
            CFXR_ParticleText scriptTexto = textObj.GetComponent<CFXR_ParticleText>();
            if (scriptTexto != null) scriptTexto.MostrarValorDaño(cantidad);
        }

        if (jugador != null)
        {
            Vector3 direccionEmpuje = (transform.position - jugador.position).normalized;
            direccionEmpuje.y = 0f;
            transform.position += direccionEmpuje * 0.05f;
        }

        vida -= cantidad;

        if (vida <= 0f)
        {
            if (GameManager.Instancia != null) GameManager.Instancia.SumarEliminado();

            if (prefabCofre != null)
            {
                Instantiate(prefabCofre, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            }

            if (efectoMuerte != null) Instantiate(efectoMuerte, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}