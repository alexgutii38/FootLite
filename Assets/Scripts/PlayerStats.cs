using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    [Header("Vida del Jugador")]
    public int vidaMaxima = 100;
    public int vidaActual;


    void Start()
    {
        vidaActual = vidaMaxima;
    }

    // Update is called once per frame
    public void RecibirDano(int dano)
    {
        vidaActual -= dano;
        Debug.Log("El jugador a recibido daño. Vida actual: " + vidaActual);
        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    void Morir()
    {
        Debug.Log("El jugador ha muerto.");
        SceneManager.LoadScene("EscenaDerrota");
    }

    public int ObtenerVidaActual()
    {
        return vidaActual;
    }
}
