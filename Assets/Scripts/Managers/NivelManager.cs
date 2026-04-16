using UnityEngine;
using UnityEngine.SceneManagement;

public class NivelManager : MonoBehaviour
{
    public static NivelManager Instancia;

    public int mundoSeleccionado = 1;
    public int nivelSeleccionado = 1;

    void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void SeleccionarMundo(int mundo)
    {
        mundoSeleccionado = mundo;
        SceneManager.LoadScene("EscenaCampo"); // ← nombre correcto

    }

    public void SeleccionarNivel(int nivel)
    {
        nivelSeleccionado = nivel;
        SceneManager.LoadScene("EscenaCampo"); // ← nombre correcto

    }
}

