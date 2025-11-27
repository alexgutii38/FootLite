using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton: acceso global fácil
    public static GameManager Instancia;

    [Header("Estado del jugador y partida")]
    //public int score = 0;
    public int enemigosEliminados = 0;
    

    [Header("Ronda / nivel / tiempo")]
    //*public int nivel = 1;
    public float tiempoPartida = 0f;
    public bool juegoEnPausa = false;

    [Header("Referencias de UI")]
    public TextMeshProUGUI textoVida;
    public TextMeshProUGUI textoScore;
    public TextMeshProUGUI textoEnemigos;
    public TextMeshProUGUI textoRonda;
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
        if (panelGameOver != null)
            panelGameOver.SetActive(false);
    }

    void Update()
    {
        if (!juegoEnPausa)
        {
            tiempoPartida += Time.deltaTime;
        }
        ActualizarUI();

        // Ejemplo: Pausa con Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PausarJuego(!juegoEnPausa);
        }
    }

    void OnSceneLoaded(Scene escena, LoadSceneMode modo)
    {
        // Reiniciar valores al cargar una nueva escena
        ReiniciarContadores();
        enemigosEliminados = 0;
        tiempoPartida = 0f;
        ActualizarUI();
    }

    /*public void SumarPuntos(int cantidad)
    {
        score += cantidad;
        ActualizarUI();
    }*/

    public void SumarEliminado()
    {
        enemigosEliminados++;
        ActualizarUI();
    }
    
    /*public void RestablecerVidaJugador(float cantidadMax)
    {
        vidaJugadorActual = cantidadMax;
        ActualizarUI();
    }
    public void SubirRonda()
    {
        nivel++;
        ActualizarUI();
    }*/

    public void PausarJuego(bool pausar)
    {
        juegoEnPausa = pausar;
        Time.timeScale = pausar ? 0f : 1f;
        // Aquí puedes mostrar/ocultar menú de pausa
    }

    public void GameOver()
    {
        SceneManager.LoadScene("EscenaDerrota");
    }

    // ---- UI ----
    public void ActualizarUI()
    {
        if(playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();

        if (textoVida != null)
          textoVida.text = "Vida: " + (playerStats != null ? playerStats.vidaActual: 0);
        //if (textoScore != null)
            //textoScore.text = "Puntos: " + score;
        if (textoEnemigos != null)
            textoEnemigos.text = "Eliminados: " + enemigosEliminados;
        //if (textoRonda != null)
            //textoRonda.text = "Ronda: " + nivel;
    }

    // Métodos para power-ups (ejemplo)
    /*public void AplicarPowerup(string tipo)
    {
        // Ejemplo: switch/case para distintos power-up
        if (tipo == "vida") RestablecerVidaJugador(100);
        else if (tipo == "puntos") SumarPuntos(50);
        // etc.
    }*/

    public void ReiniciarValores()
    {
        // Reiniciar variables si es necesario
        enemigosEliminados = 0;
        tiempoPartida = 0f;
        juegoEnPausa = false;

        if(playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();
        if(playerStats != null)
            playerStats.vidaActual = playerStats.vidaMaxima;

        ActualizarUI();
    }

    public void ReiniciarContadores()
    {
        GameObject objVida = GameObject.Find("textoVida");
        if (objVida != null)
        {
            textoVida = objVida.GetComponent<TextMeshProUGUI>();
    }
    else
        {
            Debug.LogWarning("No se encontró el objeto TextoVida en la escena.");
        }

        GameObject objEnemigos = GameObject.Find("textoEnemigos");
        if (objEnemigos != null)
        {
            textoEnemigos = objEnemigos.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            Debug.LogWarning("No se encontró el objeto TextoEnemigos en la escena.");
        }
    }
}
