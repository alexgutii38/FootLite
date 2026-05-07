using UnityEngine;

// Coloca este script en un Empty GameObject (ej. "CameraController").
// Desactiva Cinemachine automáticamente y controla la cámara directamente.
// [DefaultExecutionOrder(9998)] garantiza que corre ANTES que CamaraTemblar (9999).
[DefaultExecutionOrder(9998)]
public class CamaraFollowDirecto : MonoBehaviour
{
    [Header("Objetivo (dejar vacío = busca Player automáticamente)")]
    public Transform objetivo;

    [Header("Posición relativa al jugador")]
    public Vector3 offset = new Vector3(0f, 8f, -10f);

    [Header("Altura hacia donde mira")]
    public float alturaLookAt = 1.2f;

    [Header("Suavizado (mayor = más rápido)")]
    public float suavizado = 6f;

    private Transform camTransform;
    private Vector3 velocidad;

    void Awake()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("[CamaraFollowDirecto] No se encontró Main Camera.");
            return;
        }

        camTransform = cam.transform;

        // Desactivar todos los scripts de Cinemachine en la cámara principal
        foreach (MonoBehaviour mb in cam.GetComponents<MonoBehaviour>())
        {
            string typeName = mb.GetType().Name;
            if (typeName.IndexOf("Cinemachine", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                mb.enabled = false;
                Debug.Log($"[CamaraFollowDirecto] Cinemachine desactivado: {typeName}");
            }
        }
    }

    void Start()
    {
        if (objetivo == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) objetivo = p.transform;
        }

        // Teleportar la cámara a la posición correcta desde el primer frame
        if (camTransform != null && objetivo != null)
        {
            camTransform.position = objetivo.position + offset;
            camTransform.LookAt(objetivo.position + Vector3.up * alturaLookAt);
        }
    }

    void LateUpdate()
    {
        if (camTransform == null) return;

        if (objetivo == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) objetivo = p.transform;
            else return;
        }

        Vector3 posObjetivo = objetivo.position + offset;
        camTransform.position = Vector3.SmoothDamp(
            camTransform.position, posObjetivo, ref velocidad,
            suavizado > 0f ? 1f / suavizado : 0.001f
        );

        camTransform.LookAt(objetivo.position + Vector3.up * alturaLookAt);
    }
}
