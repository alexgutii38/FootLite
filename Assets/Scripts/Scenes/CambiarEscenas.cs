using UnityEngine;
using UnityEngine.SceneManagement;

public class CambiarEscenas : MonoBehaviour
{
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
