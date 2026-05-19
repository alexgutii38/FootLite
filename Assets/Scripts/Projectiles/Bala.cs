using UnityEngine;

public class Bala : MonoBehaviour
{
    [Header("Vida")]
    public float tiempoVida = 2f;

    [Header("Anti-atasco al spawnear")]
    public float ignorarColisionesAlInicio = 0.05f;

    private float velocidad = 10f;
    private float dano      = 20f;
    private float tiempoActual;
    private float tiempoSpawn;

    /// <summary>
    /// Configura la bala al dispararla. La llama DisparoAutomatico justo
    /// después de obtenerla del pool. Así no hace falta buscar al jugador
    /// (FindGameObjectWithTag) una vez por cada bala, y se reinicia el
    /// estado para que la reutilización del pool funcione correctamente.
    /// </summary>
    public void Inicializar(float dano, float velocidad)
    {
        this.dano       = dano;
        this.velocidad  = velocidad;
        tiempoActual    = 0f;
        tiempoSpawn     = Time.time;
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * velocidad * Time.deltaTime);

        tiempoActual += Time.deltaTime;
        if (tiempoActual >= tiempoVida)
            Devolver();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time < tiempoSpawn + ignorarColisionesAlInicio) return;
        if (other.CompareTag("Player")) return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null)
            damageable.RecibirDano(dano);

        Devolver();
    }

    /// <summary>Devuelve la bala al pool (o la destruye si no proviene de uno).</summary>
    private void Devolver()
    {
        ObjectPool.Instancia.Devolver(gameObject);
    }
}
