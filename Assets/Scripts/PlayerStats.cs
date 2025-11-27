using UnityEngine;
using System.Collections;


public class PlayerStats : MonoBehaviour
{
    
    
    
    
    public int vidaActual;

    [Header("Leveling")]
    public int nivel = 1;
    public int experienciaActual = 0;
    public int experienciaSiguienteNivel = 100;
    public float multiplicadorExperiencia = 1f;

    [Header("Stats del jugador")]
    public int vidaMaxima = 100;
    public float speed = 5.0f;

    public float danoProyectil = 20f;

    public float rangoDisparo = 15f;
    public float cadenciaDisparo = 5f; 

    public int cantidadDirecciones = 0; // 0 = solo hacia enemigos, >0 = modo circular

    public float velocidadProyectil = 10f;

    public float suerte = 0f;
    public float regeneracionVida = 0f;
    void Start()
    {
        vidaActual = vidaMaxima;
    }

    void Update()
    {
        if (regeneracionVida > 0f && vidaActual < vidaMaxima)
        {
            vidaActual += Mathf.RoundToInt(regeneracionVida * Time.deltaTime);
            if (vidaActual > vidaMaxima)
                vidaActual = vidaMaxima;
        }
    }

    // Update is called once per frame
    public void RecibirDano(int dano)
    {
        vidaActual -= dano;
        if (vidaActual < 0)
            vidaActual = 0;

        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.ActualizarUI();
            if (vidaActual <= 0)
            {
                GameManager.Instancia.GameOver();
            }
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
        
        LevelUpManager manager = FindFirstObjectByType<LevelUpManager>();
        if (manager != null)
        {
            manager.MostrarOpciones(this);
        }
    }
}
