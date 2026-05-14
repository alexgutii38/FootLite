using UnityEngine;
using UnityEngine.SceneManagement;

public class EscenaMundos : MonoBehaviour
{
    public void SeleccionarMundo(int mundo)
    {
        NivelManager.Instancia.mundoSeleccionado = mundo;
        SceneManager.LoadScene("EscenaNiveles");
    }

    public void Volver()
    {
        SceneManager.LoadScene("EscenaTitulo");
    }
}
