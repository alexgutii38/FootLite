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
    public Slider sliderExperiencia;
    public GameObject panelVictoria;
    public GameObject panelGameOver;

    void Start()
    {
        // Le pasamos todas estas referencias frescas al GameManager Inmortal
        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.textoVida = this.textoVida;
            GameManager.Instancia.textoEnemigos = this.textoEnemigos;
            GameManager.Instancia.textoRonda = this.textoRonda;
            GameManager.Instancia.textoTiempo = this.textoTiempo;
            GameManager.Instancia.textoNivelJugador = this.textoNivelJugador;
            GameManager.Instancia.sliderExperiencia = this.sliderExperiencia;
            
            GameManager.Instancia.panelVictoria = this.panelVictoria;
            GameManager.Instancia.panelGameOver = this.panelGameOver;

            // Nos aseguramos de que los paneles empiecen ocultos
            if(panelVictoria != null) panelVictoria.SetActive(false);
            if(panelGameOver != null) panelGameOver.SetActive(false);

            GameManager.Instancia.ActualizarUI();
        }
    }

    // Funciones puente para los botones. ¡Nunca se romperán!
    public void BotonContinuarInfinito()
    {
        if (GameManager.Instancia != null)
            GameManager.Instancia.ContinuarModoInfinito();
    }

    public void BotonSalirMenu()
    {
        if (GameManager.Instancia != null)
            GameManager.Instancia.SalirAlMenu();
    }
}