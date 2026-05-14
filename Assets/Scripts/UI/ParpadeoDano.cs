using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Efecto de "feedback de impacto" para enemigos: al recibir daño da un
/// pequeño tirón de escala (scale punch) y un destello de color sobre el
/// material. Mejora mucho la sensación de disparo sin depender de
/// animaciones nuevas.
///
/// No necesita configuración en el editor: los scripts de enemigo lo añaden
/// solos en tiempo de ejecución (gameObject.AddComponent) si no está
/// presente, y llaman a Flash() al recibir daño.
/// </summary>
[DisallowMultipleComponent]
public class ParpadeoDano : MonoBehaviour
{
    [Header("Ajustes del efecto")]
    [Tooltip("Duración del destello, en segundos.")]
    public float duracion = 0.1f;

    [Tooltip("Cuánto crece el enemigo en el pico del tirón (1 = sin tirón).")]
    public float intensidadEscala = 1.15f;

    [Tooltip("Color del destello. Valores >1 (HDR) hacen un flash más brillante.")]
    [ColorUsage(true, true)]
    public Color colorFlash = new Color(3f, 3f, 3f, 1f);

    private Renderer[] renderers;
    private Vector3    escalaBase;
    private float      timer;
    private bool       activo;

    private MaterialPropertyBlock mpbFlash;
    private MaterialPropertyBlock mpbVacio;

    void Awake()
    {
        escalaBase = transform.localScale;

        // Renderers visibles del enemigo (se excluyen los de partículas).
        List<Renderer> lista = new List<Renderer>();
        foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
            if (!(r is ParticleSystemRenderer)) lista.Add(r);
        renderers = lista.ToArray();

        // Un bloque con el tinte del destello y otro vacío para restaurar.
        mpbFlash = new MaterialPropertyBlock();
        mpbFlash.SetColor("_BaseColor", colorFlash);   // URP
        mpbFlash.SetColor("_Color", colorFlash);       // Built-in / compatibilidad
        mpbVacio = new MaterialPropertyBlock();
    }

    /// <summary>Dispara el efecto de impacto. Se puede llamar repetidamente.</summary>
    public void Flash()
    {
        timer = duracion;
        if (!activo)
        {
            activo = true;
            AplicarBloque(mpbFlash);
        }
    }

    void Update()
    {
        if (!activo) return;

        timer -= Time.deltaTime;
        float t = Mathf.Clamp01(timer / duracion);   // va de 1 → 0 durante el efecto

        // Tirón de escala: parte de escalaBase * intensidad y vuelve a escalaBase.
        transform.localScale = escalaBase * Mathf.Lerp(1f, intensidadEscala, t);

        if (timer <= 0f)
        {
            activo = false;
            transform.localScale = escalaBase;
            AplicarBloque(mpbVacio);   // restaura el color original del material
        }
    }

    void AplicarBloque(MaterialPropertyBlock bloque)
    {
        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i] != null) renderers[i].SetPropertyBlock(bloque);
    }
}
