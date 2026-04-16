using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class GemaVelAtq : MonoBehaviour
{
    [HideInInspector]
    public float cantidadVelAtq = 5f;

    [Header("Imán de Gemas")]
    public float rangoAtraccion = 3.5f;
    public float velocidadAtraccion = 10f;

    [Header("Movimiento al aparecer")]
    public float fuerzaHorizontal = 1.2f;
    public float fuerzaVertical = 1.3f;
    public float distanciaRecogida = 1.1f;

    private Transform jugador;
    private bool siendoAtraida = false;
    private Vector3 escalaFinal;
    private Rigidbody rb;
    private SphereCollider col;

    private void Start()
    {
        GameObject objJugador = GameObject.FindGameObjectWithTag("Player");
        if (objJugador != null) jugador = objJugador.transform;

        escalaFinal = transform.localScale;
        transform.localScale = Vector3.zero;

        rb = GetComponent<Rigidbody>();
        col = GetComponent<SphereCollider>();

        rb.useGravity = true;
        rb.isKinematic = false;
        rb.linearDamping = 1.5f;
        rb.angularDamping = 2f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        col.isTrigger = false;

        Vector3 dir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
        if (dir == Vector3.zero) dir = Vector3.forward;

        Vector3 fuerza = dir * fuerzaHorizontal + Vector3.up * fuerzaVertical;
        rb.AddForce(fuerza, ForceMode.Impulse);
    }

    private void Update()
    {
        if (transform.localScale.x < escalaFinal.x)
            transform.localScale = Vector3.Lerp(transform.localScale, escalaFinal, Time.deltaTime * 15f);

        transform.Rotate(Vector3.up * 150f * Time.deltaTime, Space.World);

        if (jugador == null) return;

        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (!siendoAtraida && distancia <= rangoAtraccion)
        {
            siendoAtraida = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (siendoAtraida)
        {
            velocidadAtraccion += Time.deltaTime * 15f;
            Vector3 objetivo = jugador.position + Vector3.up * 1f;
            transform.position = Vector3.MoveTowards(transform.position, objetivo, velocidadAtraccion * Time.deltaTime);

            if (Vector3.Distance(transform.position, jugador.position) <= distanciaRecogida)
            {
                PlayerStats ps = jugador.GetComponent<PlayerStats>();
                if (ps != null) ps.ModificarVelAtq(cantidadVelAtq);

                Destroy(gameObject);
            }
        }
    }
}


