using UnityEngine;
using System.Collections.Generic;

public class DisparoAutomatico : MonoBehaviour
{
    public GameObject balaPrefab;
    private Transform puntoDisparo;

    private float cadenciaDisparo;
    private float rangoDisparo;
    private int   maxBalasSimultaneas = 1;
    public  AudioClip   sonidoChute;
    public  AudioSource audioSource;
    private float siguienteDisparo = 0f;

    private PlayerStats statsCache;
    private LayerMask   enemiesLayer;

    // Buffer reutilizable para evitar allocations de LINQ
    private static Collider[] collidersBuffer = new Collider[64];

    void Start()
    {
        statsCache   = GetComponent<PlayerStats>();
        enemiesLayer = LayerMask.GetMask("Enemies");

        puntoDisparo = transform.Find("PuntoDisparo");
        if (puntoDisparo == null)
        {
            GameObject temp = new GameObject("PuntoDisparo");
            temp.transform.SetParent(transform);
            temp.transform.localPosition = Vector3.zero;
            puntoDisparo = temp.transform;
        }

        ActualizarStats();
    }

    void Update()
    {
        ActualizarStats();

        if (Time.time >= siguienteDisparo)
        {
            DispararAEnemigos();
            siguienteDisparo = Time.time + cadenciaDisparo;
        }
    }

    void ActualizarStats()
    {
        if (statsCache == null) return;
        cadenciaDisparo    = statsCache.cadenciaDisparo;
        rangoDisparo       = statsCache.rangoDisparo;
        maxBalasSimultaneas = 1 + statsCache.cantidadDirecciones;
    }

    void DispararAEnemigos()
    {
        int count = Physics.OverlapSphereNonAlloc(puntoDisparo.position, rangoDisparo,
            collidersBuffer, enemiesLayer);

        if (count == 0) return;

        // Ordenar los primeros `count` elementos por distancia sin LINQ (sin GC)
        float volEfectos = AudioManager.Instancia != null
            ? AudioManager.Instancia.ObtenerVolumenEfectos()
            : PlayerPrefs.GetFloat("VolumenEfectos", 1f);

        int disparados = 0;
        // Selección simple: encontrar el más cercano, disparar, marcarlo nulo, repetir
        for (int t = 0; t < maxBalasSimultaneas && t < count; t++)
        {
            float mejorDist = float.MaxValue;
            int   mejorIdx  = -1;

            for (int i = 0; i < count; i++)
            {
                if (collidersBuffer[i] == null) continue;
                float d = Vector3.SqrMagnitude(puntoDisparo.position - collidersBuffer[i].transform.position);
                if (d < mejorDist) { mejorDist = d; mejorIdx = i; }
            }

            if (mejorIdx < 0) break;

            Collider objetivo = collidersBuffer[mejorIdx];
            collidersBuffer[mejorIdx] = null; // marcado como usado

            Vector3 direccion = (objetivo.transform.position - puntoDisparo.position).normalized;
            Instantiate(balaPrefab, puntoDisparo.position, Quaternion.LookRotation(direccion));

            if (audioSource != null && sonidoChute != null)
                audioSource.PlayOneShot(sonidoChute, volEfectos);

            disparados++;
        }

        // Limpiar buffer para el próximo uso
        for (int i = 0; i < count; i++) collidersBuffer[i] = null;
    }
}
