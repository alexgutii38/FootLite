using UnityEngine;

public class GemaBase : MonoBehaviour
{
    [Header("Valores")]
    public float valorGema = 1f;
    
    [Header("Efectos Visuales")]
    public ParticleSystem particulaRecogida;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            RecogerGema(other.transform);
        }
    }

    protected virtual void RecogerGema(Transform jugador)
    {
        // Efecto visual
        if (particulaRecogida != null)
        {
            ParticleSystem ps = Instantiate(particulaRecogida, transform.position, Quaternion.identity);
            Destroy(ps.gameObject, 2f);
        }

        AplicarEfecto(jugador);
        Destroy(gameObject);
    }

    protected virtual void AplicarEfecto(Transform jugador) { }
}

