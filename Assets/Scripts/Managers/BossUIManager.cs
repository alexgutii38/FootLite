using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossUIManager : MonoBehaviour
{
    public static BossUIManager Instancia;

    [Header("UI del Jefe")]
    public TextMeshProUGUI textoTemporizador;
    [Tooltip("Panel que contiene el texto del contador. Se oculta cuando aparece el jefe o cuando muere.")]
    public GameObject contenedorTemporizador;
    public GameObject contenedorBarraVida;
    public Slider sliderVida; // ¡Volvemos al Slider!

    private void Awake()
    {
        Instancia = this;
    }

    private void Start()
    {
        if (contenedorBarraVida != null) contenedorBarraVida.SetActive(false);
        // El contador empieza oculto: solo aparece cuando WaveManager empieza a llamar ActualizarTemporizador.
        if (contenedorTemporizador != null) contenedorTemporizador.SetActive(false);
    }

    public void ActualizarTemporizador(float tiempoRestante)
    {
        if (tiempoRestante > 0)
        {
            if (contenedorTemporizador != null && !contenedorTemporizador.activeSelf)
                contenedorTemporizador.SetActive(true);
            if (textoTemporizador != null)
                textoTemporizador.text = "¡Jefe Final en: " + Mathf.CeilToInt(tiempoRestante) + "s!";
        }
        else
        {
            // El contador llegó a 0: el jefe va a aparecer (o ya apareció), ocultamos el panel.
            if (textoTemporizador != null) textoTemporizador.text = "";
            if (contenedorTemporizador != null) contenedorTemporizador.SetActive(false);
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
        // Por seguridad: si el contador aún estaba visible (no debería), también lo escondemos.
        if (contenedorTemporizador != null) contenedorTemporizador.SetActive(false);
    }
}