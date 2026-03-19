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
    public float dañoPorFallo = 25f;

    [Header("Interfaz (UI)")]
    public Slider barraSanidad; 

    [Header("Efectos Visuales")]
    public Volume volumenGlobal;
    private ColorAdjustments ajustesColor;
    private Vignette efectoVinetado; 

    void Awake()
    {
        Instance = this;
        sanidadActual = sanidadMaxima;

        if (barraSanidad != null)
        {
            barraSanidad.maxValue = sanidadMaxima;
            barraSanidad.value = sanidadActual;
        }
    }

    void Start()
    {
        if (volumenGlobal != null)
        {
            volumenGlobal.profile.TryGet(out ajustesColor);
            volumenGlobal.profile.TryGet(out efectoVinetado);

            ActualizarVisuales();
        }
    }

    public void RecibirDañoMental()
    {
        sanidadActual -= dañoPorFallo;
        if (sanidadActual < 0f) sanidadActual = 0f;

        ActualizarVisuales();

        if (sanidadActual <= 0f)
        {
            Debug.Log("¡COLAPSO MENTAL! Has perdido la partida.");
        }
    }

    void ActualizarVisuales()
    {
        float porcentajeCordura = sanidadActual / sanidadMaxima;

        if (barraSanidad != null)
        {
            barraSanidad.value = sanidadActual;
        }

        if (ajustesColor != null)
        {
            ajustesColor.postExposure.value = Mathf.Lerp(-2.5f, 0f, porcentajeCordura);
            ajustesColor.contrast.value = Mathf.Lerp(-40f, 0f, porcentajeCordura);
        }

        if (efectoVinetado != null)
        {
        
            efectoVinetado.intensity.value = Mathf.Lerp(0.6f, 0f, porcentajeCordura);
        }
    }
}