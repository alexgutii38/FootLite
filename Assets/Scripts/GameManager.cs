using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instancia;

    [Header("Estado del jugador y partida")]
    public int enemigosEliminados = 0;

    [Header("Optimización")]
    public int enemigosActivos = 0; 

    [Header("Ronda / nivel / tiempo")]
    public float tiempoPartida = 0f;
    public bool juegoEnPausa = false;
    
    [Header("Condición de Victoria (Modo Supervivencia)")]
    public float tiempoParaGanar = 480f; // 480 segundos = 8 minutos
    public bool nivelCompletado = false;
    public GameObject panelVictoria; // <-- NUEVO PANEL

    [Header("Referencias de UI")]
    public TextMeshProUGUI textoVida;
    public TextMeshProUGUI textoEnemigos;
    public TextMeshProUGUI textoRonda; 
    
    [Header("Nuevos Elementos HUD")]
    public TextMeshProUGUI textoTiempo;
    public TextMeshProUGUI textoNivelJugador;
    public Slider sliderExperiencia;

    public GameObject panelGameOver;
    public PlayerStats playerStats;

    void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        ActualizarUI();
        if (panelGameOver != null) panelGameOver.SetActive(false);
        if (panelVictoria != null) panelVictoria.SetActive(false);
    }

    void Update()
    {
        if (!juegoEnPausa)
        {
            tiempoPartida += Time.deltaTime;

            if (textoTiempo != null)
            {
                int minutos = Mathf.FloorToInt(tiempoPartida / 60F);
                int segundos = Mathf.FloorToInt(tiempoPartida - minutos * 60);
                textoTiempo.text = string.Format("{0:00}:{1:00}", minutos, segundos);
            }

            // <-- NUEVO: Comprobar si hemos llegado a los 8 minutos
            if (!nivelCompletado && tiempoPartida >= tiempoParaGanar)
            {
                CompletarNivel();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PausarJuego(!juegoEnPausa);
        }
    }

    void OnSceneLoaded(Scene escena, LoadSceneMode modo)
    {
        ReiniciarContadores();
        ReiniciarValores();
        ActualizarUI();
    }

    // --- NUEVOS MÉTODOS DE VICTORIA ---
    public void CompletarNivel()
    {
        nivelCompletado = true;
        if (panelVictoria != null)
        {
            panelVictoria.SetActive(true);
        }
        PausarJuego(true); // Pausamos para que el jugador elija qué hacer
    }

    public void ContinuarModoInfinito()
    {
        if (panelVictoria != null) panelVictoria.SetActive(false);
        PausarJuego(false);
    }

    public void SalirAlMenu()
    {
        PausarJuego(false);
        SceneManager.LoadScene("EscenaTitulo"); 
    }
    // ----------------------------------

    public void AgregarEnemigoActivo() { enemigosActivos++; }

    public void QuitarEnemigoActivo()
    {
        enemigosActivos--;
        if (enemigosActivos < 0) enemigosActivos = 0;
    }

    public void SumarEliminado()
    {
        enemigosEliminados++;
        ActualizarUI();
    }

    public void PausarJuego(bool pausar)
    {
        juegoEnPausa = pausar;
        Time.timeScale = pausar ? 0f : 1f;
    }

    public void GameOver()
    {
        SceneManager.LoadScene("EscenaDerrota");
    }

    public void ActualizarUI()
    {
        if(playerStats == null) playerStats = FindFirstObjectByType<PlayerStats>();

        if (textoVida != null) textoVida.text = "Vida: " + (playerStats != null ? playerStats.vidaActual: 0);
        if (textoEnemigos != null) textoEnemigos.text = "Muertes: " + enemigosEliminados;

        if (playerStats != null)
        {
            if (textoNivelJugador != null) textoNivelJugador.text = "Nivel: " + playerStats.nivel;
            if (sliderExperiencia != null)
            {
                sliderExperiencia.maxValue = playerStats.experienciaSiguienteNivel;
                sliderExperiencia.value = playerStats.experienciaActual;
            }
        }
    }

    public void ActualizarTextoOleada(int oleada, int total, int ciclo)
    {
        if (textoRonda != null)
        {
            string texto = $"Oleada: {oleada}/{total}";
            if (ciclo > 0) texto += $" (Ciclo {ciclo + 1})";
            textoRonda.text = texto;
        }
    }

    public void ReiniciarValores()
    {
        enemigosEliminados = 0;
        enemigosActivos = 0; 
        tiempoPartida = 0f;
        nivelCompletado = false; // <-- Resetear condición de victoria
        juegoEnPausa = false;
        
        if (panelVictoria != null) panelVictoria.SetActive(false);

        if(playerStats == null) playerStats = FindFirstObjectByType<PlayerStats>();
        if(playerStats != null) playerStats.vidaActual = playerStats.vidaMaxima;

        ActualizarUI();
    }

    public void ReiniciarContadores()
    {
        GameObject objVida = GameObject.Find("textoVida");
        if (objVida != null) textoVida = objVida.GetComponent<TextMeshProUGUI>();
        
        GameObject objEnemigos = GameObject.Find("textoEnemigos");
        if (objEnemigos != null) textoEnemigos = objEnemigos.GetComponent<TextMeshProUGUI>();
    }
}