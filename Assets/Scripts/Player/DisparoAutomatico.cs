using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Necesario para ordenar la lista de enemigos

public class DisparoAutomatico : MonoBehaviour
{
    public GameObject balaPrefab;
    private Transform puntoDisparo;

    private float cadenciaDisparo;
    private float rangoDisparo;
    private int maxBalasSimultaneas = 1;
    public AudioClip sonidoChute;
    public AudioSource audioSource;
    private float siguienteDisparo = 0f;

    private PlayerStats statsCache;

    void Start()
    {
        statsCache = GetComponent<PlayerStats>();

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
        cadenciaDisparo = statsCache.cadenciaDisparo;
        rangoDisparo = statsCache.rangoDisparo;
        maxBalasSimultaneas = 1 + statsCache.cantidadDirecciones;
    }

    void DispararAEnemigos()
    {
        Collider[] colls = Physics.OverlapSphere(puntoDisparo.position, rangoDisparo, LayerMask.GetMask("Enemies"));
        if (colls.Length == 0) return;

        var enemigosOrdenados = colls
            .OrderBy(c => Vector3.Distance(puntoDisparo.position, c.transform.position))
            .Take(maxBalasSimultaneas)
            .ToList();

        float volEfectos = AudioManager.Instancia != null
            ? AudioManager.Instancia.ObtenerVolumenEfectos()
            : PlayerPrefs.GetFloat("VolumenEfectos", 1f);

        foreach (var enemigo in enemigosOrdenados)
        {
            if (enemigo == null) continue;
            Vector3 direccion = (enemigo.transform.position - puntoDisparo.position).normalized;
            Instantiate(balaPrefab, puntoDisparo.position, Quaternion.LookRotation(direccion));
            if (audioSource != null && sonidoChute != null)
                audioSource.PlayOneShot(sonidoChute, volEfectos);
        }
    }
}