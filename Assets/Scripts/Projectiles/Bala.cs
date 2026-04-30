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

    private PlayerStats stats;

    private void Start()
    {
        tiempoSpawn = Time.time;
        stats = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerStats>();
        ActualizarStats();
    }

    private void Update()
    {
        ActualizarStats();

        transform.Translate(Vector3.forward * velocidad * Time.deltaTime);

        tiempoActual += Time.deltaTime;
        if (tiempoActual >= tiempoVida)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignora colisiones justo al spawnear (si nace dentro del suelo/collider)
        if (Time.time < tiempoSpawn + ignorarColisionesAlInicio) return;

        if (other.CompareTag("Player")) return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            damageable.RecibirDano(dano);
            Destroy(gameObject);
            return;
        }

        // Si no es enemigo, destruye la bala (en vez de dejarla tirada en el suelo)
        Destroy(gameObject);
    }

    private void ActualizarStats()
    {
        if (stats == null) return;
        velocidad = stats.velocidadProyectil;
        dano = stats.danoProyectil;
    }
}
