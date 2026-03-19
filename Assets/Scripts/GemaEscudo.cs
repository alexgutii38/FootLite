using UnityEngine;

public class GemaEscudo : GemaBase
{
    protected override void AplicarEfecto(Transform jugador)
    {
        PlayerStats stats = jugador.GetComponent<PlayerStats>();
        if (stats != null) stats.ModificarEscudo(valorGema);
    }
}
