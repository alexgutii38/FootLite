using UnityEngine;
using System.Collections.Generic;

public class DisparoAutomatico : MonoBehaviour
{
    public GameObject balaPrefab;
    [SerializeField] private Transform puntoDisparo;
    private float cadenciaDisparo;
    private float rangoDisparo;
    private int numDirecciones = 0; // 0 = solo hacia enemigos, >0 = modo circular

    private float siguienteDisparo = 0f;

    void Start()
    {
        PlayerStats stats = GetComponent<PlayerStats>();

        if (stats == null)
        {
            Debug.LogError("DisparoAutomatico: No se encontró PlayerStats en el objeto.");
            return;
        }

        cadenciaDisparo = stats.cadenciaDisparo;
        rangoDisparo = stats.rangoDisparo;
        numDirecciones =  stats.cantidadDirecciones;

    }

    void Update()
    {
        if (Time.time >= siguienteDisparo)
        {
            if (Time.time >= siguienteDisparo)
            {
                GameObject objetivo = ObtenerEnemigoMasCercano();
                if (objetivo != null)
                {
                    // Apunta al enemigo
                    Vector3 direccion = (objetivo.transform.position - puntoDisparo.position).normalized;
                    puntoDisparo.rotation = Quaternion.LookRotation(direccion);

                    // Dispara la bala
                    Instantiate(balaPrefab, puntoDisparo.position, puntoDisparo.rotation);

                    siguienteDisparo = Time.time + cadenciaDisparo;
                }
            }
            
            /*float paso = 360f / numDirecciones;
            for (int i = 0; i < numDirecciones; i++)
            {
                float angulo = i * paso;
                Vector3 dir = Quaternion.Euler(0, angulo, 0) * Vector3.forward;
                Instantiate(balaPrefab, puntoDisparo.position, Quaternion.LookRotation(dir));
            }
            */


        }
    }

    GameObject ObtenerEnemigoMasCercano()
{
    Collider[] colls = Physics.OverlapSphere(puntoDisparo.position, rangoDisparo, LayerMask.GetMask("Enemies"));
    GameObject masCercano = null;
    float minDist = Mathf.Infinity;

    foreach (var c in colls)
    {
        float dist = Vector3.Distance(puntoDisparo.position, c.transform.position);
        if (dist < minDist)
        {
            minDist = dist;
            masCercano = c.gameObject;
        }
    }
    return masCercano;
}

}