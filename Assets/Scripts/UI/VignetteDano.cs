using UnityEngine;
using UnityEngine.UI;

public class VignetteDano : MonoBehaviour
{
    [Header("Configuración")]
    public float intensidadMaxima = 0.85f;   // Alpha máximo (0-1)
    public float velocidadAparicion = 8f;     // Qué rápido aparece el rojo
    public float velocidadDesaparicion = 2f;  // Qué rápido se desvanece

    private Image imagenVignette;
    private float alphaObjetivo = 0f;

    void Awake()
    {
        imagenVignette = GetComponent<Image>();

        // Aseguramos que empieza invisible
        SetAlpha(0f);
    }

    // Llamado desde PlayerStats al recibir daño
    public void MostrarEfecto()
    {
        alphaObjetivo = intensidadMaxima;
    }

    void Update()
    {
        if (imagenVignette == null) return;

        if (Time.timeScale == 0f)
        {
            alphaObjetivo = 0f;
            SetAlpha(0f);
            return;
        }

        float alphaActual = imagenVignette.color.a;

        if (alphaActual < alphaObjetivo)
        {
            // Aparece rápido
            SetAlpha(Mathf.MoveTowards(alphaActual, alphaObjetivo, velocidadAparicion * Time.deltaTime));
        }
        else
        {
            // Desaparece lento
            alphaObjetivo = 0f;
            SetAlpha(Mathf.MoveTowards(alphaActual, 0f, velocidadDesaparicion * Time.deltaTime));
        }
    }

    void SetAlpha(float alpha)
    {
        Color c = imagenVignette.color;
        c.a = alpha;
        imagenVignette.color = c;
    }
}
