using UnityEngine;
using UnityEngine.UI;

public class BandejaHUD : MonoBehaviour
{
    [Header("UI")]
    public Slider sliderBandeja;
    public GameObject contenedorHUD;
    public CanvasGroup canvasGroupHUD;

    [Header("Fisica")]
    public float gravedad = 0.9f;
    public float fuerzaCorrecion = 2.0f;
    public float umbralCaida = 1f;

    [Header("Penalizacion normal")]
    public float dañoPorCaida = 10f;
    public float tiempoEntrepenalizaciones = 3f;

    private float timerPenalizacion = 0f;
    private float balance = 0f;
    private bool bandejaActiva = false;
    private bool modoEntrega = false;

    void Start()
    {
        ReiniciarBalance();
        InicializarSlider();
        MostrarHUD(false);
    }

    void InicializarSlider()
    {
        if (sliderBandeja == null) return;
        sliderBandeja.minValue = -1f;
        sliderBandeja.maxValue = 1f;
        sliderBandeja.value = balance;
    }

    void Update()
    {
        if (!bandejaActiva) return;

        float dir = Mathf.Sign(balance);
        if (balance == 0f) dir = Random.Range(0, 2) == 0 ? -1f : 1f;

        balance += dir * (gravedad + Mathf.Abs(balance) * gravedad) * Time.deltaTime;

        if (Input.GetKey(KeyCode.Q)) balance -= fuerzaCorrecion * Time.deltaTime;
        else if (Input.GetKey(KeyCode.E)) balance += fuerzaCorrecion * Time.deltaTime;

        balance = Mathf.Clamp(balance, -1f, 1f);
        if (sliderBandeja != null) sliderBandeja.value = balance;

        if (Mathf.Abs(balance) >= umbralCaida)
        {
            timerPenalizacion -= Time.deltaTime;
            if (timerPenalizacion <= 0f)
            {
                timerPenalizacion = tiempoEntrepenalizaciones;
                SanidadManager.Instance?.RecibirDañoPersonalizado(dañoPorCaida);
                balance = Random.Range(-0.15f, 0.15f);

                if (modoEntrega)
                {
                    EntregaBandeja.ObtenerInstancia()?.CancelarEntrega();
                    MinigameManager.Instance?.CerrarMinijuego(false);
                    modoEntrega = false;
                }
            }
        }
        else
        {
            timerPenalizacion = 0f;
        }
    }

    public void ActivarModoEntrega()
    {
        bandejaActiva = true;
        modoEntrega = true;
        timerPenalizacion = 0f;
        ReiniciarBalance();
        InicializarSlider();
        MostrarHUD(true);
    }

    public void DesactivarModoEntrega()
    {
        bandejaActiva = false;
        modoEntrega = false;
        timerPenalizacion = 0f;
        MostrarHUD(false);
    }

    void ReiniciarBalance()
    {
        balance = Random.Range(-0.1f, 0.1f);
    }

    void MostrarHUD(bool visible)
    {
        if (canvasGroupHUD != null)
        {
            canvasGroupHUD.alpha = visible ? 1f : 0f;
            canvasGroupHUD.interactable = visible;
            canvasGroupHUD.blocksRaycasts = visible;
            return;
        }

        if (contenedorHUD != null)
        {
            contenedorHUD.SetActive(visible);
            return;
        }

        if (sliderBandeja == null) return;

        sliderBandeja.enabled = visible;

        Graphic[] graficos = sliderBandeja.GetComponentsInChildren<Graphic>(true);
        foreach (Graphic grafico in graficos)
            grafico.enabled = visible;
    }
}
