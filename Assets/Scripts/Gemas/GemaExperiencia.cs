using UnityEngine;

public class GemaExperiencia : MonoBehaviour
{
    [HideInInspector]
    public int cantidadExperiencia = 20;

    [Header("Imán de Gemas")]
    public float rangoAtraccion    = 3.5f;
    public float velocidadAtraccion = 10f;

    private Transform jugador;
    private bool      siendoAtraida = false;
    private Vector3   escalaFinal;

    // Comparar con sqrMagnitude evita la raíz cuadrada (más rápido)
    private float rangoAtraccionSqr;

    private void Start()
    {
        GameObject obj = GameObject.FindGameObjectWithTag("Player");
        if (obj != null) jugador = obj.transform;

        escalaFinal      = transform.localScale;
        transform.localScale = Vector3.zero;
        rangoAtraccionSqr = rangoAtraccion * rangoAtraccion;
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
        Destroy(gameObject);
    }
}
