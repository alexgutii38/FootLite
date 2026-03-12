using UnityEngine;

public class GemaExperiencia : MonoBehaviour
{
    [HideInInspector] 
    public int cantidadExperiencia = 20; // Esto lo configurará el enemigo al morir

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats ps = other.GetComponent<PlayerStats>();
            if (ps != null)
            {
                ps.GanarExperiencia(cantidadExperiencia);
            }
            
            // Destruimos la gema una vez recogida
            Destroy(gameObject);
        }
    }
}