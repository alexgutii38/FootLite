using UnityEngine;
using TMPro;

/// <summary>
/// Muestra el resumen de la última partida: resultado, tiempo sobrevivido,
/// enemigos eliminados, nivel alcanzado y los récords del nivel. Lee los
/// datos de <see cref="ResultadoPartida"/> y <see cref="SaveSystem"/>.
///
/// SETUP en Unity:
///  - PANTALLA DE VICTORIA: añade este componente al GameObject "panelVictoria"
///    y asigna 'textoResumen' (y, si quieres, 'textoTitulo').
///  - ESCENA DE DERROTA: añade este componente a un GameObject del Canvas de
///    EscenaDerrota y asigna 'textoResumen'.
///  Funciona en ambos casos: se actualiza en OnEnable (que se dispara tanto al
///  activar el panel de victoria como al cargar la escena de derrota).
/// </summary>
public class PantallaResultados : MonoBehaviour
{
    [Header("UI obligatoria")]
    [Tooltip("Texto donde se vuelca el resumen de la partida.")]
    public TextMeshProUGUI textoResumen;

    [Header("UI opcional")]
    [Tooltip("Si se asigna, muestra un título distinto según victoria/derrota.")]
    public TextMeshProUGUI textoTitulo;
    public string tituloVictoria = "¡NIVEL COMPLETADO!";
    public string tituloDerrota  = "DERROTA";

    void OnEnable()
    {
        Mostrar();
    }

    /// <summary>Compone y vuelca el texto del resumen a partir de los datos de la partida.</summary>
    void Mostrar()
    {
        if (textoTitulo != null)
            textoTitulo.text = ResultadoPartida.victoria ? tituloVictoria : tituloDerrota;

        if (textoResumen == null) return;

        int m = ResultadoPartida.mundo;
        int n = ResultadoPartida.nivel;

        string tiempo      = FormatearTiempo(ResultadoPartida.tiempoSobrevivido);
        string mejorTiempo = FormatearTiempo(SaveSystem.MejorTiempo(m, n));

        string marcaT = ResultadoPartida.nuevoRecordTiempo ? "   <color=#FFD24A>¡RÉCORD!</color>" : "";
        string marcaK = ResultadoPartida.nuevoRecordKills  ? "   <color=#FFD24A>¡RÉCORD!</color>" : "";

        textoResumen.text =
            $"Nivel  M{m}-N{n}\n\n" +
            $"Tiempo sobrevivido:  {tiempo}{marcaT}\n" +
            $"Enemigos eliminados:  {ResultadoPartida.enemigosEliminados}{marcaK}\n" +
            $"Nivel del jugador:  {ResultadoPartida.nivelJugador}\n\n" +
            $"<size=80%>Mejor tiempo: {mejorTiempo}   ·   " +
            $"Mejor marca: {SaveSystem.MejorKills(m, n)} eliminaciones</size>";
    }

    /// <summary>Convierte segundos a formato mm:ss.</summary>
    static string FormatearTiempo(float segundosTotales)
    {
        int minutos  = Mathf.FloorToInt(segundosTotales / 60f);
        int segundos = Mathf.FloorToInt(segundosTotales % 60f);
        return string.Format("{0:00}:{1:00}", minutos, segundos);
    }
}
