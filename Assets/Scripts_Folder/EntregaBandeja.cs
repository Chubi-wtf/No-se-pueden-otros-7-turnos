using UnityEngine;
using TMPro;

[System.Serializable]
public class MesaEntregaConfig
{
    public string nombreMesa = "Mesa";
    public Transform puntoMesa;
    public TableController tableController;
    public SphereCollider colliderEntrega;

    public Transform ObtenerTransform()
    {
        if (puntoMesa != null)
            return puntoMesa;

        if (tableController != null)
            return tableController.transform;

        if (colliderEntrega != null)
            return colliderEntrega.transform;

        return null;
    }

    public SphereCollider ObtenerCollider()
    {
        if (colliderEntrega != null)
            return colliderEntrega;

        if (tableController != null)
            return tableController.ObtenerColliderEntrega();

        Transform transformMesa = ObtenerTransform();
        if (transformMesa == null)
            return null;

        SphereCollider collider = transformMesa.GetComponent<SphereCollider>();
        if (collider != null)
            return collider;

        collider = transformMesa.GetComponentInChildren<SphereCollider>(true);
        if (collider != null)
            return collider;

        return transformMesa.GetComponentInParent<SphereCollider>();
    }
}

public class EntregaBandeja : MonoBehaviour
{
    public static EntregaBandeja Instance;

    [Header("Mesas disponibles para entrega")]
    public Transform[] mesas;

    [Header("Mesas configuradas en inspector")]
    public MesaEntregaConfig[] mesasConfiguradas;

    [Header("Busqueda directa por nombre")]
    public bool usarBusquedaPorNombre = true;
    public string[] nombresMesas = { "Mesapedido1", "Mesapedido2", "Mesapedido3" };

    [Header("Indicador visual opcional")]
    public GameObject prefabIndicador;

    [Header("UI HUD")]
    public TextMeshProUGUI textoMeta;

    [Header("Interaccion con mesa")]
    public GameObject panelInteraccionEntrega;
    public TextMeshProUGUI textoInteraccionEntrega;
    public string mensajeInteraccionEntrega = "Ve manteniendo el balance con Q y E, y aprieta la S cuando la barra este en verde!";

    [Header("Deteccion de entrega")]
    public float radioEntregaFallback = 1.5f;

    [Header("Minijuego de servir")]
    public GameObject panelEntrega;
    public MinijuegoServirPEDIDO minijuegoServirPedido;
    public float bloqueoTrasCerrarServir = 3f;

    private enum EstadoEntrega
    {
        Inactiva,
        EnCamino,
        EnMinijuego
    }

    private EstadoEntrega estadoActual = EstadoEntrega.Inactiva;
    private Transform mesaObjetivo;
    private MesaEntregaConfig mesaObjetivoConfig;
    private TableController mesaObjetivoController;
    private SphereCollider colliderEntregaMesa;
    private GameObject indicadorActivo;
    private Transform playerTransform;
    private string nombrePedido = "pedido";
    private bool colliderEntregaEraTrigger = false;
    private EntregaMesaTriggerRelay relayEntregaMesa;
    private float tiempoFinBloqueoServir = -1f;

    public bool HayEntregaActiva => estadoActual != EstadoEntrega.Inactiva;

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

    void Start()
    {
        playerTransform = FindFirstObjectByType<CubeMovement>()?.transform;
        OcultarTextoMeta();

        DebuggearJuegos debug = DebuggearJuegos.ObtenerInstancia();
        if (panelEntrega == null && debug != null && debug.panelServirPedido != null)
            panelEntrega = ObtenerPanelRaiz(debug.panelServirPedido);
        else if (panelEntrega == null && minijuegoServirPedido != null && minijuegoServirPedido.panelMinijuego != null)
            panelEntrega = ObtenerPanelRaiz(minijuegoServirPedido.panelMinijuego);
        else if (panelEntrega == null && minijuegoServirPedido != null)
            panelEntrega = ObtenerPanelRaiz(minijuegoServirPedido.gameObject);

        if (panelEntrega != null)
            panelEntrega.SetActive(false);

        MostrarPromptEntrega(false);
    }

    void Update()
    {
        if (playerTransform == null)
            playerTransform = FindFirstObjectByType<CubeMovement>()?.transform;

        if (EstaBloqueadoTemporalmenteTrasServir())
        {
            MostrarPromptEntrega(false);
            return;
        }

        if (estadoActual != EstadoEntrega.EnCamino || mesaObjetivo == null || playerTransform == null)
        {
            MostrarPromptEntrega(false);
            return;
        }

        bool cerca = EstaJugadorEnRangoDeMesa();
        ActualizarTextoMetaEnCamino(cerca);
        MostrarPromptEntrega(cerca);

        if (cerca && Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log(
                $"EntregaBandeja: se presiono F en rango. " +
                $"Mesa={mesaObjetivo?.name}, PanelEntregaAsignado={(panelEntrega != null)}, " +
                $"MinijuegoServirAsignado={(minijuegoServirPedido != null)}");
            AbrirMinijuegoServir();
        }
    }

    public bool IniciarEntrega(string pedido)
    {
        if (EstaBloqueadoTemporalmenteTrasServir())
            return false;

        if (estadoActual != EstadoEntrega.Inactiva)
        {
            Debug.LogWarning("EntregaBandeja: se intento iniciar una entrega mientras otra seguia activa.");
            return false;
        }

        MesaEntregaConfig[] mesasConfiguradasDisponibles = ObtenerMesasConfiguradasDisponibles();
        if (mesasConfiguradasDisponibles != null && mesasConfiguradasDisponibles.Length > 0)
        {
            mesaObjetivoConfig = mesasConfiguradasDisponibles[Random.Range(0, mesasConfiguradasDisponibles.Length)];
            mesaObjetivo = mesaObjetivoConfig.ObtenerTransform();
            mesaObjetivoController = mesaObjetivoConfig.tableController != null
                ? mesaObjetivoConfig.tableController
                : BuscarMesaController(mesaObjetivo);
            colliderEntregaMesa = mesaObjetivoConfig.ObtenerCollider();
        }
        else
        {
            Transform[] mesasDisponibles = ObtenerMesasDisponibles();
            if (mesasDisponibles == null || mesasDisponibles.Length == 0)
                return false;

            mesaObjetivo = mesasDisponibles[Random.Range(0, mesasDisponibles.Length)];
            mesaObjetivoConfig = null;
            mesaObjetivoController = BuscarMesaController(mesaObjetivo);
            colliderEntregaMesa = BuscarColliderEntrega(mesaObjetivo);
        }

        if (mesaObjetivo == null)
        {
            Debug.LogWarning("EntregaBandeja: no se pudo resolver una mesa objetivo.");
            return false;
        }

        nombrePedido = string.IsNullOrEmpty(pedido) ? "pedido" : pedido;
        PrepararTriggerEntrega();
        mesaObjetivoController?.ReceiveNewOrder();

        Debug.Log(
            $"EntregaBandeja: entrega iniciada. Pedido={nombrePedido}, Mesa={mesaObjetivo.name}, " +
            $"MesaController={(mesaObjetivoController != null ? mesaObjetivoController.name : "null")}, " +
            $"ColliderEntrega={(colliderEntregaMesa != null ? colliderEntregaMesa.name : "null")}");

        BandejaHUD bandeja = BuscarBandejaHUD();
        bandeja?.ActivarModoEntrega();

        if (prefabIndicador != null)
        {
            Vector3 posicionIndicador = mesaObjetivo.position;
            if (mesaObjetivoConfig != null && mesaObjetivoConfig.ObtenerTransform() != null)
                posicionIndicador = mesaObjetivoConfig.ObtenerTransform().position;

            indicadorActivo = Instantiate(
                prefabIndicador,
                posicionIndicador + Vector3.up * 1.5f,
                Quaternion.identity);
        }

        estadoActual = EstadoEntrega.EnCamino;
        ActualizarTextoMetaEnCamino(false);
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
        mesaObjetivoConfig = null;
        mesaObjetivoController = null;
        mesaObjetivo = null;
        LimpiarTriggerEntrega();
        colliderEntregaMesa = null;

        BuscarBandejaHUD()?.DesactivarModoEntrega();
        OcultarTextoMeta();
        MostrarPromptEntrega(false);
        ActivarBloqueoTemporalTrasServir();

        MinigameManager.Instance?.CerrarMinijuego(true);
    }

    public void FallarEntregaDesdeMinijuego()
    {
        if (estadoActual == EstadoEntrega.Inactiva)
            return;

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
        mesaObjetivoConfig = null;
        mesaObjetivoController = null;
        mesaObjetivo = null;
        LimpiarTriggerEntrega();
        colliderEntregaMesa = null;

        if (indicadorActivo != null)
            Destroy(indicadorActivo);
        indicadorActivo = null;

        if (panelEntrega != null)
            panelEntrega.SetActive(false);

        BuscarBandejaHUD()?.DesactivarModoEntrega();
        OcultarTextoMeta();
        MostrarPromptEntrega(false);
        ActivarBloqueoTemporalTrasServir();
    }

    public void LimpiarEstadoResidualSinEntrega()
    {
        if (estadoActual != EstadoEntrega.Inactiva)
        {
            CancelarEntrega();
            return;
        }

        if (indicadorActivo != null)
            Destroy(indicadorActivo);
        indicadorActivo = null;

        if (panelEntrega != null)
            panelEntrega.SetActive(false);

        LimpiarTriggerEntrega();
        colliderEntregaMesa = null;
        mesaObjetivoConfig = null;
        mesaObjetivoController = null;
        mesaObjetivo = null;

        BuscarBandejaHUD()?.DesactivarModoEntrega();
        OcultarTextoMeta();
        MostrarPromptEntrega(false);
    }

    void AbrirMinijuegoServir()
    {
        if (EstaBloqueadoTemporalmenteTrasServir())
            return;

        if (estadoActual != EstadoEntrega.EnCamino)
        {
            Debug.LogWarning($"EntregaBandeja: se intento abrir servir pero el estado actual es {estadoActual}.");
            return;
        }

        string nombreMesa = mesaObjetivo != null ? mesaObjetivo.name : "Mesa";
        DebuggearJuegos debug = DebuggearJuegos.ObtenerInstancia();

        if (debug != null && debug.panelServirPedido != null)
            panelEntrega = ObtenerPanelRaiz(debug.panelServirPedido);
        else if (minijuegoServirPedido != null && minijuegoServirPedido.panelMinijuego != null)
            panelEntrega = ObtenerPanelRaiz(minijuegoServirPedido.panelMinijuego);
        else if (panelEntrega == null && minijuegoServirPedido != null)
            panelEntrega = ObtenerPanelRaiz(minijuegoServirPedido.gameObject);

        if (panelEntrega == null)
        {
            Debug.LogWarning(
                "EntregaBandeja: no se pudo abrir el minijuego porque panelEntrega quedo null.");
            return;
        }

        Debug.Log(
            $"EntregaBandeja: abriendo servir pedido. " +
            $"PanelRaiz={panelEntrega.name}, ActivoEnJerarquia={panelEntrega.activeInHierarchy}, " +
            $"DebuggearJuegos={(debug != null)}, " +
            $"PanelDebug={(debug != null && debug.panelServirPedido != null ? debug.panelServirPedido.name : "null")}");

        estadoActual = EstadoEntrega.EnMinijuego;
        MostrarPromptEntrega(false);

        Debug.Log("EntregaBandeja: llamando a MinigameManager.AbrirMinijuego desde EntregaBandeja.");
        MinigameManager.Instance?.AbrirMinijuego(panelEntrega);

        if (minijuegoServirPedido != null)
        {
            minijuegoServirPedido.AbrirPanelPropio(nombrePedido, nombreMesa);
            Debug.Log(
                $"EntregaBandeja: panelMinijuego preparado en " +
                $"{(minijuegoServirPedido.panelMinijuego != null ? minijuegoServirPedido.panelMinijuego.name : "null")}");
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

    void PrepararTriggerEntrega()
    {
        if (colliderEntregaMesa == null)
            return;

        colliderEntregaEraTrigger = colliderEntregaMesa.isTrigger;
        colliderEntregaMesa.isTrigger = true;

        relayEntregaMesa = colliderEntregaMesa.GetComponent<EntregaMesaTriggerRelay>();
        if (relayEntregaMesa == null)
            relayEntregaMesa = colliderEntregaMesa.gameObject.AddComponent<EntregaMesaTriggerRelay>();

        relayEntregaMesa.Configurar(this);
    }

    void LimpiarTriggerEntrega()
    {
        if (relayEntregaMesa != null)
        {
            Destroy(relayEntregaMesa);
            relayEntregaMesa = null;
        }

        if (colliderEntregaMesa != null)
            colliderEntregaMesa.isTrigger = colliderEntregaEraTrigger;

        colliderEntregaEraTrigger = false;
    }

    public void NotificarJugadorEnTrigger(Collider other)
    {
        if (EstaBloqueadoTemporalmenteTrasServir())
            return;

        if (estadoActual != EstadoEntrega.EnCamino || colliderEntregaMesa == null)
            return;

        if (!EsColliderDelJugador(other))
            return;

        Debug.Log(
            $"EntregaBandeja: jugador detectado en trigger de entrega. " +
            $"Mesa={mesaObjetivo?.name}, Collider={colliderEntregaMesa.name}");
        MostrarPromptEntrega(true);
    }

    void ActualizarTextoMetaEnCamino(bool cerca)
    {
        if (textoMeta == null || mesaObjetivo == null)
            return;

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

    void MostrarPromptEntrega(bool visible)
    {
        if (panelInteraccionEntrega != null)
            panelInteraccionEntrega.SetActive(visible);

        if (textoInteraccionEntrega != null)
        {
            textoInteraccionEntrega.text = mensajeInteraccionEntrega;
            textoInteraccionEntrega.gameObject.SetActive(visible);
        }
    }

    bool EstaBloqueadoTemporalmenteTrasServir()
    {
        return Time.unscaledTime < tiempoFinBloqueoServir;
    }

    void ActivarBloqueoTemporalTrasServir()
    {
        float duracion = Mathf.Max(0f, bloqueoTrasCerrarServir);
        if (duracion <= 0f)
            return;

        tiempoFinBloqueoServir = Time.unscaledTime + duracion;

        if (panelEntrega != null)
            panelEntrega.SetActive(false);

        if (minijuegoServirPedido != null)
        {
            if (minijuegoServirPedido.panelPrevioMinijuego != null)
                minijuegoServirPedido.panelPrevioMinijuego.SetActive(false);

            if (minijuegoServirPedido.panelMinijuego != null)
                minijuegoServirPedido.panelMinijuego.SetActive(false);
        }
    }

    TableController BuscarMesaController(Transform mesa)
    {
        TableController controller = mesa.GetComponent<TableController>();
        if (controller != null)
            return controller;

        controller = mesa.GetComponentInChildren<TableController>(true);
        if (controller != null)
            return controller;

        return mesa.GetComponentInParent<TableController>();
    }

    SphereCollider BuscarColliderEntrega(Transform mesa)
    {
        TableController controller = BuscarMesaController(mesa);
        if (controller != null)
        {
            SphereCollider colliderDesdeMesa = controller.ObtenerColliderEntrega();
            if (colliderDesdeMesa != null)
                return colliderDesdeMesa;
        }

        SphereCollider collider = mesa.GetComponent<SphereCollider>();
        if (collider != null)
            return collider;

        collider = mesa.GetComponentInChildren<SphereCollider>(true);
        if (collider != null)
            return collider;

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

    bool EsColliderDelJugador(Collider other)
    {
        if (other == null || playerTransform == null)
            return false;

        if (other.transform == playerTransform || other.transform.IsChildOf(playerTransform))
            return true;

        CharacterController characterController = playerTransform.GetComponent<CharacterController>();
        if (characterController != null && other == characterController)
            return true;

        return false;
    }

    Transform[] ObtenerMesasDisponibles()
    {
        if (usarBusquedaPorNombre && nombresMesas != null && nombresMesas.Length > 0)
        {
            System.Collections.Generic.List<Transform> mesasEncontradas = new System.Collections.Generic.List<Transform>();
            Transform[] transforms = FindObjectsByType<Transform>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            foreach (string nombreMesa in nombresMesas)
            {
                if (string.IsNullOrWhiteSpace(nombreMesa))
                    continue;

                for (int i = 0; i < transforms.Length; i++)
                {
                    if (transforms[i] == null || transforms[i].name != nombreMesa)
                        continue;

                    if (!transforms[i].gameObject.activeInHierarchy)
                        continue;

                    mesasEncontradas.Add(transforms[i]);
                    break;
                }
            }

            if (mesasEncontradas.Count > 0)
                return mesasEncontradas.ToArray();
        }

        if (mesas != null && mesas.Length > 0)
        {
            System.Collections.Generic.List<Transform> mesasActivas = new System.Collections.Generic.List<Transform>();
            for (int i = 0; i < mesas.Length; i++)
            {
                if (mesas[i] == null)
                    continue;

                if (!mesas[i].gameObject.activeInHierarchy)
                    continue;

                mesasActivas.Add(mesas[i]);
            }

            return mesasActivas.ToArray();
        }

        return mesas;
    }

    MesaEntregaConfig[] ObtenerMesasConfiguradasDisponibles()
    {
        if (mesasConfiguradas == null || mesasConfiguradas.Length == 0)
            return null;

        System.Collections.Generic.List<MesaEntregaConfig> lista = new System.Collections.Generic.List<MesaEntregaConfig>();
        for (int i = 0; i < mesasConfiguradas.Length; i++)
        {
            if (mesasConfiguradas[i] == null)
                continue;

            Transform transformMesa = mesasConfiguradas[i].ObtenerTransform();
            if (transformMesa == null)
                continue;

            if (!transformMesa.gameObject.activeInHierarchy)
                continue;

            lista.Add(mesasConfiguradas[i]);
        }

        return lista.Count > 0 ? lista.ToArray() : null;
    }
}

public class EntregaMesaTriggerRelay : MonoBehaviour
{
    private EntregaBandeja entregaBandeja;

    public void Configurar(EntregaBandeja entrega)
    {
        entregaBandeja = entrega;
    }

    void OnTriggerEnter(Collider other)
    {
        entregaBandeja?.NotificarJugadorEnTrigger(other);
    }

    void OnTriggerStay(Collider other)
    {
        entregaBandeja?.NotificarJugadorEnTrigger(other);
    }
}
