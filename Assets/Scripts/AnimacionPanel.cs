using UnityEngine;
using System.Collections;

public class AnimacionPanel : MonoBehaviour
{
    private Vector3 escalaOriginal;

    // Awake se ejecuta al principio del todo, antes de que el panel se apague por primera vez
    void Awake()
    {
        // Guardamos el tamaño que le has puesto en el editor de Unity
        escalaOriginal = transform.localScale;
    }

    void OnEnable()
    {
        StartCoroutine(EfectoPopElastico());
    }

    private IEnumerator EfectoPopElastico()
    {
        // Empezamos con el panel encogido a tamaño 0
        transform.localScale = Vector3.zero;
        
        float tiempo = 0f;
        float duracion = 0.35f; // Un tercio de segundo

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime; 
            float progreso = tiempo / duracion;
            
            // Fórmula matemática para el rebote
            float rebote = Mathf.Sin(progreso * Mathf.PI * 0.5f) + Mathf.Sin(progreso * Mathf.PI) * 0.25f; 

            // Multiplicamos el rebote por TU escala original
            transform.localScale = escalaOriginal * rebote;
            yield return null;
        }
        
        // Lo dejamos clavado en el tamaño que tú elegiste
        transform.localScale = escalaOriginal;
    }
}