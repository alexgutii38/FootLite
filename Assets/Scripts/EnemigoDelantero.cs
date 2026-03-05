using CartoonFX;
using UnityEngine;

public class EnemigoDelantero : MonoBehaviour, IDamageable
{
    [Header("Atributos escalables")]
    public float vida = 50f;
    public float velocidadMovimiento = 5f;
    public int experienciaAlMorir = 20;
    public int experienciaPorGolpe = 5;

    public int danoPorContacto = 10;
    public float tiempoEntreDanos = 0.5f;
    public float rangoDeteccion = 50f;

    [Header("Futbol FX")]
    public ParticleSystem efectoMuerte;
    public ParticleSystem prefabTextoDaño;

    private float alturaInicial;
    private Transform jugador;
    private float tiempoUltimoDano;
    private Rigidbody rb;
    [Header("Animación")]
    public Animator animator;

    private bool moviendose;

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
            moviendose = true;
            Vector3 direccion = (jugador.position - transform.position).normalized;

            if (direccion != Vector3.zero)
            {
                Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, Time.deltaTime * 5f);
            }

            if (rb != null)
                rb.linearVelocity = new Vector3(direccion.x * velocidadMovimiento, 0f, direccion.z * velocidadMovimiento);
            else
                transform.position += direccion * velocidadMovimiento * Time.deltaTime;
        }
        else
        {
            moviendose = false;
            if (rb != null)
                rb.linearVelocity = Vector3.zero;
        }

        // Velocidad REAL para la animación: 0 cuando parado, velocidadMovimiento cuando persigue
        float speedAnimacion = moviendose ? velocidadMovimiento : 0f;
        if (animator != null)
            animator.SetFloat("speed", speedAnimacion, 0.1f, Time.deltaTime);


        // Separación entre enemigos
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
            float distanciaJugador = Vector3.Distance(transform.position, jugador.position);
            if (distanciaJugador < 1f)
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

    public void RecibirDano(float cantidad)
    {
        if (prefabTextoDaño != null)
        {
            ParticleSystem textObj = Instantiate(prefabTextoDaño, transform.position + Vector3.up * 2f, Quaternion.identity);
            CFXR_ParticleText scriptTexto = textObj.GetComponent<CFXR_ParticleText>();
            if (scriptTexto != null)
                scriptTexto.MostrarValorDaño(cantidad);
        }

        PlayerStats ps = FindFirstObjectByType<PlayerStats>();
        vida -= cantidad;

        if (vida <= 0f)
        {
            GameManager.Instancia.SumarEliminado();
            if (ps != null)
                ps.GanarExperiencia(experienciaAlMorir);

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
            GameManager.Instancia.QuitarEnemigoActivo();
        }
    }
}
