using UnityEngine;

public class GemaManager : MonoBehaviour
{
    public static GemaManager Instance { get; private set; }

    [Header("Probabilidades")]
    public float probabilidadPowerUp = 0.3f; // 30% para power-ups

    [Header("Prefabs")]
    public GameObject gemaXP; // La que sale SIEMPRE
    public GameObject[] powerUps = new GameObject[4]; // Las 4 power-ups (sin XP)

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void GenerarGemas(Vector3 posicion)
    {
        // 1. SIEMPRE suelta XP
        if (gemaXP != null)
        {
            GameObject xp = Instantiate(gemaXP, posicion + Vector3.up * 0.5f, Quaternion.identity);
            // Física automática si ya la tiene configurada
        }

        // 2. 30% chance de power-up adicional
        if (Random.value <= probabilidadPowerUp && powerUps.Length > 0)
        {
            int indice = Random.Range(0, powerUps.Length);
            GameObject powerUp = Instantiate(powerUps[indice], posicion + Vector3.up * 0.3f, Quaternion.identity);
        }
    }
}

