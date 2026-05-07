using UnityEngine;
using TMPro;

// Añade este componente a un GameObject vacío en la escena de juego.
// Asigna en el inspector: panel (GameObject UI) y textoStats (TextMeshProUGUI).
// Pulsa TAB en juego para mostrar/ocultar las estadísticas del jugador.
public class PanelEstadisticas : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;
    public TextMeshProUGUI textoStats;

    private PlayerStats stats;

    void Start()
    {
        stats = FindFirstObjectByType<PlayerStats>();
        if (panel != null) panel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            TogglePanel();

        if (panel != null && panel.activeSelf)
            ActualizarTexto();
    }

    void TogglePanel()
    {
        if (panel == null) return;
        panel.SetActive(!panel.activeSelf);
    }

    void ActualizarTexto()
    {
        if (stats == null) stats = FindFirstObjectByType<PlayerStats>();
        if (stats == null || textoStats == null) return;

        float dps = stats.danoProyectil * (1f / Mathf.Max(0.01f, stats.cadenciaDisparo)) * (1 + stats.cantidadDirecciones);

        textoStats.text =
            "<b>── ESTADÍSTICAS ──</b>\n\n" +
            $"Nivel:              {stats.nivel}\n" +
            $"Vida:               {stats.vidaActual} / {stats.vidaMaxima}\n" +
            $"Daño proyectil:     {stats.danoProyectil:F1}\n" +
            $"Cadencia:           {(1f / Mathf.Max(0.01f, stats.cadenciaDisparo)):F2} disp/s\n" +
            $"Proyectiles:        {1 + stats.cantidadDirecciones}\n" +
            $"DPS estimado:       {dps:F1}\n" +
            $"Velocidad:          {stats.speed:F1}\n" +
            $"Vel. proyectil:     {stats.velocidadProyectil:F1}\n" +
            $"Rango:              {stats.rangoDisparo:F1} m\n" +
            $"Regeneración:       {stats.regeneracionVida:F1} / s\n" +
            $"Multiplicador XP:   x{stats.multiplicadorExperiencia:F2}\n" +
            $"Suerte:             {stats.suerte:F0}\n\n" +
            "<size=80%>[TAB] para cerrar</size>";
    }
}
