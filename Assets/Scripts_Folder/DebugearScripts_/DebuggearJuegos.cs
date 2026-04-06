using UnityEngine;
using UnityEngine.SceneManagement;

public class DebuggearJuegos : MonoBehaviour
{
    public static DebuggearJuegos Instance;

    [Header("Activacion")]
    public bool debugActivo = true;
    public KeyCode teclaToggleDebug = KeyCode.F9;

    [Header("Canvas / Panel debug opcional")]
    public GameObject panelDebug;

    [Header("Minijuegos")]
    public GameObject panelLimpiar;
    public GameObject panelCocina;
    public GameObject panelCafe;
    public GameObject panelServirPedido;

    [Header("Scripts opcionales para debug")]
    public LimpiarMinijuego limpiarMinijuego;
    public CookingMinigame cookingMinigame;
    public EntregaBandeja entregaBandeja;

    [Header("Teclas de acceso rapido")]
    public KeyCode teclaLimpiar = KeyCode.F1;
    public KeyCode teclaCocina = KeyCode.F2;
    public KeyCode teclaCafe = KeyCode.F3;
    public KeyCode teclaServirPedido = KeyCode.F4;
    public KeyCode teclaActivarZonas = KeyCode.F5;
    public KeyCode teclaForzarEntrega = KeyCode.F6;
    public KeyCode teclaGameOver = KeyCode.F7;
    public KeyCode teclaPanelSiguienteTurno = KeyCode.F10;
    public KeyCode teclaQuedarAUnoDelTurno = KeyCode.F11;

    [Header("Eventos forzados")]
    public bool forzarBuffLimpieza = false;
    public bool forzarNerviosCocina = false;

    [Header("Reinicio")]
    public float tiempoParaReiniciar = 3f;
    public KeyCode teclaReinicio = KeyCode.R;
    private float timerReinicio = 0f;

    void Awake()
    {
        Instance = this;
    }

    public static DebuggearJuegos ObtenerInstancia()
    {
        if (Instance != null)
            return Instance;

        DebuggearJuegos[] debugs = FindObjectsByType<DebuggearJuegos>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        if (debugs.Length > 0)
            Instance = debugs[0];

        return Instance;
    }

    void Start()
    {
        MostrarPanelDebug(debugActivo);
    }

    void Update()
    {
        if (Input.GetKeyDown(teclaToggleDebug))
        {
            debugActivo = !debugActivo;
            MostrarPanelDebug(debugActivo);
        }

        if (!debugActivo) return;

        if (Input.GetKeyDown(teclaLimpiar))
            AbrirLimpiar();

        if (Input.GetKeyDown(teclaCocina))
            AbrirCocina();

        if (Input.GetKeyDown(teclaCafe))
            AbrirCafe();

        if (Input.GetKeyDown(teclaServirPedido))
            AbrirServirPedido();

        if (Input.GetKeyDown(teclaActivarZonas))
            ActivarTodasLasZonas();

        if (Input.GetKeyDown(teclaForzarEntrega))
            ForzarEntrega();

        if (Input.GetKeyDown(teclaGameOver))
            GameoverManager.Instance?.ActivarGameOver();

        if (Input.GetKeyDown(teclaPanelSiguienteTurno))
            ForzarPanelSiguienteTurno();

        if (Input.GetKeyDown(teclaQuedarAUnoDelTurno))
            DejarTurnoAUno();

        if (Input.GetKey(teclaReinicio))
        {
            timerReinicio += Time.unscaledDeltaTime;
            if (timerReinicio >= tiempoParaReiniciar)
                ReiniciarEscena();
        }
        else
        {
            timerReinicio = 0f;
        }
    }

    public void AbrirLimpiar()
    {
        if (limpiarMinijuego != null)
            limpiarMinijuego.forzarEventoBuff = forzarBuffLimpieza;

        AbrirPanel(panelLimpiar, "Limpiar");
    }

    public void AbrirCocina()
    {
        if (cookingMinigame != null)
            cookingMinigame.forzarEventoNervios = forzarNerviosCocina;

        AbrirPanel(panelCocina, "Cocina");
    }

    public void AbrirCafe()
    {
        AbrirPanel(panelCafe, "Cafe");
    }

    public void AbrirServirPedido()
    {
        AbrirPanel(panelServirPedido, "Servir Pedido");
    }

    public bool AbrirServirPedidoDesdeEntrega(
        MinijuegoServirPEDIDO minijuegoServirPedido,
        string pedido,
        string mesa)
    {
        GameObject panelObjetivo = ObtenerPanelRaiz(panelServirPedido);

        if (panelObjetivo == null && minijuegoServirPedido != null)
            panelObjetivo = ObtenerPanelRaiz(minijuegoServirPedido.gameObject);

        if (panelObjetivo == null)
        {
            Debug.LogWarning("DebuggearJuegos: no hay panel asignado para Servir Pedido desde entrega.");
            return false;
        }

        if (minijuegoServirPedido != null)
        {
            if (minijuegoServirPedido.panelMinijuego == null)
                minijuegoServirPedido.panelMinijuego = panelObjetivo;
            minijuegoServirPedido.PrepararEntrega(pedido, mesa);
        }

        bool abierto = AbrirPanel(panelObjetivo, "Servir Pedido");
        return abierto;
    }

    public void ActivarTodasLasZonas()
    {
        ZonaInteractuable[] zonas = FindObjectsByType<ZonaInteractuable>(FindObjectsSortMode.None);
        foreach (ZonaInteractuable zona in zonas)
        {
            if (!zona.tareaActiva)
                zona.ActivarTarea();
        }

    }

    public void ForzarEntrega()
    {
        EntregaBandeja entrega = entregaBandeja != null ? entregaBandeja : EntregaBandeja.ObtenerInstancia();
        if (entrega == null)
        {
            Debug.LogWarning("DebuggearJuegos: no se encontro EntregaBandeja en la escena.");
            return;
        }

        bool ok = entrega.IniciarEntrega("debug");
    }

    public void ForzarPanelSiguienteTurno()
    {
        MinigameManager.Instance?.DebugForzarPanelSiguienteTurno();
    }

    public void DejarTurnoAUno()
    {
        MinigameManager.Instance?.DebugDejarAUnoDelSiguienteTurno();
    }

    bool AbrirPanel(GameObject panel, string nombre)
    {
        if (panel == null)
        {
            Debug.LogWarning($"DebuggearJuegos: no hay panel asignado para {nombre}.");
            return false;
        }

        if (MinigameManager.Instance == null)
        {
            Debug.LogWarning("DebuggearJuegos: no existe MinigameManager en la escena.");
            return false;
        }

        MinigameManager.Instance.AbrirMinijuego(panel);
        return true;
    }

    GameObject ObtenerPanelRaiz(GameObject panel)
    {
        if (panel == null)
            return null;

        Canvas canvas = panel.GetComponentInParent<Canvas>(true);
        if (canvas != null)
            return canvas.gameObject;

        return panel;
    }

    void ReiniciarEscena()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void MostrarPanelDebug(bool visible)
    {
        if (panelDebug != null)
            panelDebug.SetActive(visible);
    }
}
