using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Aplica ajustes de calidad gráfica al arrancar el juego. NO necesita
/// ningún GameObject ni configuración: el método marcado con
/// [RuntimeInitializeOnLoadMethod] se ejecuta automáticamente al iniciar.
///
/// Mejora visible (bordes más suaves, texturas más nítidas en ángulo,
/// sombras menos pixeladas) a cambio de un coste de rendimiento moderado.
/// Si hace falta bajarlo, ajusta los valores de AplicarCalidad().
/// </summary>
public static class AjustesGraficos
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Inicializar()
    {
        AplicarCalidad();

        // Evita doble suscripción si el método se reejecuta (entrar en Play
        // sin recarga de dominio en el editor).
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void AplicarCalidad()
    {
        // 4x MSAA: suaviza los bordes dentados de los modelos.
        QualitySettings.antiAliasing = 4;

        // Filtrado anisotrópico: mantiene nítidas las texturas vistas en
        // ángulo (sobre todo el césped del campo).
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;

        // Sombras de mayor resolución (bordes menos pixelados).
        QualitySettings.shadowResolution = ShadowResolution.High;
    }

    static void OnSceneLoaded(Scene escena, LoadSceneMode modo)
    {
        // El MSAA global solo surte efecto si la cámara lo permite. Lo está
        // por defecto, pero lo forzamos en cada escena por si acaso.
        Camera cam = Camera.main;
        if (cam != null) cam.allowMSAA = true;
    }
}
