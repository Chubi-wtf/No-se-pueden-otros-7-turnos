using UnityEngine;
using UnityEngine.SceneManagement;

public class DebuggearJuegos : MonoBehaviour
{
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

    [Header("Eventos forzados")]
    public bool forzarBuffLimpieza = false;
    public bool forzarNerviosCocina = false;

    [Header("Reinicio")]
    public float tiempoParaReiniciar = 3f;
    public KeyCode teclaReinicio = KeyCode.R;
    private float timerReinicio = 0f;

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

    public void ActivarTodasLasZonas()
    {
        ZonaInteractuable[] zonas = FindObjectsByType<ZonaInteractuable>(FindObjectsSortMode.None);
        foreach (ZonaInteractuable zona in zonas)
        {
            if (!zona.tareaActiva)
                zona.ActivarTarea();
        }

        Debug.Log($"Debug -> zonas activadas: {zonas.Length}");
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
        Debug.Log(ok ? "Debug -> entrega forzada." : "Debug -> no se pudo forzar la entrega.");
    }

    void AbrirPanel(GameObject panel, string nombre)
    {
        if (panel == null)
        {
            Debug.LogWarning($"DebuggearJuegos: no hay panel asignado para {nombre}.");
            return;
        }

        if (MinigameManager.Instance == null)
        {
            Debug.LogWarning("DebuggearJuegos: no existe MinigameManager en la escena.");
            return;
        }

        MinigameManager.Instance.AbrirMinijuego(panel);
        Debug.Log($"Debug -> abriendo minijuego: {nombre}");
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
