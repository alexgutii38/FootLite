using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CambiarEscenas : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void CambiarJuego()
    {
        SceneManager.LoadScene("EscenaCampo");
        StartCoroutine(IniciarPartida());
    }

    public void CambiarTitulo()
    {
        SceneManager.LoadScene("EscenaTitulo");
        
    }

    public void CambiarDerrota()
    {
       SceneManager.LoadScene("EscenaDerrota");
    }
    
    IEnumerator IniciarPartida()
    {
        yield return null;
        GameManager.Instancia.ReiniciarContadores();
        GameManager.Instancia.ReiniciarValores();
        GameManager.Instancia.ActualizarUI();
        
    }
}
