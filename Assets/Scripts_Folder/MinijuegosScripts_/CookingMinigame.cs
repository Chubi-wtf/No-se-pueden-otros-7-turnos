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
    public Slider barraTemporizador;
    public TextMeshProUGUI textoInstrucciones;

    [Header("Evento Nervios — aparece 1 de cada 5 veces")]
    [Tooltip("El GameObject VentanaJuego completo — es el que tiembla")]
    public RectTransform ventanaJuego;
    [Tooltip("Panel superpuesto con instrucción, contador y barra")]
    public GameObject panelNervios;
    [Tooltip("Texto que dice INHALA / instrucción")]
    public TextMeshProUGUI textoNervios;
    [Tooltip("Contador numérico que baja de 3.0 a 0.0")]
    public TextMeshProUGUI textoContador;
    [Tooltip("Barra de respiraciones completadas")]
    public Slider barraCalma;
    [Tooltip("Texto encima de la barra: 'Respiración 2/4'")]
    public TextMeshProUGUI textoBarraCalma;

    [Header("Ajustes del Evento Nervios")]
    public float duracionCiclo = 3f;
    public float umbralInhala = 1.2f;
    public int respiracionesNecesarias = 4;
    public float tiempoLimiteNervios = 20f;
    public float tiempoExtraMinijuego = 3.5f;
    public float duracionBuff = 10f;

    [Header("Shake")]
    public float shakeMagnitud = 8f;

    [Header("Debug")]
    public bool forzarEventoNervios = false;

    private Transform panelIngredientes;
    private List<string> secuenciaActual = new List<string>();
    private int slotsBien = 0;
    private bool terminado = false;
    private bool minijuegoNormalListo = false;
    private float tiempoMaximoBase = 7f;
    private float tiempoRestante = 0f;

    private bool eventoNerviosActivo = false;
    private bool nerviosResueltos = false;
    private float tiempoRestanteNervios;
    private float cicloTimer;
    private int respiracionesOk = 0;
    private bool ventanaInhala = false;
    private bool shiftUsadoEnCiclo = false;

    private float shakeTimer = 0f;
    private Vector3 posOriginalVentana;


    Sprite GetSprite(string nombre)
    {
        if (string.IsNullOrEmpty(nombre)) return null;

        foreach (var e in spritePorNombre)
        {
            if (string.IsNullOrEmpty(e.nombre)) continue;

            if (e.nombre.Trim().Equals(nombre.Trim(), System.StringComparison.OrdinalIgnoreCase))
            {
                return e.sprite;
            }
        }

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
        minijuegoNormalListo = false;
        terminado = false;
        tiempoRestante = 0f;

        eventoNerviosActivo = forzarEventoNervios || (Random.Range(0, 5) == 0);
        forzarEventoNervios = false;
        nerviosResueltos = false;
        respiracionesOk = 0;
        cicloTimer = duracionCiclo;
        ventanaInhala = false;
        shiftUsadoEnCiclo = false;
        shakeTimer = 0f;
        tiempoRestanteNervios = tiempoLimiteNervios;

        if (eventoNerviosActivo)
        {
            if (ventanaJuego != null)
                posOriginalVentana = ventanaJuego.localPosition;

            if (panelNervios != null) panelNervios.SetActive(true);

            if (barraCalma != null)
            {
                barraCalma.minValue = 0;
                barraCalma.maxValue = respiracionesNecesarias;
                barraCalma.value = 0;
            }

            RefrescarUI();
        }
        else
        {
            if (panelNervios != null) panelNervios.SetActive(false);
            StartCoroutine(InicializarConDelay(false));
        }
    }

    void Update()
    {
        if (eventoNerviosActivo && !nerviosResueltos)
        {
            UpdateNervios();
            return;
        }

        if (!minijuegoNormalListo || terminado || !gameObject.activeInHierarchy) return;

        tiempoRestante -= Time.unscaledDeltaTime;

        if (barraTemporizador != null)
        {
            barraTemporizador.value = tiempoRestante;

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
            terminado = true;
            MostrarResultado("Tiempo agotado!", false);
        }
    }

    void UpdateNervios()
    {
        tiempoRestanteNervios -= Time.unscaledDeltaTime;
        cicloTimer -= Time.unscaledDeltaTime;

        shakeTimer += Time.unscaledDeltaTime;
        if (ventanaJuego != null)
        {
            float progreso = (float)respiracionesOk / respiracionesNecesarias;
            float magnitudAct = Mathf.Lerp(shakeMagnitud, 0f, progreso);
            float xOff = Mathf.Sin(shakeTimer * 30f) * magnitudAct;
            float yOff = Mathf.Cos(shakeTimer * 24f) * magnitudAct * 0.6f;
            ventanaJuego.localPosition = posOriginalVentana + new Vector3(xOff, yOff, 0f);
        }

        if (textoContador != null)
            textoContador.text = Mathf.Max(0f, cicloTimer).ToString("F1");

        bool eraVentana = ventanaInhala;
        ventanaInhala = (cicloTimer <= umbralInhala && cicloTimer > 0f);

        if (ventanaInhala && !eraVentana)
        {
            shiftUsadoEnCiclo = false;
            if (textoNervios != null)
                textoNervios.text = "<b><color=#FFFF00>INHALA</color></b>";
        }

        if (ventanaInhala && !shiftUsadoEnCiclo)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                shiftUsadoEnCiclo = true;
                respiracionesOk++;

                if (barraCalma != null) barraCalma.value = respiracionesOk;
                if (textoBarraCalma != null) textoBarraCalma.text = $"Respiración {respiracionesOk}/{respiracionesNecesarias}";
                if (textoNervios != null) textoNervios.text = "<color=#00FF88>✓ Bien</color>";

                if (respiracionesOk >= respiracionesNecesarias)
                {
                    TerminarEventoNervios(buffGanado: true);
                    return;
                }
            }
        }

        if (cicloTimer <= 0f)
        {
            cicloTimer = duracionCiclo;
            ventanaInhala = false;
            shiftUsadoEnCiclo = false;
            if (textoNervios != null)
                textoNervios.text = "¡Respira!\nEspera el <b>INHALA</b> y aprieta <b>Shift</b>";
        }

        if (tiempoRestanteNervios <= 0f)
            TerminarEventoNervios(buffGanado: false);
    }

    void RefrescarUI()
    {
        if (textoNervios != null) textoNervios.text = "¡La comanda te pone nervioso!\nEspera el <b>INHALA</b> y aprieta <b>Shift</b>";
        if (textoContador != null) textoContador.text = duracionCiclo.ToString("F1");
        if (textoBarraCalma != null) textoBarraCalma.text = $"Respiración 0/{respiracionesNecesarias}";
    }

    void TerminarEventoNervios(bool buffGanado)
    {
        nerviosResueltos = true;

        if (ventanaJuego != null)
            ventanaJuego.localPosition = posOriginalVentana;

        if (buffGanado)
        {
            SanidadManager.Instance?.ActivarInmunidadCordura(duracionBuff);
        }

        if (panelNervios != null) panelNervios.SetActive(false);

        StartCoroutine(InicializarConDelay(true));
    }


    IEnumerator InicializarConDelay(bool conTiempoExtra)
    {
        yield return null;
        Inicializar(conTiempoExtra);
        yield return null;
        foreach (var ing in ingredientes) ing.GuardarPosicionOriginal();
        MezclarIngredientes();
    }

    void Inicializar(bool conTiempoExtra)
    {
        slotsBien = 0;
        terminado = false;

        minijuegoNormalListo = true;

        float multiplicadorTiempo = MinigameManager.Instance != null
            ? MinigameManager.Instance.ObtenerMultiplicadorTiempoTurnoActual()
            : 1f;
        tiempoRestante = (tiempoMaximoBase * multiplicadorTiempo) + (conTiempoExtra ? tiempoExtraMinijuego : 0f);

        if (barraTemporizador != null)
        {
            barraTemporizador.maxValue = tiempoRestante;
            barraTemporizador.value = tiempoRestante;
        }

        if (textoResultado != null) textoResultado.gameObject.SetActive(false);

        if (textoInstrucciones != null)
        {
            textoInstrucciones.text = "Arrastra los ingredientes a los slots\nen el orden exacto de la secuencia.";
        }

        int idx = Random.Range(0, secuenciasPosibles.Count);
        secuenciaActual = new List<string>(secuenciasPosibles[idx].ingredientes);

        if (textoOrden != null)
            textoOrden.text = string.Join("  >  ", secuenciaActual);

        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].ingredienteEsperado = (i < secuenciaActual.Count) ? secuenciaActual[i] : "";
            slots[i].Resetear();
        }

        foreach (var ing in ingredientes)
        {
            if (ing == null) continue;
            ing.VolverAlPanel();
            ing.ResetearEstado();
        }

        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].ingredienteEsperado = i < secuenciaActual.Count ? secuenciaActual[i] : "";
            slots[i].Resetear();
        }

        List<string> pool = new List<string>(secuenciaActual);
        Mezclar(pool);

        for (int i = 0; i < ingredientes.Count; i++)
        {
            DraggableIngredient ing = ingredientes[i];

            ing.VolverAlPanel();

            ing.ingredienteName = pool[i];

            Image img = ing.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = GetSprite(pool[i]);
                img.color = Color.white;
                img.enabled = true;
            }

            ing.ResetearEstado();
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

        if (exito)
        {
            SonidoManager.Instance?.Acierto();
        }
        else
        {
            SonidoManager.Instance?.Fallo();
        }

        StartCoroutine(CerrarConRetraso(exito));
    }

    IEnumerator CerrarConRetraso(bool exito)
    {
        yield return new WaitForSecondsRealtime(1.5f);

        if (exito)
        {
            EntregaBandeja entrega = EntregaBandeja.ObtenerInstancia();
            bool entregaIniciada = entrega != null &&
                                   entrega.IniciarEntrega("hamburguesa");

            if (entregaIniciada)
                MinigameManager.Instance?.CerrarMinijuegoPausa();
            else
                MinigameManager.Instance?.CerrarMinijuego(true);
        }
        else
        {
            MinigameManager.Instance?.CerrarMinijuego(false);
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
}
