using UnityEngine;
using UnityEngine.SceneManagement;

public class EscenaNiveles : MonoBehaviour
{
    public void SeleccionarNivel(int nivel)
    {
        NivelManager.Instancia.SeleccionarNivel(nivel);
    }

    public void Volver()
    {
        SceneManager.LoadScene("EscenaMundos");
    }
}

