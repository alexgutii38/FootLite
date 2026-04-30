using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossUIManager : MonoBehaviour
{
    public static BossUIManager Instancia;

    [Header("UI del Jefe")]
    public TextMeshProUGUI textoTemporizador;
    public GameObject contenedorBarraVida;
    public Slider sliderVida; // ¡Volvemos al Slider!

    private void Awake()
    {
        Instancia = this;
    }

    private void Start()
    {
        if (contenedorBarraVida != null) contenedorBarraVida.SetActive(false);
    }

    public void ActualizarTemporizador(float tiempoRestante)
    {
        if (textoTemporizador != null)
        {
            if (tiempoRestante > 0)
                textoTemporizador.text = "¡Jefe Final en: " + Mathf.CeilToInt(tiempoRestante) + "s!";
            else
                textoTemporizador.text = "";
        }
    }

    public void MostrarBarraVida(float vidaMaxima)
    {
        if (contenedorBarraVida != null) contenedorBarraVida.SetActive(true);
        if (sliderVida != null)
        {
            sliderVida.maxValue = vidaMaxima;
            sliderVida.value = vidaMaxima;
        }
    }

    public void ActualizarBarraVida(float vidaActual)
    {
        if (sliderVida != null) sliderVida.value = vidaActual;
    }

    public void OcultarBarraVida()
    {
        if (contenedorBarraVida != null) contenedorBarraVida.SetActive(false);
    }
}