using UnityEngine;

// Singleton persistente para gestionar volúmenes de audio en toda la partida.
// Añade este componente a un GameObject vacío en la escena del menú principal
// (o en cualquier escena con DontDestroyOnLoad).
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instancia;

    private float volumenMusica   = 1f;
    private float volumenEfectos  = 1f;

    void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        volumenMusica  = PlayerPrefs.GetFloat("VolumenMusica",  1f);
        volumenEfectos = PlayerPrefs.GetFloat("VolumenEfectos", 1f);
    }

    public float ObtenerVolumenEfectos() => volumenEfectos;
    public float ObtenerVolumenMusica()  => volumenMusica;

    public void SetVolumenEfectos(float v)
    {
        volumenEfectos = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat("VolumenEfectos", volumenEfectos);
    }

    public void SetVolumenMusica(float v)
    {
        volumenMusica = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat("VolumenMusica", volumenMusica);
    }
}
