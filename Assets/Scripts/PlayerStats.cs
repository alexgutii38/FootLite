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

    private float acumuladorRegeneracion = 0f; // ← NUEVO

    void Start()
    {
        nivel = 1;
        experienciaActual = 0;
        experienciaSiguienteNivel = Mathf.RoundToInt(Mathf.Pow(nivel + 3, 2));
        vidaActual = vidaMaxima;
    }

    void Update()
    {
        // ← BLOQUE CORREGIDO
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
}
