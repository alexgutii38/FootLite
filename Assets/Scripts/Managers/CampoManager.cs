using UnityEngine;
using UnityEngine.Rendering;

// ─────────────────────────────────────────────────────────────────
//  CampoManager  –  Cambia estadio, skybox, luz y niebla según el
//  mundo seleccionado en NivelManager.
//
//  SETUP en Unity Editor (EscenaCampo):
//   1. Crea un Empty GameObject llamado "CampoManager" y añade este script.
//   2. Arrastra los 3 GameObjects de estadio a los campos Estadio Mundo 1/2/3.
//   3. (Opcional) Arrastra materiales Skybox a los campos Skybox Mundo 1/2/3.
//      Si los dejas vacíos se genera un skybox procedural automáticamente.
// ─────────────────────────────────────────────────────────────────

public class CampoManager : MonoBehaviour
{
    [Header("Estadios — arrastra los 3 GameObjects del estadio")]
    public GameObject estadioMundo1;
    public GameObject estadioMundo2;
    public GameObject estadioMundo3;

    [Header("Skybox (opcional — si están vacíos se usa el procedural)")]
    public Material skyboxMundo1;
    public Material skyboxMundo2;
    public Material skyboxMundo3;

    // ── Configuración visual por mundo ──────────────────────────
    // Mundo 1: Día soleado (estadio clásico)
    // Mundo 2: Atardecer (tonos cálidos)
    // Mundo 3: Noche dramática (azul oscuro + niebla)

    private static readonly Color[] SKY_TINT = {
        new Color(0.50f, 0.70f, 1.00f),   // M1 – azul día
        new Color(1.00f, 0.45f, 0.10f),   // M2 – naranja atardecer
        new Color(0.04f, 0.04f, 0.18f),   // M3 – noche
    };

    private static readonly Color[] GROUND_COL = {
        new Color(0.28f, 0.35f, 0.18f),   // M1 – verde campo
        new Color(0.30f, 0.16f, 0.10f),   // M2 – tierra rojiza
        new Color(0.03f, 0.03f, 0.08f),   // M3 – negro casi total
    };

    private static readonly Color[] AMBIENT = {
        new Color(0.85f, 0.90f, 1.00f),   // M1 – luz blanca diurna
        new Color(1.00f, 0.65f, 0.35f),   // M2 – luz cálida
        new Color(0.15f, 0.15f, 0.35f),   // M3 – luz azul fría
    };

    // Niebla (solo M2 y M3)
    private static readonly bool[]  FOG_ON      = { false, true,  true  };
    private static readonly Color[] FOG_COLOR   = {
        Color.white,
        new Color(1.00f, 0.55f, 0.20f),   // M2 – niebla anaranjada
        new Color(0.05f, 0.05f, 0.15f),   // M3 – niebla azul oscuro
    };
    private static readonly float[] FOG_DENSITY = { 0f, 0.018f, 0.030f };

    // Luz direccional (sol / luna) por mundo: color, intensidad y orientación.
    private static readonly Color[] LUZ_COLOR = {
        new Color(1.00f, 0.96f, 0.84f),   // M1 – sol cálido neutro
        new Color(1.00f, 0.55f, 0.25f),   // M2 – sol de atardecer
        new Color(0.45f, 0.55f, 0.95f),   // M3 – luna azul fría
    };
    private static readonly float[] LUZ_INTENSIDAD = { 1.15f, 0.95f, 0.40f };
    // Orientación de la luz: X = elevación (bajo = rasante), Y = azimut.
    private static readonly Vector2[] LUZ_ANGULO = {
        new Vector2(50f,  30f),   // M1 – sol alto
        new Vector2(12f, -25f),   // M2 – sol rasante (sombras largas)
        new Vector2(35f,  60f),   // M3 – luna
    };
    // Exposición del skybox procedural por mundo (M3 noche = más oscuro).
    private static readonly float[] SKY_EXPOSURE = { 1.30f, 1.10f, 0.55f };

    private Material skyboxProcedural;

    void Start()
    {
        int mundo = (NivelManager.Instancia != null)
            ? NivelManager.Instancia.mundoSeleccionado
            : 1;

        AplicarMundo(mundo);
    }

    void AplicarMundo(int mundo)
    {
        int idx = Mathf.Clamp(mundo - 1, 0, 2);

        // ── 1. Activar solo el estadio de este mundo ─────────────
        ActivarEstadio(estadioMundo1, mundo == 1);
        ActivarEstadio(estadioMundo2, mundo == 2);
        ActivarEstadio(estadioMundo3, mundo == 3);

        // ── 2. Skybox ────────────────────────────────────────────
        Material[] skyboxesAsignados = { skyboxMundo1, skyboxMundo2, skyboxMundo3 };
        Material skybox = skyboxesAsignados[idx];

        if (skybox != null)
        {
            RenderSettings.skybox = skybox;
        }
        else
        {
            AplicarSkyboxProcedural(SKY_TINT[idx], GROUND_COL[idx], SKY_EXPOSURE[idx]);
        }

        // ── 3. Luz ambiente ───────────────────────────────────────
        RenderSettings.ambientMode  = AmbientMode.Flat;
        RenderSettings.ambientLight = AMBIENT[idx];

        // ── 4. Niebla ─────────────────────────────────────────────
        RenderSettings.fog = FOG_ON[idx];
        if (FOG_ON[idx])
        {
            RenderSettings.fogColor   = FOG_COLOR[idx];
            RenderSettings.fogMode    = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = FOG_DENSITY[idx];
        }

        // ── 5. Luz direccional (sol / luna) ───────────────────────
        AplicarLuzDireccional(idx);

        // ── 6. Actualizar WaveManager con el renderer del suelo ───
        GameObject estadioActivo = ObtenerEstadioActivo(mundo);
        if (estadioActivo != null)
        {
            WaveManager wm = FindFirstObjectByType<WaveManager>();
            if (wm != null)
            {
                // Buscar el renderer más grande (el suelo del estadio)
                Renderer mejor = BuscarRendererSuelo(estadioActivo);
                if (mejor != null)
                {
                    wm.rendererSuelo = mejor;
                    Debug.Log($"[CampoManager] WaveManager.rendererSuelo → {mejor.name} (Mundo {mundo})");
                }
            }
        }

        // Recalcular iluminación global
        DynamicGI.UpdateEnvironment();

        Debug.Log($"[CampoManager] Mundo {mundo} aplicado correctamente.");
    }

    // ── Helpers ──────────────────────────────────────────────────

    void ActivarEstadio(GameObject estadio, bool activo)
    {
        if (estadio != null) estadio.SetActive(activo);
    }

    GameObject ObtenerEstadioActivo(int mundo)
    {
        switch (mundo)
        {
            case 1: return estadioMundo1;
            case 2: return estadioMundo2;
            case 3: return estadioMundo3;
            default: return estadioMundo1;
        }
    }

    void AplicarSkyboxProcedural(Color cielo, Color suelo, float exposure)
    {
        if (skyboxProcedural == null)
        {
            Shader shader = Shader.Find("Skybox/Procedural");
            if (shader == null)
            {
                Debug.LogWarning("[CampoManager] Shader 'Skybox/Procedural' no encontrado.");
                return;
            }
            skyboxProcedural = new Material(shader);
        }

        skyboxProcedural.SetColor("_SkyTint",     cielo);
        skyboxProcedural.SetColor("_GroundColor", suelo);
        skyboxProcedural.SetFloat("_Exposure",    exposure);
        skyboxProcedural.SetFloat("_AtmosphereThickness", 1.0f);

        RenderSettings.skybox = skyboxProcedural;
    }

    /// <summary>
    /// Ajusta el color, la intensidad y la orientación de la luz direccional
    /// (sol o luna) según el mundo, para que cada uno tenga su propia
    /// identidad visual. Usa RenderSettings.sun y, si no está definido,
    /// busca la primera luz direccional de la escena.
    /// </summary>
    void AplicarLuzDireccional(int idx)
    {
        Light luz = RenderSettings.sun;

        if (luz == null)
        {
            foreach (Light l in FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (l.type == LightType.Directional) { luz = l; break; }
            }
        }

        if (luz == null)
        {
            Debug.LogWarning("[CampoManager] No se encontró ninguna luz direccional.");
            return;
        }

        luz.color     = LUZ_COLOR[idx];
        luz.intensity = LUZ_INTENSIDAD[idx];
        luz.transform.rotation = Quaternion.Euler(LUZ_ANGULO[idx].x, LUZ_ANGULO[idx].y, 0f);
    }

    // Devuelve el Renderer con mayor área de bounds (probablemente el suelo)
    Renderer BuscarRendererSuelo(GameObject estadio)
    {
        Renderer[] renderers = estadio.GetComponentsInChildren<Renderer>();
        Renderer mejor = null;
        float mayorVolumen = 0f;

        foreach (Renderer r in renderers)
        {
            // Preferir renderers con "suelo", "ground", "floor", "campo" en el nombre
            string nombre = r.gameObject.name.ToLower();
            if (nombre.Contains("suelo") || nombre.Contains("ground") ||
                nombre.Contains("floor") || nombre.Contains("campo"))
                return r;

            float vol = r.bounds.size.x * r.bounds.size.z;
            if (vol > mayorVolumen)
            {
                mayorVolumen = vol;
                mejor = r;
            }
        }

        return mejor;
    }
}
