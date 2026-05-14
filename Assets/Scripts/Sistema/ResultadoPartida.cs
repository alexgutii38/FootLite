/// <summary>
/// Contenedor estático con los datos de la última partida jugada.
///
/// Al ser datos planos en memoria (no un componente), sobreviven al
/// cambio de escena EscenaCampo → EscenaDerrota sin necesidad de
/// DontDestroyOnLoad. Lo rellena <see cref="GameManager"/> al ganar o
/// perder, y lo lee <see cref="PantallaResultados"/> para mostrar el
/// resumen al jugador.
/// </summary>
public static class ResultadoPartida
{
    /// <summary>Mundo donde se jugó la partida.</summary>
    public static int mundo;

    /// <summary>Nivel donde se jugó la partida.</summary>
    public static int nivel;

    /// <summary>Tiempo sobrevivido, en segundos.</summary>
    public static float tiempoSobrevivido;

    /// <summary>Enemigos eliminados durante la partida.</summary>
    public static int enemigosEliminados;

    /// <summary>Nivel alcanzado por el jugador (sistema de experiencia).</summary>
    public static int nivelJugador;

    /// <summary>True si el nivel se completó; false si el jugador murió.</summary>
    public static bool victoria;

    /// <summary>True si esta partida batió el récord de tiempo del nivel.</summary>
    public static bool nuevoRecordTiempo;

    /// <summary>True si esta partida batió el récord de eliminaciones del nivel.</summary>
    public static bool nuevoRecordKills;
}
