using UnityEngine;

/// <summary>
/// Marca que <see cref="ObjectPool"/> añade automáticamente a cada instancia
/// que gestiona. Solo guarda de qué prefab proviene, para saber a qué cola
/// devolverla. No hay que añadirlo a mano en el editor.
/// </summary>
public class PooledObject : MonoBehaviour
{
    [HideInInspector] public GameObject prefabOrigen;
}
