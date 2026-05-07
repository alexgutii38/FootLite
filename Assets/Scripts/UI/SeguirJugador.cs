using UnityEngine;

// Script de seguimiento de cámara como FALLBACK.
// Se desactiva automáticamente si detecta cualquier componente de Cinemachine
// en la misma cámara (en ese caso, Cinemachine ya gestiona el seguimiento).
public class SeguirJugador : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform objetivo;

    [Header("Offset desde el jugador")]
    public Vector3 offset = new Vector3(0f, 12f, -8f);

    [Header("Suavizado")]
    public float suavizado = 6f;

    private Vector3 velocidad;

    void Awake()
    {
        // Buscar cualquier componente de Cinemachine en este mismo GameObject
        foreach (MonoBehaviour mb in GetComponents<MonoBehaviour>())
        {
            if (mb == this) continue;
            if (mb.GetType().Name.IndexOf("Cinemachine", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Debug.Log("[SeguirJugador] Cinemachine detectado en este GameObject — SeguirJugador desactivado.");
                enabled = false;
                return;
            }
        }
    }

    void Start()
    {
        if (!enabled) return;

        if (objetivo == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) objetivo = p.transform;
        }

        if (objetivo != null)
            transform.position = objetivo.position + offset;
    }

    void LateUpdate()
    {
        if (objetivo == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) objetivo = p.transform;
            else return;
        }

        Vector3 posObjetivo = objetivo.position + offset;
        transform.position = Vector3.SmoothDamp(
            transform.position, posObjetivo, ref velocidad,
            suavizado > 0 ? 1f / suavizado : 0.001f
        );

        transform.LookAt(objetivo.position + Vector3.up * 1.2f);
    }
}
