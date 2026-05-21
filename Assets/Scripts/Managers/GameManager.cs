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

    [Header("Condición de Victoria")]
    public float tiempoParaGanar = 480f;
    public bool nivelCompletado = false;
    public GameObject panelVictoria;

    [Header("Referencias de UI")]
    public TextMeshProUGUI textoVida;
    public TextMeshProUGUI textoEnemigos;
    public TextMeshProUGUI textoRonda;

    [Header("HUD Principal")]
    public TextMeshProUGUI textoTiempo;
    public TextMeshProUGUI textoNivelJugador;
    public TextMeshProUGUI textoMundoNivel;
    public Slider sliderExperiencia;

    public GameObject panelGameOver;
    public PlayerStats playerStats;

    [Header("Menú de Pausa")]
    public GameObject panelPausa;

    // Referencia cacheada de la escena actual (se limpia en cada carga de escena).
    private LevelUpManager levelUpManagerCache;

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
            if (sliderExperiencia != null && playerStats != null)
            {
                sliderExperiencia.maxValue = playerStats.experienciaSiguienteNivel;
                sliderExperiencia.value = Mathf.Lerp(sliderExperiencia.value, playerStats.experienciaActual, Time.unscaledDeltaTime * 10f);
            }

            tiempoPartida += Time.deltaTime;

            if (textoTiempo != null)
            {
                int minutos = Mathf.FloorToInt(tiempoPartida / 60f);
                int segundos = Mathf.FloorToInt(tiempoPartida - minutos * 60);
                textoTiempo.text = string.Format("{0:00}:{1:00}", minutos, segundos);
            }

            if (!nivelCompletado && tiempoPartida >= tiempoParaGanar)
            {
                CompletarNivel();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // No pausar si el panel de level-up está activo.
            // Se cachea de forma perezosa: solo se busca una vez por escena.
            if (levelUpManagerCache == null)
                levelUpManagerCache = FindFirstObjectByType<LevelUpManager>();

            bool levelUpAbierto = levelUpManagerCache != null && levelUpManagerCache.gameObject.activeSelf;
            if (!levelUpAbierto)
                AlternarPausa();
        }
    }

    void OnSceneLoaded(Scene escena, LoadSceneMode modo)
    {
        levelUpManagerCache = null; // la escena cambió: la referencia anterior ya no vale
        ReiniciarContadores();
        ReiniciarValores();
        ActualizarUI();
    }

    public void CompletarNivel()
    {
        nivelCompletado = true;
        GuardarResultado(true);
        AudioManager.Instancia?.SonarVictoria();

        // Fallback robusto: si nadie asignó panelVictoria (ConexionUI ausente o
        // sin referencia en el Inspector), lo buscamos en la escena por el
        // componente PantallaResultados, incluso si está desactivado.
        if (panelVictoria == null)
        {
            PantallaResultados[] candidatos = FindObjectsByType<PantallaResultados>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (candidatos != null && candidatos.Length > 0)
            {
                panelVictoria = candidatos[0].gameObject;
                Debug.Log($"[GameManager] panelVictoria no asignado; recuperado por búsqueda: {panelVictoria.name}");
            }
        }

        if (panelVictoria != null)
        {
            panelVictoria.SetActive(true);
            Debug.Log("[GameManager] ¡NIVEL COMPLETADO! Panel de victoria activado.");
        }
        else
        {
            Debug.LogError("[GameManager] ¡NIVEL COMPLETADO! pero no se encontró ningún panel de victoria en la escena. Añade un GameObject con PantallaResultados o asigna 'panelVictoria' en el Inspector.");
        }

        PausarJuego(true);
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

    /// <summary>
    /// Alterna el menú de pausa (se llama al pulsar ESC). No actúa si el
    /// nivel ya está completado, para no tapar la pantalla de victoria.
    /// </summary>
    public void AlternarPausa()
    {
        if (nivelCompletado) return;

        bool pausar = !juegoEnPausa;
        PausarJuego(pausar);
        if (panelPausa != null) panelPausa.SetActive(pausar);
    }

    /// <summary>Reanuda la partida desde el menú de pausa (botón "Reanudar").</summary>
    public void ReanudarDesdePausa()
    {
        if (panelPausa != null) panelPausa.SetActive(false);
        PausarJuego(false);
    }

    /// <summary>
    /// Vuelca los datos de la partida actual en <see cref="ResultadoPartida"/>
    /// y los registra en <see cref="SaveSystem"/> (récords y, si es victoria,
    /// el desbloqueo del siguiente nivel).
    /// </summary>
    void GuardarResultado(bool victoria)
    {
        int mundo = NivelManager.Instancia != null ? NivelManager.Instancia.mundoSeleccionado : 1;
        int nivel = NivelManager.Instancia != null ? NivelManager.Instancia.nivelSeleccionado : 1;

        ResultadoPartida.mundo              = mundo;
        ResultadoPartida.nivel              = nivel;
        ResultadoPartida.tiempoSobrevivido  = tiempoPartida;
        ResultadoPartida.enemigosEliminados = enemigosEliminados;
        ResultadoPartida.nivelJugador       = playerStats != null ? playerStats.nivel : 1;
        ResultadoPartida.victoria           = victoria;

        SaveSystem.RegistrarResultado(mundo, nivel, tiempoPartida, enemigosEliminados,
            out bool recordTiempo, out bool recordKills);
        ResultadoPartida.nuevoRecordTiempo = recordTiempo;
        ResultadoPartida.nuevoRecordKills  = recordKills;

        if (victoria)
            SaveSystem.CompletarNivel(mundo, nivel);
    }

    public void GameOver()
    {
        GuardarResultado(false);
        AudioManager.Instancia?.SonarDerrota();
        SceneManager.LoadScene("EscenaDerrota");
    }

    public void ActualizarUI()
    {
        if (playerStats == null) playerStats = FindFirstObjectByType<PlayerStats>();

        if (textoVida != null) textoVida.text = "Vida: " + (playerStats != null ? playerStats.vidaActual : 0);
        if (textoEnemigos != null) textoEnemigos.text = "Muertes: " + enemigosEliminados;

        if (playerStats != null)
        {
            if (textoNivelJugador != null) textoNivelJugador.text = "Nivel: " + playerStats.nivel;
            if (sliderExperiencia != null)
                sliderExperiencia.maxValue = playerStats.experienciaSiguienteNivel;
        }

        // Mostrar mundo y nivel actuales
        if (textoMundoNivel != null && NivelManager.Instancia != null)
        {
            textoMundoNivel.text = $"M{NivelManager.Instancia.mundoSeleccionado}-N{NivelManager.Instancia.nivelSeleccionado}";
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
        nivelCompletado = false;
        juegoEnPausa = false;

        if (panelVictoria != null) panelVictoria.SetActive(false);

        playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats != null) playerStats.vidaActual = playerStats.vidaMaxima;

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
