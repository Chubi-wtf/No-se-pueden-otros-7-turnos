using UnityEngine;

public class DebuggearJuegos : MonoBehaviour
{
    public static DebuggearJuegos Instance;

    [Header("Debug")]
    public bool debugActivo = true;
    public KeyCode teclaLimpiar = KeyCode.F1;
    public KeyCode teclaCocina = KeyCode.F2;
    public KeyCode teclaCafe = KeyCode.F3;
    public KeyCode teclaServirPedido = KeyCode.F4;
    public KeyCode teclaActivarZonas = KeyCode.F5;
    public KeyCode teclaPanelSiguienteTurno = KeyCode.F10;

    [Header("Paneles de minijuego")]
    public GameObject panelLimpiar;
    public GameObject panelCocina;
    public GameObject panelCafe;
    public GameObject panelServirPedido;

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

    void Update()
    {
        if (!debugActivo)
            return;

        if (Input.GetKeyDown(teclaLimpiar))
            AbrirPanel(panelLimpiar);

        if (Input.GetKeyDown(teclaCocina))
            AbrirPanel(panelCocina);

        if (Input.GetKeyDown(teclaCafe))
            AbrirPanel(panelCafe);

        if (Input.GetKeyDown(teclaServirPedido))
            AbrirPanel(panelServirPedido);

        if (Input.GetKeyDown(teclaActivarZonas))
            ActivarTodasLasZonas();

        if (Input.GetKeyDown(teclaPanelSiguienteTurno))
            MinigameManager.Instance?.DebugForzarPanelSiguienteTurno();
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
            return false;

        if (minijuegoServirPedido != null)
        {
            if (minijuegoServirPedido.panelMinijuego == null)
                minijuegoServirPedido.panelMinijuego = panelObjetivo;

            minijuegoServirPedido.PrepararEntrega(pedido, mesa);
        }

        if (MinigameManager.Instance == null)
            return false;

        return AbrirPanel(panelObjetivo);
    }

    public void ActivarTodasLasZonas()
    {
        ZonaInteractuable[] zonas = FindObjectsByType<ZonaInteractuable>(FindObjectsSortMode.None);
        for (int i = 0; i < zonas.Length; i++)
        {
            if (zonas[i] != null && !zonas[i].tareaActiva && zonas[i].gameObject.activeInHierarchy)
                zonas[i].ActivarTarea();
        }
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

    bool AbrirPanel(GameObject panel)
    {
        if (panel == null || MinigameManager.Instance == null)
            return false;

        GameObject panelRaiz = ObtenerPanelRaiz(panel);
        if (panelRaiz == null)
            return false;

        MinigameManager.Instance.AbrirMinijuego(panelRaiz);
        return true;
    }
}
