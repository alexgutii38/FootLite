using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Menú de opciones que construye TODA su interfaz por código: Canvas, panel,
/// sliders de volumen y botones. No hay que montar nada en el editor de Unity.
///
/// SETUP en Unity:
///  1. Crea un GameObject vacío (p. ej. "MenuOpciones") en la escena donde lo
///     quieras —EscenaTitulo y/o EscenaCampo— y añádele este componente.
///  2. Para abrirlo desde un botón ya existente (el botón "Opciones" del título
///     o del menú de pausa): en el evento OnClick de ese botón arrastra este
///     GameObject y elige la función MenuOpciones.Abrir().
///  El botón "Cerrar" del propio panel (y el clic fuera del panel) se conectan
///  solos.
///
/// Notas:
///  - El panel se crea oculto. Funciona con el juego pausado (timeScale 0).
///  - Los volúmenes se enrutan por AudioManager si existe; si no, van directos
///    a PlayerPrefs, así que también funciona en una escena suelta de prueba.
///  - Para cerrarlo se usa el botón "Cerrar" o un clic fuera del panel (no
///    captura la tecla ESC para no interferir con el menú de pausa).
/// </summary>
public class MenuOpciones : MonoBehaviour
{
    [Header("Estilo (colores del menú)")]
    public Color colorFondo  = new Color(0f, 0f, 0f, 0.75f);
    public Color colorPanel  = new Color(0.12f, 0.14f, 0.20f, 1f);
    public Color colorBoton  = new Color(0.20f, 0.45f, 0.85f, 1f);
    public Color colorCerrar = new Color(0.75f, 0.25f, 0.25f, 1f);
    public Color colorTexto  = Color.white;

    private GameObject      panelRaiz;            // fondo a pantalla completa; se activa/desactiva
    private Slider          sliderMusica;
    private Slider          sliderEfectos;
    private TextMeshProUGUI textoBotonPantalla;
    private bool            pantallaCompleta;

    void Awake()
    {
        AsegurarEventSystem();
        ConstruirUI();
        CargarValores();
        panelRaiz.SetActive(false);   // empieza oculto
    }

    // ─────────────────────────────────────────────────────────────
    //  API pública (conéctala al OnClick de tus botones)
    // ─────────────────────────────────────────────────────────────

    /// <summary>Muestra el menú de opciones y refresca los valores actuales.</summary>
    public void Abrir()
    {
        CargarValores();
        if (panelRaiz != null) panelRaiz.SetActive(true);
    }

    /// <summary>Oculta el menú de opciones.</summary>
    public void Cerrar()
    {
        if (panelRaiz != null) panelRaiz.SetActive(false);
    }

    /// <summary>Alterna la visibilidad del menú de opciones.</summary>
    public void Alternar()
    {
        if (panelRaiz == null) return;
        if (panelRaiz.activeSelf) Cerrar(); else Abrir();
    }

    // ─────────────────────────────────────────────────────────────
    //  Carga y guardado de valores
    // ─────────────────────────────────────────────────────────────

    void CargarValores()
    {
        float volMusica = AudioManager.Instancia != null
            ? AudioManager.Instancia.ObtenerVolumenMusica()
            : PlayerPrefs.GetFloat("VolumenMusica", 1f);

        float volEfectos = AudioManager.Instancia != null
            ? AudioManager.Instancia.ObtenerVolumenEfectos()
            : PlayerPrefs.GetFloat("VolumenEfectos", 1f);

        pantallaCompleta = PlayerPrefs.GetInt("PantallaCompleta", Screen.fullScreen ? 1 : 0) == 1;

        if (sliderMusica  != null) sliderMusica.SetValueWithoutNotify(volMusica);
        if (sliderEfectos != null) sliderEfectos.SetValueWithoutNotify(volEfectos);
        ActualizarTextoBotonPantalla();
    }

    void OnMusicaCambiada(float valor)
    {
        if (AudioManager.Instancia != null) AudioManager.Instancia.SetVolumenMusica(valor);
        else PlayerPrefs.SetFloat("VolumenMusica", Mathf.Clamp01(valor));
    }

    void OnEfectosCambiados(float valor)
    {
        if (AudioManager.Instancia != null) AudioManager.Instancia.SetVolumenEfectos(valor);
        else PlayerPrefs.SetFloat("VolumenEfectos", Mathf.Clamp01(valor));
    }

    void OnPantallaCompletaPulsada()
    {
        pantallaCompleta = !pantallaCompleta;
        Screen.fullScreen = pantallaCompleta;
        PlayerPrefs.SetInt("PantallaCompleta", pantallaCompleta ? 1 : 0);
        ActualizarTextoBotonPantalla();
    }

    void ActualizarTextoBotonPantalla()
    {
        if (textoBotonPantalla != null)
            textoBotonPantalla.text = pantallaCompleta
                ? "Pantalla completa: SÍ"
                : "Pantalla completa: NO";
    }

    // ─────────────────────────────────────────────────────────────
    //  Construcción de la interfaz
    // ─────────────────────────────────────────────────────────────

    void ConstruirUI()
    {
        // ── Canvas dedicado (por encima del HUD y del menú de pausa) ──
        GameObject canvasGO = new GameObject("CanvasOpciones");
        canvasGO.transform.SetParent(transform, false);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode     = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder   = 500;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight  = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // ── Fondo oscuro a pantalla completa (bloquea clics y cierra al pulsar) ──
        panelRaiz = CrearUI("FondoOpciones", canvasGO.transform);
        Image fondoImg = panelRaiz.AddComponent<Image>();
        fondoImg.color = colorFondo;
        Estirar(panelRaiz.GetComponent<RectTransform>());

        Button btnFondo = panelRaiz.AddComponent<Button>();
        btnFondo.transition  = Selectable.Transition.None; // clic fuera = cerrar, sin tintar la pantalla
        btnFondo.targetGraphic = fondoImg;
        btnFondo.onClick.AddListener(Cerrar);

        // ── Panel central ────────────────────────────────────────────
        GameObject panel = CrearUI("Panel", panelRaiz.transform);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = colorPanel;

        // El panel tiene su propio Button "vacío" para que los clics SOBRE el
        // panel no se propaguen al fondo y lo cierren.
        Button btnPanel = panel.AddComponent<Button>();
        btnPanel.transition    = Selectable.Transition.None;
        btnPanel.targetGraphic = panelImg;

        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.pivot     = new Vector2(0.5f, 0.5f);
        panelRT.sizeDelta = new Vector2(620f, 580f);
        panelRT.anchoredPosition = Vector2.zero;

        // ── Título ───────────────────────────────────────────────────
        CrearTexto(panel.transform, "OPCIONES", 46f, FontStyles.Bold,
            new Vector2(0f, -45f), new Vector2(560f, 60f));

        // ── Volumen de música ────────────────────────────────────────
        CrearTexto(panel.transform, "Volumen de música", 28f, FontStyles.Normal,
            new Vector2(0f, -135f), new Vector2(560f, 40f));
        sliderMusica = CrearSlider(panel.transform,
            new Vector2(0f, -180f), new Vector2(520f, 28f));
        sliderMusica.onValueChanged.AddListener(OnMusicaCambiada);

        // ── Volumen de efectos ───────────────────────────────────────
        CrearTexto(panel.transform, "Volumen de efectos", 28f, FontStyles.Normal,
            new Vector2(0f, -245f), new Vector2(560f, 40f));
        sliderEfectos = CrearSlider(panel.transform,
            new Vector2(0f, -290f), new Vector2(520f, 28f));
        sliderEfectos.onValueChanged.AddListener(OnEfectosCambiados);

        // ── Botón pantalla completa ──────────────────────────────────
        Button btnPantalla = CrearBoton(panel.transform, "Pantalla completa: NO", colorBoton,
            new Vector2(0f, -370f), new Vector2(520f, 58f), out textoBotonPantalla);
        btnPantalla.onClick.AddListener(OnPantallaCompletaPulsada);

        // ── Botón cerrar ─────────────────────────────────────────────
        Button btnCerrar = CrearBoton(panel.transform, "Cerrar", colorCerrar,
            new Vector2(0f, -455f), new Vector2(520f, 58f), out _);
        btnCerrar.onClick.AddListener(Cerrar);
    }

    // ─────────────────────────────────────────────────────────────
    //  Helpers de construcción
    // ─────────────────────────────────────────────────────────────

    /// <summary>Crea un GameObject con RectTransform bajo un padre.</summary>
    GameObject CrearUI(string nombre, Transform padre)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        return go;
    }

    /// <summary>Estira un RectTransform para ocupar todo su padre.</summary>
    void Estirar(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    /// <summary>Ancla un RectTransform al borde superior-centro del padre.</summary>
    void AnclarArriba(RectTransform rt, Vector2 posicion, Vector2 tamano)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot     = new Vector2(0.5f, 1f);
        rt.anchoredPosition = posicion;
        rt.sizeDelta = tamano;
    }

    TextMeshProUGUI CrearTexto(Transform padre, string texto, float tamano,
        FontStyles estilo, Vector2 posicion, Vector2 tamanoCaja)
    {
        GameObject go = CrearUI("Texto", padre);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = texto;
        tmp.fontSize  = tamano;
        tmp.fontStyle = estilo;
        tmp.color     = colorTexto;
        tmp.alignment = TextAlignmentOptions.Center;
        AnclarArriba(go.GetComponent<RectTransform>(), posicion, tamanoCaja);
        return tmp;
    }

    Button CrearBoton(Transform padre, string texto, Color color,
        Vector2 posicion, Vector2 tamano, out TextMeshProUGUI etiqueta)
    {
        GameObject go = CrearUI("Boton", padre);
        Image img = go.AddComponent<Image>();
        img.color = color;

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        AnclarArriba(go.GetComponent<RectTransform>(), posicion, tamano);

        GameObject txtGO = CrearUI("Texto", go.transform);
        TextMeshProUGUI tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = texto;
        tmp.fontSize  = 28f;
        tmp.color     = colorTexto;
        tmp.alignment = TextAlignmentOptions.Center;
        Estirar(txtGO.GetComponent<RectTransform>());

        etiqueta = tmp;
        return btn;
    }

    /// <summary>
    /// Crea un Slider funcional reutilizando DefaultControls (la misma API que
    /// usa el menú GameObject > UI > Slider del editor) y lo recolorea.
    /// </summary>
    Slider CrearSlider(Transform padre, Vector2 posicion, Vector2 tamano)
    {
        GameObject go = DefaultControls.CreateSlider(new DefaultControls.Resources());
        go.name = "Slider";
        go.transform.SetParent(padre, false);

        AnclarArriba(go.GetComponent<RectTransform>(), posicion, tamano);

        Slider slider = go.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value    = 1f;

        // Recoloreado para que combine con el panel
        Transform fondo = go.transform.Find("Background");
        if (fondo != null)
        {
            Image bgImg = fondo.GetComponent<Image>();
            if (bgImg != null) bgImg.color = new Color(0.05f, 0.05f, 0.08f, 1f);
        }
        if (slider.fillRect != null)
        {
            Image fillImg = slider.fillRect.GetComponent<Image>();
            if (fillImg != null) fillImg.color = colorBoton;
        }
        if (slider.handleRect != null)
        {
            Image handleImg = slider.handleRect.GetComponent<Image>();
            if (handleImg != null) handleImg.color = Color.white;
        }

        return slider;
    }

    /// <summary>Crea un EventSystem si la escena no tiene ninguno (necesario para la UI).</summary>
    void AsegurarEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null) return;
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }
}
