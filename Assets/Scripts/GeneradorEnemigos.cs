using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GeneradorEnemigos : MonoBehaviour
{
    [Header("Prefab de Enemigo")]
    public GameObject enemigoPrefab;

    [Header("Configuración de Generación")]
    public int cantidadEnemigos = 5;
    public float radioGeneracion = 15f;
    public float intervaloGeneracion = 3f;

    private Transform jugador;
    private float tiempoUltimaGeneracion;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("Player").transform;

        GenerarOleada();
        tiempoUltimaGeneracion = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > tiempoUltimaGeneracion + intervaloGeneracion)
        {
            GenerarOleada();
            tiempoUltimaGeneracion = Time.time;
        }
    }

    void GenerarOleada()
    {
        for (int i = 0; i < cantidadEnemigos; i++)
        {
            float angulo = (360f / cantidadEnemigos) * i;

            float anguloRad = angulo * Mathf.Deg2Rad;

            float x = jugador.position.x + radioGeneracion * Mathf.Cos(anguloRad);
            float z = jugador.position.z + radioGeneracion * Mathf.Sin(anguloRad);

            Vector3 posicionGeneracion = new Vector3(x, 0f, z);

            Instantiate(enemigoPrefab, posicionGeneracion, Quaternion.identity);
        }
    }
}
