using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum Raridad { Comun, Raro, Epico, Legendario }

public class LevelUpManager : MonoBehaviour
{
    [Header("Base de datos de mejoras")]
    public List<MejoraConfig> mejorasDisponibles;

    [Header("UI - Textos")]
    public GameObject panelLevelUp;
    public TMPro.TextMeshProUGUI textoOpcion1;
    public TMPro.TextMeshProUGUI textoOpcion2;
    public TMPro.TextMeshProUGUI textoOpcion3;

    [Header("UI - Botones (arrastra aquí los 3 botones del panel)")]
    public Button botonOpcion1;
    public Button botonOpcion2;
    public Button botonOpcion3;

    // Colores por rareza
    private static readonly Color COLOR_COMUN      = new Color(0.85f, 0.85f, 0.85f);
    private static readonly Color COLOR_RARO       = new Color(0.35f, 0.55f, 1.00f);
    private static readonly Color COLOR_EPICO      = new Color(0.75f, 0.25f, 1.00f);
    private static readonly Color COLOR_LEGENDARIO = new Color(1.00f, 0.65f, 0.00f);

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
        AudioManager.Instancia?.SonarLevelUp();
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

        ColorearBoton(botonOpcion1, opciones[0].raridad);
        ColorearBoton(botonOpcion2, opciones[1].raridad);
        ColorearBoton(botonOpcion3, opciones[2].raridad);
    }

    void ColorearBoton(Button boton, Raridad raridad)
    {
        if (boton == null) return;
        Image img = boton.GetComponent<Image>();
        if (img == null) return;

        switch (raridad)
        {
            case Raridad.Comun:      img.color = COLOR_COMUN;      break;
            case Raridad.Raro:       img.color = COLOR_RARO;       break;
            case Raridad.Epico:      img.color = COLOR_EPICO;      break;
            case Raridad.Legendario: img.color = COLOR_LEGENDARIO; break;
        }
    }

    string DescribirOpcion(OpcionGenerada op)
    {
        if (op.config == null) return "Sin mejora";
        string rarNombre;
        switch (op.raridad)
        {
            case Raridad.Raro:       rarNombre = "★ Raro";          break;
            case Raridad.Epico:      rarNombre = "★★ Epico";        break;
            case Raridad.Legendario: rarNombre = "★★★ Legendario";  break;
            default:                 rarNombre = "Comun";           break;
        }
        return $"[{rarNombre}] {op.config.nombre}\n{op.config.descripcion}";
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

        // Suerte ahora tiene impacto real: cada punto reduce la probabilidad de común
        // Fórmula curva: a suerte=50 → 33% de reducción de común; a suerte=100 → 50%
        float bonus = suerte / (suerte + 100f);
        float quitarDeComun = pComun * bonus;
        pComun -= quitarDeComun;
        pRaro  += quitarDeComun * 0.50f;
        pEpico += quitarDeComun * 0.30f;
        pLegendario += quitarDeComun * 0.20f;

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

            case TipoStat.Regeneracion:
                statsJugador.regeneracionVida += v;
                break;

            case TipoStat.VelocidadProyectil:
                statsJugador.velocidadProyectil *= (1f + v);
                break;

            case TipoStat.Rango:
                statsJugador.rangoDisparo += v;
                break;

            case TipoStat.VelocidadXP:
                statsJugador.multiplicadorExperiencia += v;
                break;

            case TipoStat.Suerte:
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
