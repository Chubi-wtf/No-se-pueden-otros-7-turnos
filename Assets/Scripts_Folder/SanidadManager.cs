using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SanidadManager : MonoBehaviour
{
    public static SanidadManager Instance;

    [Header("Valores de Sanidad")]
    public float sanidadMaxima = 100f;
    public float sanidadActual;
    public float daoPorFallo = 25f;
    public float recuperacionPorExito = 25f;

    [Header("Interfaz (UI)")]
    public Slider barraSanidad;

    [Header("Efectos Visuales")]
    public Volume volumenGlobal;
    private ColorAdjustments ajustesColor;
    private Vignette efectoVinetado;

    [Header("Gracia inicial")]
    public float tiempoGracia = 3f;
    private float timerGracia = 0f;
    private bool graciaActiva = true;

    void Awake()
    {
        Instance = this;
        // Fuerza 100 en Awake
        sanidadActual = sanidadMaxima;

        if (barraSanidad != null)
        {
            barraSanidad.maxValue = sanidadMaxima;
            barraSanidad.value = sanidadMaxima; 
        }
    }

    void Start()
    {
        sanidadActual = sanidadMaxima;

        if (volumenGlobal != null)
        {
            volumenGlobal.profile.TryGet(out ajustesColor);
            volumenGlobal.profile.TryGet(out efectoVinetado);
        }

        ActualizarVisuales();
    }

    void Update()
    {
        if (graciaActiva)
        {
            timerGracia += Time.deltaTime;
            if (timerGracia >= tiempoGracia)
                graciaActiva = false;
        }
    }

    public void RecibirDañoMental()
    {
        if (graciaActiva)
        {
            Debug.Log("Daño bloqueado por gracia inicial.");
            return;
        }

        sanidadActual = Mathf.Max(0f, sanidadActual - daoPorFallo);
        ActualizarVisuales();

        if (sanidadActual <= 0f)
            Debug.Log("COLAPSO MENTAL! Has perdido la partida.");
    }

    public void RecuperarSanidad()
    {
        sanidadActual = Mathf.Min(sanidadMaxima, sanidadActual + recuperacionPorExito);
        ActualizarVisuales();
    }

    void ActualizarVisuales()
    {
        float p = sanidadActual / sanidadMaxima;

        if (barraSanidad != null)
            barraSanidad.value = sanidadActual;

        if (ajustesColor != null)
        {
            ajustesColor.postExposure.value = Mathf.Lerp(-2.5f, 0f, p);
            ajustesColor.contrast.value = Mathf.Lerp(-40f, 0f, p);
        }

        if (efectoVinetado != null)
            efectoVinetado.intensity.value = Mathf.Lerp(0.6f, 0f, p);
    }
}