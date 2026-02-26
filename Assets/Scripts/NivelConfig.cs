using UnityEngine;

[CreateAssetMenu(fileName = "NuevoNivel", menuName = "Sistema/Nivel Config")]
public class NivelConfig : ScriptableObject
{
    public string nombreNivel;
    public int mundoIndex;
    public int nivelIndex;
    public WaveConfig[] oleadas;
}

