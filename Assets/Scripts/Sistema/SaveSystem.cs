using UnityEngine;

/// <summary>
/// Sistema de guardado del juego, basado en PlayerPrefs. Gestiona el
/// desbloqueo progresivo de niveles del modo 3x3 y los récords
/// (mejor tiempo sobrevivido y más enemigos eliminados) de cada nivel.
///
/// Es una clase estática: NO requiere ninguna configuración en el editor
/// de Unity ni ningún GameObject. Cualquier script puede llamarla.
/// </summary>
public static class SaveSystem
{
    private const int TOTAL_MUNDOS      = 3;
    private const int NIVELES_POR_MUNDO = 3;

    // ── Claves de PlayerPrefs ────────────────────────────────────
    private static string ClaveDesbloqueado(int mundo, int nivel) => $"FL_M{mundo}N{nivel}_Desbloqueado";
    private static string ClaveCompletado(int mundo, int nivel)   => $"FL_M{mundo}N{nivel}_Completado";
    private static string ClaveMejorTiempo(int mundo, int nivel)  => $"FL_M{mundo}N{nivel}_MejorTiempo";
    private static string ClaveMejorKills(int mundo, int nivel)   => $"FL_M{mundo}N{nivel}_MejorKills";

    // ── Consultas ────────────────────────────────────────────────

    /// <summary>Devuelve true si el nivel puede jugarse. M1-N1 siempre está desbloqueado.</summary>
    public static bool NivelDesbloqueado(int mundo, int nivel)
    {
        if (mundo == 1 && nivel == 1) return true;
        return PlayerPrefs.GetInt(ClaveDesbloqueado(mundo, nivel), 0) == 1;
    }

    /// <summary>Devuelve true si el jugador ya superó este nivel alguna vez.</summary>
    public static bool NivelCompletado(int mundo, int nivel)
        => PlayerPrefs.GetInt(ClaveCompletado(mundo, nivel), 0) == 1;

    /// <summary>Mejor tiempo sobrevivido (en segundos) registrado en este nivel. 0 si no hay marca.</summary>
    public static float MejorTiempo(int mundo, int nivel)
        => PlayerPrefs.GetFloat(ClaveMejorTiempo(mundo, nivel), 0f);

    /// <summary>Mayor número de enemigos eliminados registrado en este nivel. 0 si no hay marca.</summary>
    public static int MejorKills(int mundo, int nivel)
        => PlayerPrefs.GetInt(ClaveMejorKills(mundo, nivel), 0);

    // ── Escritura ────────────────────────────────────────────────

    /// <summary>
    /// Marca un nivel como completado y desbloquea el siguiente:
    /// el nivel N+1 dentro del mismo mundo, o el N1 del mundo siguiente.
    /// </summary>
    public static void CompletarNivel(int mundo, int nivel)
    {
        PlayerPrefs.SetInt(ClaveCompletado(mundo, nivel), 1);

        int siguienteMundo = mundo;
        int siguienteNivel = nivel + 1;
        if (siguienteNivel > NIVELES_POR_MUNDO)
        {
            siguienteNivel = 1;
            siguienteMundo++;
        }

        if (siguienteMundo <= TOTAL_MUNDOS)
            PlayerPrefs.SetInt(ClaveDesbloqueado(siguienteMundo, siguienteNivel), 1);

        PlayerPrefs.Save();
    }

    /// <summary>
    /// Registra el resultado de una partida. Actualiza los récords si
    /// procede e informa por los parámetros 'out' si se ha batido alguno.
    /// </summary>
    public static void RegistrarResultado(int mundo, int nivel, float tiempo, int kills,
                                          out bool nuevoRecordTiempo, out bool nuevoRecordKills)
    {
        nuevoRecordTiempo = false;
        nuevoRecordKills  = false;

        if (tiempo > MejorTiempo(mundo, nivel))
        {
            PlayerPrefs.SetFloat(ClaveMejorTiempo(mundo, nivel), tiempo);
            nuevoRecordTiempo = true;
        }

        if (kills > MejorKills(mundo, nivel))
        {
            PlayerPrefs.SetInt(ClaveMejorKills(mundo, nivel), kills);
            nuevoRecordKills = true;
        }

        PlayerPrefs.Save();
    }

    /// <summary>Borra todo el progreso guardado. Útil para un botón "Reiniciar progreso".</summary>
    public static void BorrarProgreso()
    {
        for (int m = 1; m <= TOTAL_MUNDOS; m++)
            for (int n = 1; n <= NIVELES_POR_MUNDO; n++)
            {
                PlayerPrefs.DeleteKey(ClaveDesbloqueado(m, n));
                PlayerPrefs.DeleteKey(ClaveCompletado(m, n));
                PlayerPrefs.DeleteKey(ClaveMejorTiempo(m, n));
                PlayerPrefs.DeleteKey(ClaveMejorKills(m, n));
            }
        PlayerPrefs.Save();
    }
}
