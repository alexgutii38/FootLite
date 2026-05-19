using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pool de objetos genérico para evitar el coste de Instantiate/Destroy
/// constante (balas, gemas...). En lugar de crear y destruir, reutiliza
/// instancias inactivas, lo que elimina los picos del recolector de basura
/// (GC) durante las oleadas largas.
///
/// NO necesita configuración en el editor: se crea solo la primera vez que
/// alguien lo usa. Vive dentro de la escena actual, así que sus objetos se
/// limpian automáticamente al cambiar de escena.
///
/// Uso:
///   GameObject obj = ObjectPool.Instancia.Obtener(prefab, posicion, rotacion);
///   ...
///   ObjectPool.Instancia.Devolver(obj);   // en lugar de Destroy(obj)
/// </summary>
public class ObjectPool : MonoBehaviour
{
    private static ObjectPool _instancia;

    /// <summary>Acceso al pool. Si todavía no existe, se crea automáticamente.</summary>
    public static ObjectPool Instancia
    {
        get
        {
            if (_instancia == null)
            {
                GameObject go = new GameObject("ObjectPool");
                _instancia = go.AddComponent<ObjectPool>();
            }
            return _instancia;
        }
    }

    // Una cola de instancias libres por cada prefab.
    private readonly Dictionary<GameObject, Queue<GameObject>> pools =
        new Dictionary<GameObject, Queue<GameObject>>();

    void Awake()
    {
        if (_instancia != null && _instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        _instancia = this;
    }

    void OnDestroy()
    {
        if (_instancia == this) _instancia = null;
    }

    /// <summary>
    /// Devuelve una instancia del prefab lista para usar: reutiliza una libre
    /// si la hay, o crea una nueva si no. Equivale a Instantiate.
    /// </summary>
    public GameObject Obtener(GameObject prefab, Vector3 posicion, Quaternion rotacion)
    {
        if (prefab == null) return null;

        if (!pools.TryGetValue(prefab, out Queue<GameObject> cola))
        {
            cola = new Queue<GameObject>();
            pools[prefab] = cola;
        }

        GameObject obj = null;
        while (cola.Count > 0)
        {
            obj = cola.Dequeue();
            if (obj != null) break;   // descartar entradas que ya fueron destruidas
            obj = null;
        }

        if (obj == null)
        {
            obj = Instantiate(prefab);
            PooledObject marca = obj.GetComponent<PooledObject>();
            if (marca == null) marca = obj.AddComponent<PooledObject>();
            marca.prefabOrigen = prefab;
            obj.transform.SetParent(transform, false);
        }

        obj.transform.SetPositionAndRotation(posicion, rotacion);
        obj.SetActive(true);
        return obj;
    }

    /// <summary>
    /// Devuelve una instancia al pool (la desactiva para reutilizarla más
    /// tarde). Si el objeto no proviene de un pool, simplemente se destruye.
    /// </summary>
    public void Devolver(GameObject instancia)
    {
        if (instancia == null) return;

        PooledObject marca = instancia.GetComponent<PooledObject>();
        if (marca == null || marca.prefabOrigen == null)
        {
            Destroy(instancia);   // no salió del pool: comportamiento normal
            return;
        }

        instancia.SetActive(false);

        if (!pools.TryGetValue(marca.prefabOrigen, out Queue<GameObject> cola))
        {
            cola = new Queue<GameObject>();
            pools[marca.prefabOrigen] = cola;
        }
        cola.Enqueue(instancia);
    }
}
