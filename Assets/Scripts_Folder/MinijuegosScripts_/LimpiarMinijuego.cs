using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LimpiarMinijuego : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image manchaImagen;
    public TextMeshProUGUI textoTemporizador;

    [Header("Evento Buff — aparece 1 de cada 5 veces")]
    public GameObject panelBuff;          // panel que dice "¡SPAMEA ESPACIO! Buff x2"
    public TextMeshProUGUI textoBuff;     // texto dentro del panel

    [Header("Ajustes del Minijuego")]
    public float suciedadTotal = 100f;
    public float multiplicadorFrotar = 150f;
    public float tiempoMaximo = 5f;

    [Header("Ajustes del Buff")]
    public float duracionBuff = 3f;
    public float multiplicadorBuff = 2f;   // cuánto aumenta la velocidad
    public int spacePresionesParaBuff = 8;  // cuántas veces hay que spamear

    // ── estado ────────────────────────────────────────────────────────────────
    private float suciedadActual;
    private float tiempoRestante;
    private bool minijuegoActivo = false;

    private bool eventoBuffActivo = false;
    private int spacesPresionados = 0;
    private bool buffConcedido = false;

    void OnEnable()
    {
        suciedadActual = suciedadTotal;
        tiempoRestante = tiempoMaximo;
        minijuegoActivo = true;
        buffConcedido = false;
        spacesPresionados = 0;

        // 1 de cada 5 veces aparece el evento de buff
        eventoBuffActivo = (Random.Range(0, 5) == 0);

        if (manchaImagen != null)
        {
            Color c = manchaImagen.color;
            c.a = 1f;
            manchaImagen.color = c;
        }

        if (panelBuff != null)
            panelBuff.SetActive(eventoBuffActivo);

        if (textoBuff != null && eventoBuffActivo)
            textoBuff.text = $"¡SPAMEA ESPACIO x{spacePresionesParaBuff}!\nBuff de velocidad x{multiplicadorBuff}";
    }

    void Update()
    {
        if (!minijuegoActivo) return;

        tiempoRestante -= Time.unscaledDeltaTime;
        textoTemporizador.text = tiempoRestante.ToString("F1") + "s";

        if (tiempoRestante <= 0f)
        {
            PerderMinijuego();
            return;
        }

        // ── Evento buff: spamear Space ────────────────────────────────────────
        if (eventoBuffActivo && !buffConcedido)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                spacesPresionados++;

                if (textoBuff != null)
                    textoBuff.text = $"¡SPAMEA ESPACIO!\n{spacesPresionados}/{spacePresionesParaBuff}";

                if (spacesPresionados >= spacePresionesParaBuff)
                {
                    buffConcedido = true;
                    CubeMovement player = FindFirstObjectByType<CubeMovement>();
                    player?.AplicarBuff(multiplicadorBuff, duracionBuff);

                    if (panelBuff != null) panelBuff.SetActive(false);
                    Debug.Log("Buff de velocidad concedido!");
                }
            }
        }

        // ── Limpiar con mouse ─────────────────────────────────────────────────
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
    }

    void GanarMinijuego()
    {
        minijuegoActivo = false;
        if (panelBuff != null) panelBuff.SetActive(false);
        Debug.Log("Mesa limpia a tiempo!");
        MinigameManager.Instance?.CerrarMinijuego(true);
    }

    void PerderMinijuego()
    {
        minijuegoActivo = false;
        if (panelBuff != null) panelBuff.SetActive(false);
        Debug.Log("Tiempo agotado!");
        MinigameManager.Instance?.CerrarMinijuego(false);
    }
}