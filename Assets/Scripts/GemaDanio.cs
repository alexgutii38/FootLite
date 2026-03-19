using UnityEngine;

public class GemaDanio : GemaBase
{
    protected override void AplicarEfecto(Transform jugador)
    {
        PlayerStats stats = jugador.GetComponent<PlayerStats>();
        if (stats != null) stats.ModificarDano(valorGema);
    }
}

