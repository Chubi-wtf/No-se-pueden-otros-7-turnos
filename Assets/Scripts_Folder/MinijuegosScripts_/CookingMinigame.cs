using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class CookingMinigame : MonoBehaviour
{
    public static CookingMinigame Instance;

    [Header("Secuencias posibles (se elige una aleatoria)")]
    public List<Secuencia> secuenciasPosibles = new List<Secuencia>
    {
        new Secuencia { nombre = "Clasica", ingredientes = new List<string> { "Pan Base", "Carne",   "Lechuga", "Tomate",  "Pan Techo" } },
        new Secuencia { nombre = "BBQ",     ingredientes = new List<string> { "Pan Base", "Carne",   "Tomate",  "Carne",   "Pan Techo" } },
        new Secuencia { nombre = "Verde",   ingredientes = new List<string> { "Pan Base", "Lechuga", "Tomate",  "Lechuga", "Pan Techo" } },
    };

    [Header("Todos los ingredientes disponibles para rellenar el pool")]
    public List<string> todosLosIngredientes = new List<string>
        { "Pan Techo", "Pan Base", "Carne", "Lechuga", "Tomate" };

    [Header("Slots - donde suelta el jugador")]
    public List<IngredientSlot> slots;

    [Header("Ingredientes arrastrables")]
    public List<DraggableIngredient> ingredientes;

    [Header("Sprites por nombre de ingrediente")]
    public List<SpriteEntry> spritePorNombre;

    [Header("UI - Minijuego normal")]
    public TextMeshProUGUI textoOrden;
    public TextMeshProUGUI textoResultado;
    public TextMeshProUGUI textoTemporizador;

    // ── Evento de nervios ─────────────────────────────────────────────────────
    [Header("Evento Nervios — aparece 1 de cada 5 veces")]
    [Tooltip("Arrastra aquí el GameObject VentanaJuego — es el que tiembla")]
    public RectTransform ventanaJuego;

    [Tooltip("Panel superpuesto con instrucción y barra. Hijo de Canvas_MinijuegoCocina")]
    public GameObject panelNervios;

    [Tooltip("Texto de instrucción dentro de PanelNervios")]
    public TextMeshProUGUI textoNervios;

    [Tooltip("Slider que se llena al apretar Shift")]
    public Slider barraCalma;

    [Tooltip("Texto opcional sobre la barra, ej: 'Respira... 3/12'")]
    public TextMeshProUGUI textoBarraCalma;

    [Header("Ajustes del Evento Nervios")]
    public float tiempoLimiteNervios = 8f;   // segundos para completar el ritual
    public int pulsosShiftNecesarios = 12;  // pulsaciones de Shift necesarias
    public float duracionBuff = 10f; // segundos de inmunidad a cordura

    [Header("Debug")]
    [Tooltip("Actívalo desde DebugCommands (F5) para forzar el microjuego de nervios ignorando el random")]
    public bool forzarEventoNervios = false;

    [Header("Shake")]
    public float shakeMagnitud = 8f;          // intensidad del temblor

    // ── estado privado ────────────────────────────────────────────────────────
    private Transform panelIngredientes;
    private List<string> secuenciaActual = new List<string>();
    private int slotsBien = 0;
    private bool terminado = false;
    private float tiempoMaximo = 7f;
    private float tiempoRestante = 0f;

    private bool eventoNerviosActivo = false;
    private bool nerviosResueltos = false;
    private float tiempoRestanteNervios;
    private int pulsosCompletados = 0;

    private Vector3 posOriginalVentana;
    private float shakeTimer = 0f;

    // ─────────────────────────────────────────────────────────────────────────

    Sprite GetSprite(string nombre)
    {
        foreach (var e in spritePorNombre)
            if (e.nombre == nombre) return e.sprite;
        return null;
    }

    void Mezclar<T>(List<T> lista)
    {
        for (int i = lista.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T tmp = lista[i]; lista[i] = lista[j]; lista[j] = tmp;
        }
    }

    void Awake()
    {
        Instance = this;
        if (ingredientes != null && ingredientes.Count > 0)
            panelIngredientes = ingredientes[0].transform.parent;
    }

    void OnEnable()
    {
        eventoNerviosActivo = forzarEventoNervios || (Random.Range(0, 5) == 0);
        forzarEventoNervios = false;
        nerviosResueltos = false;
        pulsosCompletados = 0;
        tiempoRestanteNervios = tiempoLimiteNervios;
        shakeTimer = 0f;

        if (eventoNerviosActivo)
        {
            if (ventanaJuego != null)
                posOriginalVentana = ventanaJuego.localPosition;

            if (panelNervios != null) panelNervios.SetActive(true);

            if (barraCalma != null)
            {
                barraCalma.minValue = 0;
                barraCalma.maxValue = pulsosShiftNecesarios;
                barraCalma.value = 0;
            }

            ActualizarTextoNervios();
        }
        else
        {
            if (panelNervios != null) panelNervios.SetActive(false);
            StartCoroutine(InicializarConDelay());
        }
    }

    // ── Update principal ──────────────────────────────────────────────────────

    void Update()
    {
        if (eventoNerviosActivo && !nerviosResueltos)
        {
            UpdateNervios();
            return;
        }

        if (terminado || !gameObject.activeInHierarchy) return;

        tiempoRestante -= Time.unscaledDeltaTime;

        if (textoTemporizador != null)
            textoTemporizador.text = Mathf.Max(0f, tiempoRestante).ToString("F1") + "s";

        if (tiempoRestante <= 0f)
        {
            terminado = true;
            MostrarResultado("Tiempo agotado!", false);
        }
    }

    // ── Lógica del evento nervios ─────────────────────────────────────────────

    void UpdateNervios()
    {
        tiempoRestanteNervios -= Time.unscaledDeltaTime;

        // Shake — se suaviza según progreso de la barra
        shakeTimer += Time.unscaledDeltaTime;
        if (ventanaJuego != null)
        {
            float progreso = (float)pulsosCompletados / pulsosShiftNecesarios;
            float magnitudAct = Mathf.Lerp(shakeMagnitud, 0f, progreso);
            float xOff = Mathf.Sin(shakeTimer * 30f) * magnitudAct;
            float yOff = Mathf.Cos(shakeTimer * 24f) * magnitudAct * 0.6f;
            ventanaJuego.localPosition = posOriginalVentana + new Vector3(xOff, yOff, 0f);
        }

        // Tiempo agotado → sin buff
        if (tiempoRestanteNervios <= 0f)
        {
            TerminarEventoNervios(buffGanado: false);
            return;
        }

        // Detectar pulsación de Shift
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            pulsosCompletados = Mathf.Min(pulsosCompletados + 1, pulsosShiftNecesarios);

            if (barraCalma != null)
                barraCalma.value = pulsosCompletados;

            ActualizarTextoNervios();

            if (pulsosCompletados >= pulsosShiftNecesarios)
                TerminarEventoNervios(buffGanado: true);
        }
    }

    void ActualizarTextoNervios()
    {
        if (textoNervios != null)
            textoNervios.text = "¡La comanda te pone nervioso!\nAprieta <b>Shift</b> para respirar.";

        if (textoBarraCalma != null)
            textoBarraCalma.text = $"Respira... {pulsosCompletados}/{pulsosShiftNecesarios}";
    }

    void TerminarEventoNervios(bool buffGanado)
    {
        nerviosResueltos = true;

        if (ventanaJuego != null)
            ventanaJuego.localPosition = posOriginalVentana;

        if (buffGanado)
        {
            SanidadManager.Instance?.ActivarInmunidadCordura(duracionBuff);
            Debug.Log($"Buff de inmunidad concedido por {duracionBuff}s");
        }

        if (panelNervios != null) panelNervios.SetActive(false);

        StartCoroutine(InicializarConDelay());
    }

    // ── Inicialización del minijuego ──────────────────────────────────────────

    IEnumerator InicializarConDelay()
    {
        yield return null;
        Inicializar();
        yield return null;
        foreach (var ing in ingredientes) ing.GuardarPosicionOriginal();
        MezclarIngredientes();
    }

    void Inicializar()
    {
        slotsBien = 0;
        terminado = false;
        tiempoRestante = tiempoMaximo;

        if (textoResultado != null) textoResultado.gameObject.SetActive(false);

        int idx = Random.Range(0, secuenciasPosibles.Count);
        secuenciaActual = new List<string>(secuenciasPosibles[idx].ingredientes);

        if (textoOrden != null)
            textoOrden.text = string.Join("  >  ", secuenciaActual);

        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].ingredienteEsperado = (i < secuenciaActual.Count) ? secuenciaActual[i] : "";
            slots[i].Resetear();
        }

        List<string> pool = new List<string>(secuenciaActual);
        Mezclar(pool);

        for (int i = 0; i < ingredientes.Count; i++)
        {
            DraggableIngredient ing = ingredientes[i];

            if (panelIngredientes != null)
                ing.transform.SetParent(panelIngredientes, false);

            ing.ingredienteName = pool[i];

            Image img = ing.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = GetSprite(pool[i]);
                img.color = Color.white;
                img.enabled = true;
            }

            ing.gameObject.SetActive(true);
        }
    }

    void MezclarIngredientes()
    {
        if (panelIngredientes == null) return;
        List<Transform> hijos = new List<Transform>();
        foreach (Transform h in panelIngredientes) hijos.Add(h);
        Mezclar(hijos);
        for (int i = 0; i < hijos.Count; i++) hijos[i].SetSiblingIndex(i);
    }

    // ── Slots ─────────────────────────────────────────────────────────────────

    public void VerificarProgreso()
    {
        if (terminado) return;
        slotsBien++;

        if (slotsBien >= slots.Count)
        {
            terminado = true;
            MostrarResultado("Hamburguesa lista!", true);
        }
    }

    void MostrarResultado(string msg, bool exito)
    {
        if (textoResultado != null)
        {
            textoResultado.text = msg;
            textoResultado.color = exito ? Color.green : Color.red;
            textoResultado.gameObject.SetActive(true);
        }
        StartCoroutine(CerrarConRetraso(exito));
    }

    IEnumerator CerrarConRetraso(bool exito)
    {
        yield return new WaitForSecondsRealtime(1.5f);
        MinigameManager.Instance?.CerrarMinijuego(exito);
    }
}

[System.Serializable]
public class Secuencia
{
    public string nombre;
    public List<string> ingredientes;
}

[System.Serializable]
public class SpriteEntry
{
    public string nombre;
    public Sprite sprite;
}