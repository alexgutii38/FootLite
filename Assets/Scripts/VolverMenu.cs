using UnityEngine;
using UnityEngine.SceneManagement;

public class VolverMenu : MonoBehaviour
{

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)){
            CargarMenu();
        }
    }
    public void CargarMenu()
    {
        Debug.Log("vovlemos al menu");
        SceneManager.LoadScene("EscenaTitulo"); // Usa el nombre exacto de tu escena de menú principal
    }
}

