using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Necesario para ordenar la lista de enemigos

public class DisparoAutomatico : MonoBehaviour
{
    public GameObject balaPrefab;
    private Transform puntoDisparo;
   
    private float cadenciaDisparo;
    private float rangoDisparo;
    private int maxBalasSimultaneas = 1; // Renombrado para que se entienda mejor (antes numDirecciones)
    public AudioClip sonidoChute;
    public AudioSource audioSource;
    private float siguienteDisparo = 0f;

    void Start()
    {
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
        PlayerStats stats = GetComponent<PlayerStats>();
        if (stats != null)
        {
            cadenciaDisparo = stats.cadenciaDisparo;
            rangoDisparo = stats.rangoDisparo;
            
            maxBalasSimultaneas = 1+ stats.cantidadDirecciones;
        }
    }

    void DispararAEnemigos()
    {
        // 1. Buscar todos los enemigos en rango
        Collider[] colls = Physics.OverlapSphere(puntoDisparo.position, rangoDisparo, LayerMask.GetMask("Enemies"));
       
        if (colls.Length == 0) return;

        // 2. Ordenarlos por distancia (de más cerca a más lejos)
        var enemigosOrdenados = colls
            .OrderBy(c => Vector3.Distance(puntoDisparo.position, c.transform.position))
            .Take(maxBalasSimultaneas) // Coger solo los N primeros
            .ToList();

        // 3. Disparar a cada uno de los seleccionados
        foreach (var enemigo in enemigosOrdenados)
        {
            if (enemigo != null)
            {
                Vector3 direccion = (enemigo.transform.position - puntoDisparo.position).normalized;
               
                // Orientar punto de disparo (opcional, visual)
                // puntoDisparo.rotation = Quaternion.LookRotation(direccion);

                // Crear bala mirando al enemigo
                Instantiate(balaPrefab, puntoDisparo.position, Quaternion.LookRotation(direccion));
                audioSource.PlayOneShot(sonidoChute);
            }
        }
    }
}