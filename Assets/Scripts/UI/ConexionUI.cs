using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ConexionUI : MonoBehaviour
{
    [Header("Arrastra aquí los elementos del Canvas")]
    public TextMeshProUGUI textoVida;
    public TextMeshProUGUI textoEnemigos;
    public TextMeshProUGUI textoRonda;
    public TextMeshProUGUI textoTiempo;
    public TextMeshProUGUI textoNivelJugador;
    public TextMeshProUGUI textoMundoNivel;
    public Slider sliderExperiencia;
    public GameObject panelVictoria;
    public GameObject panelGameOver;
    public GameObject panelPausa;

    void Start()
    {
        if (GameManager.Instancia == null) return;

        GameManager.Instancia.textoVida         = textoVida;
        GameManager.Instancia.textoEnemigos     = textoEnemigos;
        GameManager.Instancia.textoRonda        = textoRonda;
        GameManager.Instancia.textoTiempo       = textoTiempo;
        GameManager.Instancia.textoNivelJugador = textoNivelJugador;
        GameManager.Instancia.textoMundoNivel   = textoMundoNivel;
        GameManager.Instancia.sliderExperiencia = sliderExperiencia;
        GameManager.Instancia.panelVictoria     = panelVictoria;
        GameManager.Instancia.panelGameOver     = panelGameOver;
        GameManager.Instancia.panelPausa        = panelPausa;

        // Cachear referencia al jugador desde la escena
        PlayerStats ps = FindFirstObjectByType<PlayerStats>();
        if (ps != null) GameManager.Instancia.playerStats = ps;

        if (panelVictoria  != null) panelVictoria.SetActive(false);
        if (panelGameOver  != null) panelGameOver.SetActive(false);
        if (panelPausa     != null) panelPausa.SetActive(false);

        GameManager.Instancia.ActualizarUI();

        // Arranca la música de partida (si AudioManager existe en la escena).
        AudioManager.Instancia?.ReproducirMusicaPartida();
    }

    public void BotonContinuarInfinito()
    {
        GameManager.Instancia?.ContinuarModoInfinito();
    }

    public void BotonSalirMenu()
    {
        GameManager.Instancia?.SalirAlMenu();
    }
}
