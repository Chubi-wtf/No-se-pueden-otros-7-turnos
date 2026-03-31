using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MinijuegoServirPEDIDO : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panelMinijuego;

    [Header("Referencias UI")]
    public Image imagenBandeja;
    public Slider sliderDescenso;
    public Slider sliderTiming;
    public Slider sliderBalance;
    public TextMeshProUGUI textoInstrucciones;
    public TextMeshProUGUI textoEstado;

    [Header("Textos")]
    [TextArea(2, 5)]
    public string plantillaInstrucciones =
        "Deja el {pedido} en <b>{mesa}</b>.\n" +
        "Usa <b>Q</b> y <b>E</b> para equilibrar la bandeja.\n" +
        "Espera a que el medidor suba hasta la zona segura y pulsa <b>S</b> para bajar.";

    [Header("Movimiento de la bandeja")]
    public float desplazamientoVertical = 220f;
    public float rotacionMaxima = 12f;

    [Header("Balance")]
    public float gravedadBalance = 0.9f;
    public float correccionBalance = 2f;
    public float umbralCaida = 1f;

    [Header("Timing de bajada")]
    public int aciertosNecesarios = 5;
    public float velocidadTiming = 0.45f;
    [Range(0.05f, 0.45f)] public float tamanoZonaPerfecta = 0.18f;
    public float castigoPorError = 0.2f;
    [Range(0.55f, 0.95f)] public float puntoDeSoltar = 0.8f;

    [Header("Resultado")]
    public float retrasoCierre = 1.1f;

    private string nombrePedido = "pedido";
    private string nombreMesa = "Mesa";
    private RectTransform rectBandeja;
    private Vector2 posicionInicial;
    private float balanceActual = 0f;
    private float progresoDescenso = 0f;
    private float progresoTiming = 0f;
    private bool terminado = false;

    void Awake()
    {
        if (imagenBandeja != null)
            rectBandeja = imagenBandeja.rectTransform;
    }

    void OnEnable()
    {
        MostrarPanel(true);
        ReiniciarEstado();
    }

    void OnDisable()
    {
        MostrarPanel(false);
    }

    void Update()
    {
        if (terminado) return;

        ActualizarBalance();
        ActualizarTiming();
        ActualizarInputs();
        RefrescarVisuales();
    }

    public void PrepararEntrega(string pedido, string mesa)
    {
        nombrePedido = string.IsNullOrEmpty(pedido) ? "pedido" : pedido;
        nombreMesa = string.IsNullOrEmpty(mesa) ? "Mesa" : mesa;
    }

    void ReiniciarEstado()
    {
        terminado = false;
        balanceActual = Random.Range(-0.1f, 0.1f);
        progresoDescenso = 0f;
        progresoTiming = 0f;

        if (rectBandeja != null)
        {
            posicionInicial = rectBandeja.anchoredPosition;
            rectBandeja.anchoredPosition = posicionInicial;
            rectBandeja.localRotation = Quaternion.identity;
        }

        if (sliderDescenso != null)
        {
            sliderDescenso.minValue = 0f;
            sliderDescenso.maxValue = 1f;
            sliderDescenso.value = 0f;
        }

        if (sliderTiming != null)
        {
            sliderTiming.minValue = 0f;
            sliderTiming.maxValue = 1f;
            sliderTiming.value = progresoTiming;
        }

        if (sliderBalance != null)
        {
            sliderBalance.minValue = -1f;
            sliderBalance.maxValue = 1f;
            sliderBalance.value = balanceActual;
        }

        if (textoInstrucciones != null)
        {
            textoInstrucciones.text = plantillaInstrucciones
                .Replace("{pedido}", nombrePedido)
                .Replace("{mesa}", nombreMesa);
        }

        if (textoEstado != null)
            textoEstado.text = $"Bajando bandeja... 0/{aciertosNecesarios}";

        RefrescarVisuales();
    }

    void ActualizarBalance()
    {
        float dir = Mathf.Sign(balanceActual);
        if (Mathf.Approximately(dir, 0f))
            dir = Random.Range(0, 2) == 0 ? -1f : 1f;

        balanceActual += dir * (gravedadBalance + Mathf.Abs(balanceActual) * gravedadBalance) * Time.unscaledDeltaTime;

        if (Input.GetKey(KeyCode.Q))
            balanceActual -= correccionBalance * Time.unscaledDeltaTime;
        else if (Input.GetKey(KeyCode.E))
            balanceActual += correccionBalance * Time.unscaledDeltaTime;

        balanceActual = Mathf.Clamp(balanceActual, -1f, 1f);

        if (Mathf.Abs(balanceActual) >= umbralCaida)
            Fallar("La bandeja se inclino demasiado.");
    }

    void ActualizarTiming()
    {
        progresoTiming += velocidadTiming * Time.unscaledDeltaTime;

        if (progresoTiming >= 1f)
        {
            progresoTiming = 0f;
            progresoDescenso = Mathf.Max(0f, progresoDescenso - castigoPorError);

            if (textoEstado != null)
                textoEstado.text = "Te tardaste. Vuelve a medir la bajada.";

            SonidoManager.Instance?.Fallo();
        }
    }

    void ActualizarInputs()
    {
        if (!Input.GetKeyDown(KeyCode.S)) return;

        if (EstaEnZonaSegura())
        {
            RegistrarAcierto();
        }
        else
        {
            progresoDescenso = Mathf.Max(0f, progresoDescenso - castigoPorError);

            if (textoEstado != null)
                textoEstado.text = "Muy brusco. Vuelve a intentarlo.";

            SonidoManager.Instance?.Fallo();
        }
    }

    bool EstaEnZonaSegura()
    {
        float distanciaAlCentro = Mathf.Abs(progresoTiming - puntoDeSoltar);
        return distanciaAlCentro <= tamanoZonaPerfecta * 0.5f;
    }

    void RegistrarAcierto()
    {
        progresoDescenso += 1f / Mathf.Max(1, aciertosNecesarios);
        progresoDescenso = Mathf.Clamp01(progresoDescenso);

        if (textoEstado != null)
        {
            int aciertosActuales = Mathf.RoundToInt(progresoDescenso * aciertosNecesarios);
            textoEstado.text = $"Bajando bandeja... {Mathf.Min(aciertosNecesarios, aciertosActuales)}/{aciertosNecesarios}";
        }

        SonidoManager.Instance?.Acierto();
        progresoTiming = 0f;

        if (progresoDescenso >= 1f)
            Ganar();
    }

    void RefrescarVisuales()
    {
        if (sliderTiming != null)
            sliderTiming.value = progresoTiming;

        if (sliderBalance != null)
            sliderBalance.value = balanceActual;

        if (sliderDescenso != null)
            sliderDescenso.value = progresoDescenso;

        if (rectBandeja != null)
        {
            rectBandeja.anchoredPosition = posicionInicial + Vector2.down * (desplazamientoVertical * progresoDescenso);
            rectBandeja.localRotation = Quaternion.Euler(0f, 0f, -balanceActual * rotacionMaxima);
        }

        ActualizarColorTiming();
    }

    void ActualizarColorTiming()
    {
        if (sliderTiming?.fillRect == null) return;

        Image fill = sliderTiming.fillRect.GetComponent<Image>();
        if (fill == null) return;

        fill.color = EstaEnZonaSegura()
            ? new Color(0.2f, 0.95f, 0.45f)
            : new Color(1f, 0.8f, 0.15f);
    }

    void Ganar()
    {
        if (terminado) return;
        terminado = true;

        if (textoEstado != null)
            textoEstado.text = "Bandeja apoyada con cuidado.";

        StartCoroutine(CerrarConResultado(true));
    }

    void Fallar(string mensaje)
    {
        if (terminado) return;
        terminado = true;

        if (textoEstado != null)
            textoEstado.text = mensaje;

        SonidoManager.Instance?.Fallo();
        StartCoroutine(CerrarConResultado(false));
    }

    System.Collections.IEnumerator CerrarConResultado(bool exito)
    {
        yield return new WaitForSecondsRealtime(retrasoCierre);
        MostrarPanel(false);

        EntregaBandeja entrega = EntregaBandeja.ObtenerInstancia();
        if (entrega != null && entrega.HayEntregaActiva)
        {
            if (exito)
                entrega.CompletarEntregaDesdeMinijuego();
            else
                entrega.FallarEntregaDesdeMinijuego();
        }
        else
        {
            MinigameManager.Instance?.CerrarMinijuego(exito);
        }
    }

    void MostrarPanel(bool visible)
    {
        if (panelMinijuego != null)
            panelMinijuego.SetActive(visible);
    }
}
