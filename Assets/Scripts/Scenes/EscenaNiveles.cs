using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EscenaNiveles : MonoBehaviour
{
    [Header("Widgets de imagen de cada nivel (en orden: Nivel 1, 2, 3)")]
    public Image imagenNivel1;
    public Image imagenNivel2;
    public Image imagenNivel3;

    [Header("Sprites Mundo 1 (N1, N2, N3)")]
    public Sprite mundo1Nivel1;
    public Sprite mundo1Nivel2;
    public Sprite mundo1Nivel3;

    [Header("Sprites Mundo 2 (N1, N2, N3)")]
    public Sprite mundo2Nivel1;
    public Sprite mundo2Nivel2;
    public Sprite mundo2Nivel3;

    [Header("Sprites Mundo 3 (N1, N2, N3)")]
    public Sprite mundo3Nivel1;
    public Sprite mundo3Nivel2;
    public Sprite mundo3Nivel3;

    void Start()
    {
        int mundo = NivelManager.Instancia != null ? NivelManager.Instancia.mundoSeleccionado : 1;
        Sprite s1, s2, s3;

        switch (mundo)
        {
            case 2:
                s1 = mundo2Nivel1; s2 = mundo2Nivel2; s3 = mundo2Nivel3; break;
            case 3:
                s1 = mundo3Nivel1; s2 = mundo3Nivel2; s3 = mundo3Nivel3; break;
            default:
                s1 = mundo1Nivel1; s2 = mundo1Nivel2; s3 = mundo1Nivel3; break;
        }

        AsignarSprite(imagenNivel1, s1);
        AsignarSprite(imagenNivel2, s2);
        AsignarSprite(imagenNivel3, s3);
    }

    void AsignarSprite(Image img, Sprite sprite)
    {
        if (img == null || sprite == null) return;
        img.sprite = sprite;
    }

    public void SeleccionarNivel(int nivel)
    {
        NivelManager.Instancia.SeleccionarNivel(nivel);
    }

    public void Volver()
    {
        SceneManager.LoadScene("EscenaMundos");
    }
}
