using UnityEngine;
using System.Collections.Generic;

public enum Raridad { Comun, Raro, Epico, Legendario }

public class LevelUpManager : MonoBehaviour
{
    [Header("Base de datos de mejoras")]
    // Arrastra aquí en el Inspector todos tus MejoraConfig (Config_Daño, Config_Vida, etc.)
    public List<MejoraConfig> mejorasDisponibles;

    [Header("UI")]
    public GameObject panelLevelUp;
    public TMPro.TextMeshProUGUI textoOpcion1;
    public TMPro.TextMeshProUGUI textoOpcion2;
    public TMPro.TextMeshProUGUI textoOpcion3;

    // Estructura interna para recordar qué tiene cada botón
    private struct OpcionGenerada
    {
        public MejoraConfig config;
        public Raridad raridad;
        public float valorAplicar;
    }

    private OpcionGenerada[] opciones = new OpcionGenerada[3];
    private PlayerStats statsJugador;

    // ====== LLAMADO DESDE PlayerStats.SubirNivel() ======
    public void MostrarOpciones(PlayerStats stats)
    {
        statsJugador = stats;

        // Pausar juego
        Time.timeScale = 0f;
        if (GameManager.Instancia != null)
            GameManager.Instancia.juegoEnPausa = true;

        if (panelLevelUp != null)
            panelLevelUp.SetActive(true);

        // Generar 3 opciones distintas
        GenerarOpciones();
        ActualizarTextoUI();
    }

    // Genera las 3 opciones (tipo + rareza + valor)
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
            if (pool.Count == 0)
                break;

            int index = Random.Range(0, pool.Count);
            MejoraConfig config = pool[index];
            pool.RemoveAt(index); // Evita repetir la misma mejora en la misma subida

            Raridad raridad = GenerarRaridad(statsJugador);
            float valor = ObtenerValorPorRareza(config, raridad);

            opciones[i].config = config;
            opciones[i].raridad = raridad;
            opciones[i].valorAplicar = valor;
        }
    }

    void ActualizarTextoUI()
    {
        if (textoOpcion1 != null)
            textoOpcion1.text = DescribirOpcion(opciones[0]);
        if (textoOpcion2 != null)
            textoOpcion2.text = DescribirOpcion(opciones[1]);
        if (textoOpcion3 != null)
            textoOpcion3.text = DescribirOpcion(opciones[2]);
    }

    string DescribirOpcion(OpcionGenerada op)
    {
        if (op.config == null) return "Sin mejora";

        // Puedes mejorar este texto si quieres enseñar el valor exacto
        return $"[{op.raridad}] {op.config.nombre}";
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

    // ====== LÓGICA DE RAREZA (incluye suerte) ======

    Raridad GenerarRaridad(PlayerStats stats)
    {
        // Probabilidades base
        float pComun = 0.60f;
        float pRaro = 0.25f;
        float pEpico = 0.10f;
        float pLegendario = 0.05f;

        // Bonus por suerte (por ejemplo, suerte 50 = +5% repartido)
        float suerte = (stats != null) ? stats.suerte : 0f;
        float bonus = Mathf.Clamp01(suerte * 0.001f); // 0–0.1 como mucho

        float quitarDeComun = Mathf.Min(pComun * 0.5f, bonus);
        pComun -= quitarDeComun;
        pRaro  += quitarDeComun * 0.5f;
        pEpico += quitarDeComun * 0.3f;
        pLegendario += quitarDeComun * 0.2f;

        float suma = pComun + pRaro + pEpico + pLegendario;
        pComun      /= suma;
        pRaro       /= suma;
        pEpico      /= suma;
        pLegendario /= suma;

        float r = Random.value;

        if (r < pComun) return Raridad.Comun;
        r -= pComun;
        if (r < pRaro) return Raridad.Raro;
        r -= pRaro;
        if (r < pEpico) return Raridad.Epico;
        return Raridad.Legendario;
    }

    // ====== BOTONES (asigna estos en OnClick con índices 0,1,2) ======

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
                // Tratamos v como porcentaje: 0.10 = +10% del daño actual
                statsJugador.danoProyectil *= (1f + v);
                break;

            case TipoStat.Cadencia:
                // v es porcentaje de reducción del tiempo entre disparos (0.1 = -10%)
                statsJugador.cadenciaDisparo *= (1f - v);
                break;

            case TipoStat.VelocidadMovimiento:
                statsJugador.speed *= (1f + v);
                break;

            case TipoStat.Direcciones:
                statsJugador.cantidadDirecciones += Mathf.RoundToInt(v);
                break;

            // Amplía aquí con más tipos si los añades a TipoStat
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