using UnityEngine;

public class GemaVida : MonoBehaviour
{
    [HideInInspector] 
    public float cantidadVida = 5f;

    [Header("Imán de Gemas")]
    public float rangoAtraccion = 3.5f;
    public float velocidadAtraccion = 10f;

    private Transform jugador;
    private bool siendoAtraida = false;
    private Vector3 escalaFinal;

    private void Start()
    {
        GameObject objJugador = GameObject.FindGameObjectWithTag("Player");
        if (objJugador != null) jugador = objJugador.transform;

        escalaFinal = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    private void Update()
    {
        // Pop animation
        if (transform.localScale.x < escalaFinal.x)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, escalaFinal, Time.deltaTime * 15f);
        }

        // Rotación
        transform.Rotate(Vector3.up * 150f * Time.deltaTime, Space.World);

        if (jugador == null) return;

        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia <= rangoAtraccion) siendoAtraida = true;

        if (siendoAtraida)
        {
            velocidadAtraccion += Time.deltaTime * 15f;
            Vector3 objetivo = jugador.position + Vector3.up * 1f;
            transform.position = Vector3.MoveTowards(transform.position, objetivo, velocidadAtraccion * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats ps = other.GetComponent<PlayerStats>();
            if (ps != null) ps.ModificarVida(cantidadVida);
            
            Destroy(gameObject);
        }
    }
}

