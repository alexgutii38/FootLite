using UnityEngine;

public class GemaVida : GemaBase
{
    protected override void AplicarEfecto(Transform jugador)
    {
        PlayerStats stats = jugador.GetComponent<PlayerStats>();
        if (stats != null) stats.ModificarVida(valorGema);
    }
}

