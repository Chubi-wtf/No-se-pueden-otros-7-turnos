using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LimpiarMinijuego : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image manchaImagen;
    public TextMeshProUGUI textoTemporizador;

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

        tiempoRestante = tiempoMaximo + (eventoBuffActivo ? tiempoExtraConEvento : 0f);

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
        textoTemporizador.text = Mathf.Max(0f, tiempoRestante).ToString("F1") + "s";

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
                    Debug.Log("Buff de velocidad concedido!");
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
        if (panelBuff != null) panelBuff.SetActive(false);
        if (barraSpam != null) barraSpam.gameObject.SetActive(false);
        Debug.Log("Mesa limpia a tiempo!");
        MinigameManager.Instance?.CerrarMinijuego(true);
    }

    void PerderMinijuego()
    {
        minijuegoActivo = false;
        if (panelBuff != null) panelBuff.SetActive(false);
        if (barraSpam != null) barraSpam.gameObject.SetActive(false);
        Debug.Log("Tiempo agotado!");
        MinigameManager.Instance?.CerrarMinijuego(false);
    }
}