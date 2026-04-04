using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LimpiarMinijuego : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image manchaImagen;
    public Slider barraTemporizador;

    [Header("Evento Buff — aparece 1 de cada 5 veces")]
    public GameObject panelBuff;
    public TextMeshProUGUI textoBuff;
    public Slider barraSpam;

    [Header("Ajustes del Minijuego")]
    public float suciedadTotal = 100f;
    public float multiplicadorFrotar = 150f;
    public float tiempoMaximo = 5f;

    [Tooltip("Segundos extra que se añaden al minijuego cuando hay evento buff")]
    public float tiempoExtraConEvento = 3.5f;

    [Header("Ajustes del Buff")]
    public float duracionBuff = 3f;
    public float multiplicadorBuff = 2f;
    public int spacePresionesParaBuff = 8;

    [Header("Debug")]
    public bool forzarEventoBuff = false;

    private float suciedadActual;
    private float tiempoRestante;
    private bool minijuegoActivo = false;
    private bool eventoBuffActivo = false;
    private int spacesPresionados = 0;
    private bool buffConcedido = false;

    private float parpadeoTimer = 0f;
    private bool parpadeoVisible = true;

    void OnEnable()
    {
        suciedadActual = suciedadTotal;
        minijuegoActivo = true;
        buffConcedido = false;
        spacesPresionados = 0;
        parpadeoTimer = 0f;
        parpadeoVisible = true;

        eventoBuffActivo = forzarEventoBuff || (Random.Range(0, 5) == 0);
        forzarEventoBuff = false;

        float multiplicadorTiempo = MinigameManager.Instance != null
            ? MinigameManager.Instance.ObtenerMultiplicadorTiempoTurnoActual()
            : 1f;
        tiempoRestante = (tiempoMaximo * multiplicadorTiempo) + (eventoBuffActivo ? tiempoExtraConEvento : 0f);

        if (barraTemporizador != null)
        {
            barraTemporizador.maxValue = tiempoRestante;
            barraTemporizador.value = tiempoRestante;
        }

        if (manchaImagen != null)
        {
            Color c = manchaImagen.color;
            c.a = 1f;
            manchaImagen.color = c;
        }

        if (panelBuff != null)
            panelBuff.SetActive(eventoBuffActivo);

        if (barraSpam != null)
        {
            barraSpam.minValue = 0;
            barraSpam.maxValue = spacePresionesParaBuff;
            barraSpam.value = 0;
            barraSpam.gameObject.SetActive(eventoBuffActivo);
        }

        ActualizarTextoBuff();
    }

    void Update()
    {
        if (!minijuegoActivo) return;

        tiempoRestante -= Time.unscaledDeltaTime;

        if (barraTemporizador != null)
        {
            barraTemporizador.value = tiempoRestante;

            // NUEVO: Cambiar color de verde a rojo según el tiempo restante
            if (barraTemporizador.fillRect != null)
            {
                Image fillImage = barraTemporizador.fillRect.GetComponent<Image>();
                if (fillImage != null && barraTemporizador.maxValue > 0)
                {
                    float porcentaje = barraTemporizador.value / barraTemporizador.maxValue;
                    fillImage.color = Color.Lerp(Color.red, Color.green, porcentaje);
                }
            }
        }

        if (tiempoRestante <= 0f)
        {
            PerderMinijuego();
            return;
        }

        if (eventoBuffActivo && !buffConcedido)
        {
            parpadeoTimer += Time.unscaledDeltaTime;
            if (parpadeoTimer >= 0.4f)
            {
                parpadeoTimer = 0f;
                parpadeoVisible = !parpadeoVisible;
                if (textoBuff != null)
                    textoBuff.color = parpadeoVisible ? Color.white : Color.yellow;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                spacesPresionados = Mathf.Min(spacesPresionados + 1, spacePresionesParaBuff);
                SonidoManager.Instance?.Acierto();

                if (barraSpam != null)
                    barraSpam.value = spacesPresionados;

                ActualizarTextoBuff();

                if (spacesPresionados >= spacePresionesParaBuff)
                {
                    buffConcedido = true;
                    CubeMovement player = FindFirstObjectByType<CubeMovement>();
                    player?.AplicarBuff(multiplicadorBuff, duracionBuff);

                    if (panelBuff != null) panelBuff.SetActive(false);
                    if (barraSpam != null) barraSpam.gameObject.SetActive(false);
                }
            }
        }

        if (Input.GetMouseButton(0))
        {
            float movY = Mathf.Abs(Input.GetAxis("Mouse Y"));
            suciedadActual -= movY * multiplicadorFrotar * Time.unscaledDeltaTime;

            Color c = manchaImagen.color;
            c.a = Mathf.Max(0f, suciedadActual / suciedadTotal);
            manchaImagen.color = c;

            if (suciedadActual <= 0f)
                GanarMinijuego();
        }
        if (PauseMenu.isPaused) return;
    }

    void ActualizarTextoBuff()
    {
        if (textoBuff == null) return;
        if (spacesPresionados == 0)
            textoBuff.text = $"¡SPAMEA <b>ESPACIO</b> x{spacePresionesParaBuff}!\nBuff velocidad x{multiplicadorBuff}";
        else
            textoBuff.text = $"¡SIGUE!\n{spacesPresionados}/{spacePresionesParaBuff}";
    }

    void GanarMinijuego()
    {
        minijuegoActivo = false;
        SonidoManager.Instance?.Acierto();
        if (panelBuff != null) panelBuff.SetActive(false);
        if (barraSpam != null) barraSpam.gameObject.SetActive(false);
        MinigameManager.Instance?.CerrarMinijuego(true);
    }

    void PerderMinijuego()
    {
        minijuegoActivo = false;
        SonidoManager.Instance?.Fallo();
        if (panelBuff != null) panelBuff.SetActive(false);
        if (barraSpam != null) barraSpam.gameObject.SetActive(false);
        MinigameManager.Instance?.CerrarMinijuego(false);
    }
}
