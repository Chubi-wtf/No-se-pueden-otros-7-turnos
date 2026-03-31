using UnityEngine;
using TMPro;

public class EntregaBandeja : MonoBehaviour
{
    public static EntregaBandeja Instance;

    [Header("Mesas disponibles para entrega")]
    public Transform[] mesas;

    [Header("Indicador visual opcional")]
    public GameObject prefabIndicador;

    [Header("UI HUD")]
    public TextMeshProUGUI textoMeta;

    [Header("Deteccion de entrega")]
    public float radioEntregaFallback = 1.5f;

    [Header("Minijuego de servir")]
    public GameObject panelEntrega;
    public MinijuegoServirPEDIDO minijuegoServirPedido;

    private enum EstadoEntrega
    {
        Inactiva,
        EnCamino,
        EnMinijuego
    }

    private EstadoEntrega estadoActual = EstadoEntrega.Inactiva;
    private Transform mesaObjetivo;
    private TableController mesaObjetivoController;
    private SphereCollider colliderEntregaMesa;
    private GameObject indicadorActivo;
    private Transform playerTransform;
    private string nombrePedido = "pedido";

    void Awake()
    {
        Instance = this;
    }

    public static EntregaBandeja ObtenerInstancia()
    {
        if (Instance != null)
            return Instance;

        EntregaBandeja[] entregas = FindObjectsByType<EntregaBandeja>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        if (entregas.Length > 0)
            Instance = entregas[0];

        return Instance;
    }

    public bool HayEntregaActiva => estadoActual != EstadoEntrega.Inactiva;

    void Start()
    {
        playerTransform = FindFirstObjectByType<CubeMovement>()?.transform;
        OcultarTextoMeta();

        if (panelEntrega != null)
            panelEntrega.SetActive(false);
    }

    void Update()
    {
        if (playerTransform == null)
            playerTransform = FindFirstObjectByType<CubeMovement>()?.transform;

        if (estadoActual != EstadoEntrega.EnCamino || mesaObjetivo == null || playerTransform == null)
            return;

        bool cerca = EstaJugadorEnRangoDeMesa();
        ActualizarTextoMetaEnCamino(cerca);

        if (cerca)
        {
            Debug.Log($"EntregaBandeja: jugador en rango de {mesaObjetivo.name}, abriendo minijuego.");
            AbrirMinijuegoServir();
        }
    }

    public bool IniciarEntrega(string pedido)
    {
        if (estadoActual != EstadoEntrega.Inactiva)
        {
            Debug.LogWarning("EntregaBandeja: ya hay una entrega en curso.");
            return false;
        }

        if (mesas == null || mesas.Length == 0)
        {
            Debug.LogWarning("EntregaBandeja: no hay mesas asignadas.");
            return false;
        }

        nombrePedido = string.IsNullOrEmpty(pedido) ? "pedido" : pedido;
        mesaObjetivo = mesas[Random.Range(0, mesas.Length)];

        if (mesaObjetivo == null)
        {
            Debug.LogWarning("EntregaBandeja: la mesa seleccionada es nula.");
            return false;
        }

        mesaObjetivoController = BuscarMesaController(mesaObjetivo);
        colliderEntregaMesa = BuscarColliderEntrega(mesaObjetivo);

        if (mesaObjetivoController == null)
            Debug.LogWarning($"EntregaBandeja: la mesa '{mesaObjetivo.name}' no tiene TableController en el objeto, hijos o padres.");

        mesaObjetivoController?.ReceiveNewOrder();

        BandejaHUD bandeja = BuscarBandejaHUD();
        if (bandeja == null)
            Debug.LogWarning("EntregaBandeja: no se encontro BandejaHUD en la escena.");
        bandeja?.ActivarModoEntrega();

        if (prefabIndicador != null)
        {
            indicadorActivo = Instantiate(
                prefabIndicador,
                mesaObjetivo.position + Vector3.up * 1.5f,
                Quaternion.identity);
        }

        estadoActual = EstadoEntrega.EnCamino;
        ActualizarTextoMetaEnCamino(false);
        Debug.Log($"Entrega iniciada -> {nombrePedido} para {mesaObjetivo.name}");
        if (colliderEntregaMesa != null)
            Debug.Log($"EntregaBandeja: collider de entrega detectado en {colliderEntregaMesa.gameObject.name}");
        else
            Debug.LogWarning("EntregaBandeja: no se encontró SphereCollider de entrega, usando radio fallback.");
        return true;
    }

    public void CompletarEntregaDesdeMinijuego()
    {
        if (estadoActual == EstadoEntrega.Inactiva)
            return;

        Time.timeScale = 1f;
        estadoActual = EstadoEntrega.Inactiva;

        if (panelEntrega != null)
            panelEntrega.SetActive(false);

        if (indicadorActivo != null)
            Destroy(indicadorActivo);
        indicadorActivo = null;

        mesaObjetivoController?.CompleteOrder();
        mesaObjetivoController = null;
        mesaObjetivo = null;
        colliderEntregaMesa = null;

        BuscarBandejaHUD()?.DesactivarModoEntrega();
        OcultarTextoMeta();

        Debug.Log("Entrega completada!");
        MinigameManager.Instance?.CerrarMinijuego(true);
    }

    public void FallarEntregaDesdeMinijuego()
    {
        if (estadoActual == EstadoEntrega.Inactiva)
            return;

        Debug.Log("La entrega fallo durante el minijuego de servir.");
        SonidoManager.Instance?.Fallo();
        CancelarEntrega();
        MinigameManager.Instance?.CerrarMinijuego(false);
    }

    public void CancelarEntrega()
    {
        if (estadoActual == EstadoEntrega.Inactiva)
            return;

        Time.timeScale = 1f;
        estadoActual = EstadoEntrega.Inactiva;
        mesaObjetivoController?.CancelOrder();
        mesaObjetivoController = null;
        mesaObjetivo = null;
        colliderEntregaMesa = null;

        if (indicadorActivo != null)
            Destroy(indicadorActivo);
        indicadorActivo = null;

        if (panelEntrega != null)
            panelEntrega.SetActive(false);

        BuscarBandejaHUD()?.DesactivarModoEntrega();
        OcultarTextoMeta();
    }

    void AbrirMinijuegoServir()
    {
        if (estadoActual != EstadoEntrega.EnCamino)
            return;

        if (panelEntrega == null && minijuegoServirPedido != null)
            panelEntrega = minijuegoServirPedido.panelMinijuego;

        if (panelEntrega == null)
        {
            Debug.LogWarning("EntregaBandeja: no hay panelEntrega asignado para abrir el minijuego.");
            return;
        }

        estadoActual = EstadoEntrega.EnMinijuego;

        if (minijuegoServirPedido != null)
        {
            minijuegoServirPedido.PrepararEntrega(nombrePedido, mesaObjetivo != null ? mesaObjetivo.name : "Mesa");
            if (minijuegoServirPedido.panelMinijuego == null)
                minijuegoServirPedido.panelMinijuego = panelEntrega;
        }

        Debug.Log($"EntregaBandeja: intentando abrir panel {panelEntrega.name}");

        MinigameManager.Instance?.AbrirMinijuego(panelEntrega);
    }

    void ActualizarTextoMetaEnCamino(bool cerca)
    {
        if (textoMeta == null || mesaObjetivo == null) return;

        string accion = cerca
            ? "Apoyando bandeja..."
            : "Equilibra con <b>Q</b> y <b>E</b> y ve a la mesa";

        textoMeta.text = $"Lleva el {nombrePedido} a: <b>{mesaObjetivo.name}</b>\n{accion}";
        textoMeta.gameObject.SetActive(true);
    }

    void OcultarTextoMeta()
    {
        if (textoMeta != null)
            textoMeta.gameObject.SetActive(false);
    }

    TableController BuscarMesaController(Transform mesa)
    {
        TableController controller = mesa.GetComponent<TableController>();
        if (controller != null) return controller;

        controller = mesa.GetComponentInChildren<TableController>(true);
        if (controller != null) return controller;

        return mesa.GetComponentInParent<TableController>();
    }

    SphereCollider BuscarColliderEntrega(Transform mesa)
    {
        SphereCollider collider = mesa.GetComponent<SphereCollider>();
        if (collider != null) return collider;

        collider = mesa.GetComponentInChildren<SphereCollider>(true);
        if (collider != null) return collider;

        return mesa.GetComponentInParent<SphereCollider>();
    }

    bool EstaJugadorEnRangoDeMesa()
    {
        if (mesaObjetivo == null || playerTransform == null)
            return false;

        Vector2 posicionJugadorXZ = new Vector2(playerTransform.position.x, playerTransform.position.z);

        if (colliderEntregaMesa != null)
        {
            Vector3 centroMundo = colliderEntregaMesa.transform.TransformPoint(colliderEntregaMesa.center);
            Vector2 centroMesaXZ = new Vector2(centroMundo.x, centroMundo.z);
            float escalaMax = Mathf.Max(
                colliderEntregaMesa.transform.lossyScale.x,
                colliderEntregaMesa.transform.lossyScale.y,
                colliderEntregaMesa.transform.lossyScale.z);
            float radioMundo = colliderEntregaMesa.radius * escalaMax;
            return Vector2.Distance(posicionJugadorXZ, centroMesaXZ) <= radioMundo;
        }

        Vector2 posicionMesaXZ = new Vector2(mesaObjetivo.position.x, mesaObjetivo.position.z);
        return Vector2.Distance(posicionJugadorXZ, posicionMesaXZ) <= radioEntregaFallback;
    }

    BandejaHUD BuscarBandejaHUD()
    {
        BandejaHUD bandeja = FindFirstObjectByType<BandejaHUD>();
        if (bandeja != null)
            return bandeja;

        BandejaHUD[] bandejas = FindObjectsByType<BandejaHUD>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        return bandejas.Length > 0 ? bandejas[0] : null;
    }
}
