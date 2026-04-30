using UnityEngine;
using System.Collections.Generic;

public enum Raridad { Comun, Raro, Epico, Legendario }

public class LevelUpManager : MonoBehaviour
{
    [Header("Base de datos de mejoras")]
    public List<MejoraConfig> mejorasDisponibles;

    [Header("UI")]
    public GameObject panelLevelUp;
    public TMPro.TextMeshProUGUI textoOpcion1;
    public TMPro.TextMeshProUGUI textoOpcion2;
    public TMPro.TextMeshProUGUI textoOpcion3;

    private struct OpcionGenerada
    {
        public MejoraConfig config;
        public Raridad raridad;
        public float valorAplicar;
    }

    private OpcionGenerada[] opciones = new OpcionGenerada[3];
    private PlayerStats statsJugador;

    public void MostrarOpciones(PlayerStats stats)
    {
        statsJugador = stats;
        Time.timeScale = 0f;
        if (GameManager.Instancia != null)
            GameManager.Instancia.juegoEnPausa = true;
        if (panelLevelUp != null)
            panelLevelUp.SetActive(true);
        GenerarOpciones();
        ActualizarTextoUI();
    }

    void GenerarOpciones()
    {
        if (mejorasDisponibles == null || mejorasDisponibles.Count == 0)
        {
            Debug.LogWarning("LevelUpManager: No hay mejoras configuradas.");
            return;
        }

        List<MejoraConfig> pool = new List<MejoraConfig>(mejorasDisponibles);
        for (int i = 0; i < 3; i++)
        {
            if (pool.Count == 0) break;
            int index = Random.Range(0, pool.Count);
            MejoraConfig config = pool[index];
            pool.RemoveAt(index);
            Raridad raridad = GenerarRaridad(statsJugador);
            float valor = ObtenerValorPorRareza(config, raridad);
            opciones[i].config = config;
            opciones[i].raridad = raridad;
            opciones[i].valorAplicar = valor;
        }
    }

    void ActualizarTextoUI()
    {
        if (textoOpcion1 != null) textoOpcion1.text = DescribirOpcion(opciones[0]);
        if (textoOpcion2 != null) textoOpcion2.text = DescribirOpcion(opciones[1]);
        if (textoOpcion3 != null) textoOpcion3.text = DescribirOpcion(opciones[2]);
    }

    string DescribirOpcion(OpcionGenerada op)
    {
        if (op.config == null) return "Sin mejora";
        return $"[{op.raridad}] {op.config.nombre}\n{op.config.descripcion}";
    }

    float ObtenerValorPorRareza(MejoraConfig config, Raridad raridad)
    {
        if (config == null) return 0f;
        switch (raridad)
        {
            default:
            case Raridad.Comun:      return config.valorComun;
            case Raridad.Raro:       return config.valorRaro;
            case Raridad.Epico:      return config.valorEpico;
            case Raridad.Legendario: return config.valorLegendario;
        }
    }

    Raridad GenerarRaridad(PlayerStats stats)
    {
        float pComun = 0.60f, pRaro = 0.25f, pEpico = 0.10f, pLegendario = 0.05f;
        float suerte = (stats != null) ? stats.suerte : 0f;
        float bonus = Mathf.Clamp01(suerte * 0.001f);
        float quitarDeComun = Mathf.Min(pComun * 0.5f, bonus);
        pComun -= quitarDeComun;
        pRaro  += quitarDeComun * 0.5f;
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

    public void ElegirOpcion1() => AplicarYSalir(0);
    public void ElegirOpcion2() => AplicarYSalir(1);
    public void ElegirOpcion3() => AplicarYSalir(2);

    void AplicarYSalir(int index)
    {
        if (index < 0 || index >= opciones.Length) return;
        AplicarMejora(opciones[index]);
        CerrarMenu();
    }

    void AplicarMejora(OpcionGenerada op)
    {
        if (statsJugador == null || op.config == null) return;
        float v = op.valorAplicar;

        switch (op.config.tipoStat)
        {
            case TipoStat.VidaMax:
                statsJugador.vidaMaxima += Mathf.RoundToInt(v);
                statsJugador.vidaActual = statsJugador.vidaMaxima;
                break;

            case TipoStat.Daño:
                statsJugador.danoProyectil *= (1f + v);
                break;

            case TipoStat.Cadencia:
                statsJugador.cadenciaDisparo *= (1f - v);
                break;

            case TipoStat.VelocidadMovimiento:
                statsJugador.speed *= (1f + v);
                break;

            case TipoStat.Direcciones:
                statsJugador.cantidadDirecciones += Mathf.RoundToInt(v);
                break;

            case TipoStat.Regeneracion:       // ← NUEVO
                statsJugador.regeneracionVida += v;
                break;

            case TipoStat.VelocidadProyectil: // ← NUEVO
                statsJugador.velocidadProyectil *= (1f + v);
                break;

            case TipoStat.Rango:              // ← NUEVO
                statsJugador.rangoDisparo += v;
                break;

            case TipoStat.VelocidadXP:        // ← NUEVO
                statsJugador.multiplicadorExperiencia += v;
                break;

            case TipoStat.Suerte:             // ← NUEVO
                statsJugador.suerte += v;
                break;
        }
    }

    void CerrarMenu()
    {
        if (panelLevelUp != null)
            panelLevelUp.SetActive(false);
        Time.timeScale = 1f;
        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.juegoEnPausa = false;
            GameManager.Instancia.ActualizarUI();
        }
        statsJugador = null;
    }
}
