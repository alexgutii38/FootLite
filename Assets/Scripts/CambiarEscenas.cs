using UnityEngine;
using UnityEngine.SceneManagement;

public class CambiarEscenas : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void CambiarJuego()
    {
        SceneManager.LoadScene("EscenaCampo");
        
    }

    public void CambiarTitulo()
    {
        SceneManager.LoadScene("EscenaTitulo");
        
    }

    public void CambiarDerrota()
    {
       SceneManager.LoadScene("EscenaDerrota");
    }
}
