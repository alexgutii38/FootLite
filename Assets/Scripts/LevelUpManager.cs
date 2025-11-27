using UnityEngine;

public enum Raridad 
{
    Comun,
    Raro,
    Epico,
    Legendario
}

public enum TipoMejora
{
    VidaMaxima,
    Daño,
    Cadencia,
    VelocidadMovimiento,
    Rango,
    Regeneracion,
    Suerte,
    Direcciones,
    VelocidadProyectil,

    VelocidadExp,
}
public class LevelUpManager : MonoBehaviour
{
    [System.Serializable]
    public struct OpcionMejora
    {
        public TipoMejora tipo;
        public Raridad raridad;
    }

    public TMPro.TextMeshProUGUI textoOpcion1;
    public TMPro.TextMeshProUGUI textoOpcion2;  
    public TMPro.TextMeshProUGUI textoOpcion3;
    private OpcionMejora[] opciones = new OpcionMejora[3];
    private PlayerStats statsActual;

    [Header("UI")]
    public GameObject panelLevelUp;

    public void MostrarOpciones(PlayerStats stats)
    {
        statsActual = stats;

        Time.timeScale = 0f;
        GameManager.Instancia.juegoEnPausa = true;

        if (panelLevelUp != null)
            panelLevelUp.SetActive(true);

        for (int i = 0; i < 3; i++)
        {
            opciones[i] = GenerarOpcion(statsActual);
        }

        if (textoOpcion1 != null)
            textoOpcion1.text = DescribirOpcion(opciones[0]);
        if (textoOpcion2 != null)
            textoOpcion2.text = DescribirOpcion(opciones[1]);
        if (textoOpcion3 != null)
            textoOpcion3.text = DescribirOpcion(opciones[2]);
    }

    OpcionMejora GenerarOpcion(PlayerStats stats)
    {
        OpcionMejora op = new OpcionMejora();
        
        int tiposCount = System.Enum.GetValues(typeof(TipoMejora)).Length;
        op.tipo = (TipoMejora)Random.Range(0, tiposCount);

        op.raridad = GenerarRaridad(stats);

        return op;
    }

    string DescribirOpcion(OpcionMejora op)
    {
        string nombreTipo = op.tipo.ToString();
        string nombreRaridad = op.raridad.ToString();

        return nombreRaridad + " " + nombreTipo;
    }

    public void ElegirMejora1() {AplicarMejora(opciones[0]);}
    public void ElegirMejora2() {AplicarMejora(opciones[1]);}
    public void ElegirMejora3() {AplicarMejora(opciones[2]);}

    Raridad GenerarRaridad(PlayerStats stats)
    {
        float pComun = 0.60f;
        float pRaro = 0.25f;
        float pEpico = 0.10f; 
        float pLegendario = 0.05f;

        float bonus = stats != null ? stats.suerte * 0.001f : 0f; 

        float quitarDeComun = Mathf.Min(pComun * 0.5f, bonus); 

        pComun -= quitarDeComun;
        pRaro += quitarDeComun * 0.5f;
        pEpico += quitarDeComun * 0.3f;
        pLegendario += quitarDeComun * 0.2f;

        float suma = pComun + pRaro + pEpico + pLegendario;
        pComun /= suma; pRaro /= suma; pEpico /= suma; pLegendario /= suma;

        float r = Random.value;

        if (r < pComun) return Raridad.Comun;
        r -= pComun;
        if (r < pRaro) return Raridad.Raro;
        r -= pRaro;
        if (r < pEpico) return Raridad.Epico;
        return Raridad.Legendario;
    }

    void AplicarMejora(OpcionMejora op)
    {
        if (statsActual == null)
        {
            CerrarMenu();
            return;
        }

        float multComun = 2.0f;
        float multRaro = 3.0f;
        float multEpico = 4.5f;
        float multLegendario = 6.0f;

        float mult;

        switch (op.raridad)
        {
            default:
            case Raridad.Comun:
                mult = multComun;
                break;
            case Raridad.Raro:
                mult = multRaro;
                break;
            case Raridad.Epico:
                mult = multEpico;
                break;
            case Raridad.Legendario:
                mult = multLegendario;
                break;
            
        }

        switch (op.tipo)
        {
            case TipoMejora.VidaMaxima:
                statsActual.vidaMaxima = Mathf.RoundToInt(10*mult);
                statsActual.vidaActual = statsActual.vidaMaxima;
                break;

            case TipoMejora.Daño:
                statsActual.danoProyectil *= Mathf.RoundToInt(1f + 0.15f * mult);
                break;

            case TipoMejora.Cadencia:
                statsActual.cadenciaDisparo *= (1f - 0.08f * mult);
                break;

            case TipoMejora.VelocidadMovimiento:
                statsActual.speed *= (1f + 0.10f * mult);
                break;

            case TipoMejora.Rango:
                statsActual.rangoDisparo *= (1f + 0.12f * mult);
                break;

            case TipoMejora.Regeneracion:
                statsActual.regeneracionVida += 0.5f * mult;
                break;

            case TipoMejora.VelocidadExp:
                // Aumenta la velocidad de ganancia de experiencia
                statsActual.multiplicadorExperiencia *= (1f + 0.10f* mult);
                break;
            case TipoMejora.Suerte:
                statsActual.suerte += 5f * mult;
                break;

            case TipoMejora.Direcciones:
                statsActual.cantidadDirecciones += Mathf.RoundToInt(1 * mult);
                break;

            case TipoMejora.VelocidadProyectil:
                statsActual.velocidadProyectil *= (1f + 0.15f * mult);
                break;
        }
        CerrarMenu();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void CerrarMenu()
    {
        if (panelLevelUp != null)
            panelLevelUp.SetActive(false);

        Time.timeScale = 1f;
        GameManager.Instancia.juegoEnPausa = false;
        GameManager.Instancia.ActualizarUI();
    }
}
