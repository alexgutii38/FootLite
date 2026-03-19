using UnityEngine;

public class GemaExperiencia : MonoBehaviour
{
    [HideInInspector] 
    public int cantidadExperiencia = 20;

    [Header("Imán de Gemas")]
    public float rangoAtraccion = 3.5f; // Distancia a la que empieza a volar hacia el jugador
    public float velocidadAtraccion = 10f; // Velocidad inicial de vuelo

    private Transform jugador;
    private bool siendoAtraida = false;
    private Vector3 escalaFinal; // Para el efecto Pop

    private void Start()
    {
        // Buscamos al jugador una vez cuando la gema aparece en el mapa
        GameObject objJugador = GameObject.FindGameObjectWithTag("Player");
        if (objJugador != null)
        {
            jugador = objJugador.transform;
        }

        // Guardamos su tamaño real y la hacemos invisible al nacer para el efecto "Pop"
        escalaFinal = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    private void Update()
    {
        // 1. Animación "Pop" (crece suavemente hasta su tamaño normal)
        if (transform.localScale.x < escalaFinal.x)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, escalaFinal, Time.deltaTime * 15f);
        }

        // 2. Rotación continua para que llame la atención
        transform.Rotate(Vector3.up * 150f * Time.deltaTime, Space.World);

        // Si no hay jugador, no calculamos el imán
        if (jugador == null) return;

        // 3. Comprobamos la distancia entre la gema y el jugador
        float distancia = Vector3.Distance(transform.position, jugador.position);

        // Si entra en el rango, activamos el imán para siempre
        if (distancia <= rangoAtraccion)
        {
            siendoAtraida = true;
        }

        // 4. Si el imán está activado, volamos hacia el jugador
        if (siendoAtraida)
        {
            // Aceleramos la gema con el tiempo para que siempre te alcance aunque corras
            velocidadAtraccion += Time.deltaTime * 15f; 
            
            // Movemos la gema hacia el centro del jugador (apuntando al pecho)
            Vector3 objetivo = jugador.position + Vector3.up * 1f;
            transform.position = Vector3.MoveTowards(transform.position, objetivo, velocidadAtraccion * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Cuando por fin toca al jugador, da la experiencia y se destruye
        if (other.CompareTag("Player"))
        {
            PlayerStats ps = other.GetComponent<PlayerStats>();
            if (ps != null)
            {
                ps.GanarExperiencia(cantidadExperiencia);
            }
            
            Destroy(gameObject);
        }
    }
}