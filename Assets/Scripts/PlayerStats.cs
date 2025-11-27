using UnityEngine;
using System.Collections;


public class PlayerStats : MonoBehaviour
{
    
    [Header("Vida del Jugador")]
    
    public int vidaMaxima = 100;
    public int vidaActual;
    public float speed = 5.0f;

    public float dañoProyectil = 20f;

    public float rangoDisparo = 15f;
    public float cadenciaDisparo = 5f; 

    public int cantidadDirecciones = 0; // 0 = solo hacia enemigos, >0 = modo circular

    public float velocidadProyectil = 10f;
    void Start()
    {
        vidaActual = vidaMaxima;
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
}
