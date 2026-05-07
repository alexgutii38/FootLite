using UnityEngine;
using UnityEngine.Rendering;

// Añade este script al GameObject raíz de CADA estadio (Estadio_Mundo1, 2 y 3).
// Elimina las sombras y optimiza el rendering de toda la geometría del estadio.
[DefaultExecutionOrder(-50)]
public class EstadioOptimizer : MonoBehaviour
{
    void Awake()
    {
        int total = 0;
        foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
        {
            r.shadowCastingMode    = ShadowCastingMode.Off;
            r.receiveShadows       = false;
            r.lightProbeUsage      = LightProbeUsage.Off;
            r.reflectionProbeUsage = ReflectionProbeUsage.Off;
            r.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            total++;
        }
        Debug.Log($"[EstadioOptimizer] {gameObject.name}: {total} renderers optimizados (sombras OFF).");
    }
}
