using UnityEngine;

// Pon este script en CUALQUIER GameObject de la escena (no necesita estar en la cámara).
// El [DefaultExecutionOrder(9999)] garantiza que nuestro LateUpdate corre DESPUÉS de
// Cinemachine y cualquier otro script que mueva la cámara.
[DefaultExecutionOrder(9999)]
public class CamaraTemblar : MonoBehaviour
{
    public static CamaraTemblar Instancia;

    [Header("Valores del shake")]
    public float duracion = 0.25f;
    public float magnitud = 0.20f;

    private Transform camTransform;
    private Vector3 ultimoOffset = Vector3.zero;
    private float shakeTimer    = 0f;
    private float shakeDuracion = 0f;
    private float shakeMagnitud = 0f;

    void Awake()
    {
        Instancia = this;
    }

    void Start()
    {
        if (Camera.main != null)
            camTransform = Camera.main.transform;
        else
            Debug.LogWarning("CamaraTemblar: no hay cámara con tag 'MainCamera'.");
    }

    public void Temblar() => Temblar(duracion, magnitud);
    public void Temblar(float dur, float mag)
    {
        shakeTimer    = dur;
        shakeDuracion = dur;
        shakeMagnitud = mag;
    }

    void LateUpdate()
    {
        if (camTransform == null) return;

        camTransform.localPosition -= ultimoOffset;

        if (shakeTimer > 0f)
        {
            float t = shakeTimer / shakeDuracion;
            ultimoOffset   = Random.insideUnitSphere * shakeMagnitud * t;
            ultimoOffset.z = 0f;

            camTransform.localPosition += ultimoOffset;
            shakeTimer -= Time.unscaledDeltaTime;
        }
        else
        {
            ultimoOffset = Vector3.zero;
        }
    }
}
