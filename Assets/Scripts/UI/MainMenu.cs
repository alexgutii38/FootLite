using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Interfaz")]
    public GameObject panelOpciones;
    public Slider sliderMusica;
    public Slider sliderEfectos;         // <-- NUEVO: Deslizador de efectos
    public Toggle togglePantallaCompleta;// <-- NUEVO: Casilla de pantalla completa

    [Header("Audio")]
    public AudioSource musicaFondo;

    private void Start()
    {
        // 1. Cargar Música
        float volumenMusica = PlayerPrefs.GetFloat("VolumenMusica", 1f);
        if (sliderMusica != null) sliderMusica.value = volumenMusica;
        if (musicaFondo != null) musicaFondo.volume = volumenMusica;

        // 2. Cargar Efectos
        float volumenEfectos = PlayerPrefs.GetFloat("VolumenEfectos", 1f);
        if (sliderEfectos != null) sliderEfectos.value = volumenEfectos;

        // 3. Cargar Pantalla Completa (Guardamos 1 para activado, 0 para desactivado)
        bool esPantallaCompleta = PlayerPrefs.GetInt("PantallaCompleta", 1) == 1;
        if (togglePantallaCompleta != null) togglePantallaCompleta.isOn = esPantallaCompleta;
        Screen.fullScreen = esPantallaCompleta;
    }

    // --- MÉTODOS DE LAS OPCIONES ---

    public void CambiarVolumenMusica(float valor)
    {
        if (musicaFondo != null) musicaFondo.volume = valor;
        PlayerPrefs.SetFloat("VolumenMusica", valor); 
    }

    public void CambiarVolumenEfectos(float valor)
    {
        // Por ahora solo guardamos el valor. Lo usaremos más adelante en los scripts de disparo/enemigos.
        PlayerPrefs.SetFloat("VolumenEfectos", valor);
    }

    public void CambiarPantallaCompleta(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("PantallaCompleta", isFullscreen ? 1 : 0);
    }

    // --- MÉTODOS DE BOTONES ---

    public void BotonJugar()
    {
        SceneManager.LoadScene("EscenaMundos"); 
    }

    public void AbrirOpciones()
    {
        if (panelOpciones != null) panelOpciones.SetActive(true);
    }

    public void CerrarOpciones()
    {
        if (panelOpciones != null) panelOpciones.SetActive(false);
    }

    public void BotonSalir()
    {
        Debug.Log("¡Cerrando el juego!"); 
        Application.Quit();
    }
}