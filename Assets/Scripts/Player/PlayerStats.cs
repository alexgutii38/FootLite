using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    public int vidaActual;

    [Header("Leveling")]
    public int nivel = 1;
    public int experienciaActual = 0;
    public int experienciaSiguienteNivel = 60;
    public float multiplicadorExperiencia = 1f;

    [Header("Stats del jugador")]
    public int vidaMaxima = 120;
    public float speed = 5.0f;
    public float danoProyectil = 25f;
    public float rangoDisparo = 15f;
    public float cadenciaDisparo = 0.3f;
    public int cantidadDirecciones = 0;
    public float velocidadProyectil = 10f;
    public float suerte = 0f;
    public float regeneracionVida = 0f;

    [Header("Críticos")]
    [Range(0f, 1f)] public float probabilidadCritico = 0.08f;
    public float multiplicadorCritico = 2f;

    [Header("Auto-multidisparo")]
    [Tooltip("Cada cuántos niveles sin elegir multidisparo se concede +1 bala gratis.")]
    public int umbralAutoMultidisparo = 3;
    [HideInInspector] public int nivelesSinMultidisparo = 0;

    [Header("Defensa")]
    [Tooltip("Segundos de invulnerabilidad tras recibir un golpe. 0 = sin i-frames.")]
    public float tiempoInvulnerable = 0.4f;

    private float acumuladorRegeneracion = 0f;
    private float finInvulnerabilidad = 0f;

    private VignetteDano vignette;
    private LevelUpManager levelUpManagerCache;

    void Start()
    {
        nivel = 1;
        experienciaActual = 0;
        experienciaSiguienteNivel = Mathf.RoundToInt(Mathf.Pow(nivel + 3, 2));
        vidaActual = vidaMaxima;
        vignette = FindFirstObjectByType<VignetteDano>();
    }

    void Update()
    {
        if (regeneracionVida > 0f && vidaActual < vidaMaxima)
        {
            acumuladorRegeneracion += regeneracionVida * Time.deltaTime;

            if (acumuladorRegeneracion >= 1f)
            {
                int cantidad = Mathf.FloorToInt(acumuladorRegeneracion);
                vidaActual = Mathf.Min(vidaActual + cantidad, vidaMaxima);
                acumuladorRegeneracion -= cantidad;
                GameManager.Instancia?.ActualizarUI();
            }
        }
    }

    public void RecibirDano(int dano)
    {
        // Frames de invulnerabilidad: impide que un enjambre funda al jugador
        // de golpe. Cada enemigo lleva su propio temporizador de contacto, así
        // que sin esto 5 enemigos encima = 5 golpes casi simultáneos.
        if (Time.time < finInvulnerabilidad) return;
        finInvulnerabilidad = Time.time + tiempoInvulnerable;

        vidaActual -= dano;
        if (vignette != null) vignette.MostrarEfecto();
        if (CamaraTemblar.Instancia != null) CamaraTemblar.Instancia.Temblar();
        AudioManager.Instancia?.SonarDano();
        if (vidaActual < 0) vidaActual = 0;

        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.ActualizarUI();
            if (vidaActual <= 0)
                GameManager.Instancia.GameOver();
        }
    }

    public int ObtenerVidaActual()
    {
        return vidaActual;
    }

    public void GanarExperiencia(int cantidadBase)
    {
        int cantidad = Mathf.RoundToInt(cantidadBase * multiplicadorExperiencia);
        experienciaActual += cantidad;

        while (experienciaActual >= experienciaSiguienteNivel)
        {
            experienciaActual -= experienciaSiguienteNivel;
            SubirNivel();
        }
        
        GameManager.Instancia?.ActualizarUI();
    }

    void SubirNivel()
    {
        nivel++;
        float nuevoObjetivo = 1.50f * Mathf.Pow(nivel + 3, 2);
        experienciaSiguienteNivel = Mathf.RoundToInt(nuevoObjetivo);

        if (levelUpManagerCache == null)
            levelUpManagerCache = FindFirstObjectByType<LevelUpManager>();
        if (levelUpManagerCache != null)
            levelUpManagerCache.MostrarOpciones(this);
    }

    // MÉTODOS PARA GEMAS
    public void ModificarDano(float cantidad)
    {
        danoProyectil += cantidad;
    }

    public void ModificarVida(float cantidad)
    {
        vidaMaxima += (int)cantidad;
        vidaActual += (int)cantidad;
    }

    public void ModificarCadencia(float cantidad)
    {
        cadenciaDisparo = Mathf.Max(0.1f, cadenciaDisparo - cantidad);
    }

    public void Curar(float cantidad)
    {
        vidaActual = Mathf.Min(vidaMaxima, vidaActual + (int)cantidad);
    }

    public void ModificarEscudo(float cantidad)
    {
        int escudoExtra = (int)cantidad;
        vidaMaxima += escudoExtra;
        vidaActual = Mathf.Min(vidaActual + escudoExtra, vidaMaxima);
    }

    public void ModificarVelAtq(float cantidad)
    {
        velocidadProyectil += cantidad;
    }

    


}
