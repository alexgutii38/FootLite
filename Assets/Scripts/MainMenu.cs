using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("EscenaMundos"); // ← cambia esto
    }

    public void Options()
    {
        // Tu lógica de opciones
    }

    public void Exit()
    {
        Application.Quit();
    }
}
