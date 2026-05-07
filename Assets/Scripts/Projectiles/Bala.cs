using UnityEngine;

public class Bala : MonoBehaviour
{
    [Header("Vida")]
    public float tiempoVida = 2f;

    [Header("Anti-atasco al spawnear")]
    public float ignorarColisionesAlInicio = 0.05f;

    private float velocidad;
    private float dano;
    private float tiempoActual;
    private float tiempoSpawn;

    private void Start()
    {
        tiempoSpawn = Time.time;

        // Stats leídos una sola vez al spawnearse (no cambian durante el vuelo)
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            PlayerStats stats = p.GetComponent<PlayerStats>();
            if (stats != null)
            {
                velocidad = stats.velocidadProyectil;
                dano      = stats.danoProyectil;
                return;
            }
        }
        velocidad = 10f;
        dano      = 20f;
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * velocidad * Time.deltaTime);

        tiempoActual += Time.deltaTime;
        if (tiempoActual >= tiempoVida)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time < tiempoSpawn + ignorarColisionesAlInicio) return;
        if (other.CompareTag("Player")) return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            damageable.RecibirDano(dano);
            Destroy(gameObject);
            return;
        }

        Destroy(gameObject);
    }
}
