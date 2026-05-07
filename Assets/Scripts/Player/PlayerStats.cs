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
    public int vidaMaxima = 100;
    public float speed = 5.0f;
    public float danoProyectil = 20f;
    public float rangoDisparo = 15f;
    public float cadenciaDisparo = 5f;
    public int cantidadDirecciones = 0;
    public float velocidadProyectil = 10f;
    public float suerte = 0f;
    public float regeneracionVida = 0f;

    private float acumuladorRegeneracion = 0f;

    private VignetteDano vignette;

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
        vidaActual -= dano;
        if (vignette != null) vignette.MostrarEfecto();
        if (CamaraTemblar.Instancia != null) CamaraTemblar.Instancia.Temblar();
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

        LevelUpManager manager = FindFirstObjectByType<LevelUpManager>();
        if (manager != null)
            manager.MostrarOpciones(this);
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
