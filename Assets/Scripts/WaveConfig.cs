using UnityEngine;

[CreateAssetMenu(fileName = "NuevaOleada", menuName = "Sistema/Oleada Config")]
public class WaveConfig : ScriptableObject
{
    [Header("Configuración de tiempo")]
    public float duracionOleada = 60f;

    [Header("Spawns de enemigos")]
        public float intervaloSpawn = 1f;
    public int enemigosPorSpawn = 1;

    [Header("Tipos de enemigos")]
    public GameObject[] prefabsEnemigos;

    [Header("Multiplicadores de dificultad")]
    public float multiplicadorVida = 1f;
    public float multiplicadorVelocidad = 1f;
    public int experienciaBase = 20;
}
