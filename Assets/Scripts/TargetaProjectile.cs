using UnityEngine;

public class TarjetaProjectile : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 12f;
    [SerializeField] private float tiempoVida = 3f;

    [Header("Daño")]
    [SerializeField] private int dano = 8;

    [Header("Visual")]
    [SerializeField] private Renderer rendererTarjeta; // Asigna el MeshRenderer aquí (recomendado)

    private Vector3 direccion;
    private float t;
    private MaterialPropertyBlock mpb;

    public void Inicializar(Vector3 dir)
    {
        direccion = dir.normalized;
        if (direccion == Vector3.zero) direccion = transform.forward;
        transform.rotation = Quaternion.LookRotation(direccion);
    }

    // Llamar justo después de instanciar para poner tipo (amarilla/roja)
    public void Configurar(int nuevoDano, Color color)
    {
        dano = nuevoDano;

        if (rendererTarjeta == null)
            rendererTarjeta = GetComponentInChildren<Renderer>();

        if (rendererTarjeta == null) return;

        if (mpb == null) mpb = new MaterialPropertyBlock();
        rendererTarjeta.GetPropertyBlock(mpb);

        // URP suele usar _BaseColor, pero por compatibilidad tocamos también _Color
        mpb.SetColor("_BaseColor", color);
        mpb.SetColor("_Color", color);

        rendererTarjeta.SetPropertyBlock(mpb);
    }

    private void Update()
    {
        transform.position += direccion * velocidad * Time.deltaTime;

        t += Time.deltaTime;
        if (t >= tiempoVida) Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats ps = other.GetComponent<PlayerStats>();
            if (ps != null) ps.RecibirDano(dano);
            Destroy(gameObject);
            return;
        }

        // Ignora enemigos (por si no tenéis capas/collision matrix perfecto)
        if (other.CompareTag("Enemigo") || other.CompareTag("Enemies"))
            return;

        // Escenario/paredes/etc.
        Destroy(gameObject);
    }
}


