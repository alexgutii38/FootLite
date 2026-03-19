using UnityEngine;

public class GemaVelAtq : GemaBase
{
    protected override void AplicarEfecto(Transform jugador)
    {
        PlayerStats stats = jugador.GetComponent<PlayerStats>();
        if (stats != null) stats.ModificarVelAtq(valorGema);
    }
}

