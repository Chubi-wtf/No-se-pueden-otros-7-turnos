using UnityEngine;
using TMPro;

public class EntregaBandeja : MonoBehaviour
{
    public static EntregaBandeja Instance;

    [Header("Mesas disponibles para entrega")]
    public Transform[] mesas;             

    [Header("Indicador visual en la mesa objetivo")]
    public GameObject prefabIndicador;    

    [Header("UI HUD")]
    public TextMeshProUGUI textoMeta;     

    [Header("Radio de entrega")]
    public float radioEntrega = 1.5f;

    private Transform mesaObjetivo;
    private GameObject indicadorActivo;
    private bool entregaActiva = false;
    private Transform playerTransform;

    void Awake() { Instance = this; }

    void Start()
    {
        playerTransform = FindFirstObjectByType<CubeMovement>()?.transform;
        if (textoMeta != null) textoMeta.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!entregaActiva || mesaObjetivo == null || playerTransform == null) return;

        float dist = Vector3.Distance(playerTransform.position, mesaObjetivo.position);

        if (dist <= radioEntrega)
            CompletarEntrega();
    }


    public void IniciarEntrega()
    {
        if (mesas == null || mesas.Length == 0)
        {
            Debug.LogWarning("EntregaBandeja: no hay mesas asignadas.");
            MinigameManager.Instance?.CerrarMinijuego(true);  
            return;
        }

        mesaObjetivo = mesas[Random.Range(0, mesas.Length)];
        entregaActiva = true;

        BandejaHUD bandeja = FindFirstObjectByType<BandejaHUD>();
        bandeja?.ActivarModoEntrega();

        if (prefabIndicador != null)
            indicadorActivo = Instantiate(prefabIndicador,
                mesaObjetivo.position + Vector3.up * 1.5f, Quaternion.identity);

        if (textoMeta != null)
        {
            textoMeta.text = $"Lleva el café a: <b>{mesaObjetivo.name}</b>";
            textoMeta.gameObject.SetActive(true);
        }

        Debug.Log($"Entrega iniciada — objetivo: {mesaObjetivo.name}");
    }

    private void CompletarEntrega()
    {
        entregaActiva = false;

        if (indicadorActivo != null) Destroy(indicadorActivo);
        if (textoMeta != null) textoMeta.gameObject.SetActive(false);

        BandejaHUD bandeja = FindFirstObjectByType<BandejaHUD>();
        bandeja?.DesactivarModoEntrega();

        Debug.Log("¡Café entregado!");
        MinigameManager.Instance?.CerrarMinijuego(true);  
    }

    public void CancelarEntrega()
    {
        entregaActiva = false;
        if (indicadorActivo != null) Destroy(indicadorActivo);
        if (textoMeta != null) textoMeta.gameObject.SetActive(false);

        BandejaHUD bandeja = FindFirstObjectByType<BandejaHUD>();
        bandeja?.DesactivarModoEntrega();
    }
}