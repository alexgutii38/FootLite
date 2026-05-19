using UnityEngine;

public class GemaExperiencia : MonoBehaviour
{
    [HideInInspector]
    public int cantidadExperiencia = 20;

    [Header("Imán de Gemas")]
    public float rangoAtraccion     = 3.5f;
    public float velocidadAtraccion = 10f;

    private Transform jugador;
    private bool      siendoAtraida;
    private Vector3   escalaFinal;
    private Rigidbody rb;

    // Comparar con sqrMagnitude evita la raíz cuadrada (más rápido)
    private float rangoAtraccionSqr;
    // Valor base para poder reiniciarlo al reutilizar la gema desde el pool
    private float velocidadAtraccionInicial;

    private void Awake()
    {
        // Se captura una sola vez (sobrevive a la reutilización del pool):
        // la escala original del prefab y la velocidad base del imán.
        escalaFinal               = transform.localScale;
        velocidadAtraccionInicial = velocidadAtraccion;
        rangoAtraccionSqr         = rangoAtraccion * rangoAtraccion;
        rb                        = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        // Reinicio de estado en cada (re)activación: compatible con object pooling.
        siendoAtraida        = false;
        velocidadAtraccion   = velocidadAtraccionInicial;
        transform.localScale = Vector3.zero;

        if (rb != null)
        {
            rb.linearVelocity  = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (jugador == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("Player");
            if (obj != null) jugador = obj.transform;
        }
    }

    private void Update()
    {
        // Animación Pop
        if (transform.localScale.x < escalaFinal.x)
            transform.localScale = Vector3.Lerp(transform.localScale, escalaFinal, Time.deltaTime * 15f);

        // Rotación decorativa
        transform.Rotate(Vector3.up * 150f * Time.deltaTime, Space.World);

        if (jugador == null) return;

        if (!siendoAtraida)
        {
            // sqrMagnitude es ~3x más rápido que Vector3.Distance
            float sqrDist = (transform.position - jugador.position).sqrMagnitude;
            if (sqrDist <= rangoAtraccionSqr)
                siendoAtraida = true;
        }

        if (siendoAtraida)
        {
            velocidadAtraccion = Mathf.Min(velocidadAtraccion + Time.deltaTime * 15f, 40f);
            transform.position = Vector3.MoveTowards(transform.position,
                jugador.position + Vector3.up * 1f, velocidadAtraccion * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        PlayerStats ps = other.GetComponent<PlayerStats>();
        if (ps != null) ps.GanarExperiencia(cantidadExperiencia);
        AudioManager.Instancia?.SonarRecogerGema();

        ObjectPool.Instancia.Devolver(gameObject);
    }
}
