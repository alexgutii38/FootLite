using UnityEngine;

// Esto define los TIPOS de mejoras que existen en tu juego.
public enum TipoStat
{
    VidaMax,
    Daño,
    Cadencia,
    VelocidadMovimiento,
    Direcciones,
    VelocidadXP,
    Rango,
    VelocidadProyectil,
    Regeneracion,
    Suerte
}

// [CreateAssetMenu] permite crear archivos de este tipo con clic derecho en Unity
[CreateAssetMenu(fileName = "NuevaMejora", menuName = "Sistema/Mejora Config")]
public class MejoraConfig : ScriptableObject
{
    [Header("¿Qué mejora es esta?")]
    public string nombre;
    public TipoStat tipoStat;
    [TextArea] public string descripcion;

    [Header("Valores de Balanceo")]
    
    public float valorComun;      
    public float valorRaro;       
    public float valorEpico;      
    public float valorLegendario; 
}
