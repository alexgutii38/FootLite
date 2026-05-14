using UnityEngine;

/// <summary>
/// Singleton de audio persistente. Centraliza la reproducción de la música
/// de fondo y de los efectos de sonido del juego, y gestiona los volúmenes
/// (que se guardan en PlayerPrefs).
///
/// SETUP en Unity (EscenaTitulo):
///  - Añade este componente a un GameObject vacío.
///  - Asigna los AudioClip en el inspector. Los que dejes vacíos simplemente
///    se ignoran, sin generar errores.
///  - NO hace falta añadir componentes AudioSource a mano: se crean solos.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instancia;

    [Header("Música (en bucle)")]
    public AudioClip musicaMenu;
    public AudioClip musicaPartida;

    [Header("Efectos de sonido")]
    public AudioClip sfxDano;
    public AudioClip sfxMuerteEnemigo;
    public AudioClip sfxRecogerGema;
    public AudioClip sfxLevelUp;
    public AudioClip sfxVictoria;
    public AudioClip sfxDerrota;

    private AudioSource fuenteMusica;
    private AudioSource fuenteEfectos;

    private float volumenMusica  = 1f;
    private float volumenEfectos = 1f;

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

        // Las dos fuentes de audio se crean automáticamente sobre este mismo
        // GameObject, así no hay que configurarlas en el editor.
        fuenteMusica = gameObject.AddComponent<AudioSource>();
        fuenteMusica.loop        = true;
        fuenteMusica.playOnAwake = false;
        fuenteMusica.volume      = volumenMusica;

        fuenteEfectos = gameObject.AddComponent<AudioSource>();
        fuenteEfectos.loop        = false;
        fuenteEfectos.playOnAwake = false;
        fuenteEfectos.volume      = volumenEfectos;
    }

    // ── Volúmenes ────────────────────────────────────────────────
    public float ObtenerVolumenEfectos() => volumenEfectos;
    public float ObtenerVolumenMusica()  => volumenMusica;

    public void SetVolumenEfectos(float v)
    {
        volumenEfectos = Mathf.Clamp01(v);
        if (fuenteEfectos != null) fuenteEfectos.volume = volumenEfectos;
        PlayerPrefs.SetFloat("VolumenEfectos", volumenEfectos);
    }

    public void SetVolumenMusica(float v)
    {
        volumenMusica = Mathf.Clamp01(v);
        if (fuenteMusica != null) fuenteMusica.volume = volumenMusica;
        PlayerPrefs.SetFloat("VolumenMusica", volumenMusica);
    }

    // ── Música ───────────────────────────────────────────────────

    /// <summary>Reproduce una pista de música en bucle. Si ya está sonando esa misma pista, no la reinicia.</summary>
    public void ReproducirMusica(AudioClip clip)
    {
        if (clip == null || fuenteMusica == null) return;
        if (fuenteMusica.clip == clip && fuenteMusica.isPlaying) return;

        fuenteMusica.clip   = clip;
        fuenteMusica.volume = volumenMusica;
        fuenteMusica.Play();
    }

    public void ReproducirMusicaMenu()    => ReproducirMusica(musicaMenu);
    public void ReproducirMusicaPartida() => ReproducirMusica(musicaPartida);

    public void DetenerMusica()
    {
        if (fuenteMusica != null) fuenteMusica.Stop();
    }

    // ── Efectos de sonido ────────────────────────────────────────

    /// <summary>Reproduce un efecto puntual respetando el volumen de efectos. Ignora clips nulos.</summary>
    public void ReproducirEfecto(AudioClip clip)
    {
        if (clip == null || fuenteEfectos == null) return;
        fuenteEfectos.PlayOneShot(clip, volumenEfectos);
    }

    public void SonarDano()          => ReproducirEfecto(sfxDano);
    public void SonarMuerteEnemigo() => ReproducirEfecto(sfxMuerteEnemigo);
    public void SonarRecogerGema()   => ReproducirEfecto(sfxRecogerGema);
    public void SonarLevelUp()       => ReproducirEfecto(sfxLevelUp);
    public void SonarVictoria()      => ReproducirEfecto(sfxVictoria);
    public void SonarDerrota()       => ReproducirEfecto(sfxDerrota);
}
