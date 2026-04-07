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
        GameObject panelObjetivo = panelServirPedido;

        if (panelObjetivo == null && minijuegoServirPedido != null && minijuegoServirPedido.panelMinijuego != null)
            panelObjetivo = minijuegoServirPedido.panelMinijuego;
        else if (panelObjetivo == null && minijuegoServirPedido != null)
            panelObjetivo = minijuegoServirPedido.gameObject;

        if (panelObjetivo == null)
        {
            Debug.LogWarning("DebuggearJuegos: panelObjetivo quedo null al intentar abrir servir pedido.");
            return false;
        }

        if (minijuegoServirPedido != null)
        {
            if (minijuegoServirPedido.panelMinijuego == null)
                minijuegoServirPedido.panelMinijuego = panelObjetivo;

            minijuegoServirPedido.PrepararEntrega(pedido, mesa);
        }

        if (MinigameManager.Instance == null)
        {
            Debug.LogWarning("DebuggearJuegos: MinigameManager.Instance es null al abrir servir pedido.");
            return false;
        }

        Debug.Log(
            $"DebuggearJuegos: AbrirServirPedidoDesdeEntrega -> " +
            $"PanelInspector={(panelServirPedido != null ? panelServirPedido.name : "null")}, " +
            $"PanelObjetivo={panelObjetivo.name}, " +
            $"MinijuegoServir={(minijuegoServirPedido != null ? minijuegoServirPedido.name : "null")}");

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

    bool AbrirPanel(GameObject panel)
    {
        if (panel == null || MinigameManager.Instance == null)
        {
            Debug.LogWarning(
                $"DebuggearJuegos: AbrirPanel fallo. PanelNull={(panel == null)}, " +
                $"MinigameManagerNull={(MinigameManager.Instance == null)}");
            return false;
        }

        Debug.Log(
            $"DebuggearJuegos: AbrirPanel -> PanelOriginal={panel.name}, " +
            $"ActivoSelfAntes={panel.activeSelf}, ActivoJerarquiaAntes={panel.activeInHierarchy}");

        MinigameManager.Instance.AbrirMinijuego(panel);
        return true;
    }
}
