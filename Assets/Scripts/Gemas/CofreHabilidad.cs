using UnityEngine;

public class CofreHabilidad : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats ps = other.GetComponent<PlayerStats>();
            LevelUpManager manager = FindFirstObjectByType<LevelUpManager>();

            if (ps != null && manager != null)
            {
                // Llamamos a la pantalla de mejoras como si hubiéramos subido de nivel
                manager.MostrarOpciones(ps);
            }
            
            // Destruimos el cofre
            Destroy(gameObject);
        }
    }
}