using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class SanidadManager : MonoBehaviour
{
    public static SanidadManager Instance;

    [Header("Valores de Sanidad")]
    public float sanidadMaxima = 100f;
    public float sanidadActual;
    public float daoPorFallo = 20f;
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

    private bool inmunidadActiva = false;
    private Coroutine rutinaInmunidad;

    void Awake()
    {
        Instance = this;
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
        MusicaManager.Instance?.ActualizarPitch(1f);
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
        if (graciaActiva) { Debug.Log("Daño bloqueado por gracia."); return; }
        if (inmunidadActiva) { Debug.Log("Daño bloqueado por inmunidad."); return; }
        AplicarDaño(daoPorFallo);
    }

    public void RecibirDañoPersonalizado(float cantidad)
    {
        if (graciaActiva) { Debug.Log("Daño bloqueado por gracia."); return; }
        if (inmunidadActiva) { Debug.Log("Daño bloqueado por inmunidad."); return; }
        AplicarDaño(cantidad);
    }


    public void RecuperarSanidad()
    {
        sanidadActual = Mathf.Min(sanidadMaxima, sanidadActual + recuperacionPorExito);
        ActualizarVisuales();
    }



    public void ActivarInmunidadCordura(float duracion)
    {
        if (rutinaInmunidad != null) StopCoroutine(rutinaInmunidad);
        rutinaInmunidad = StartCoroutine(RutinaInmunidad(duracion));
    }

    private IEnumerator RutinaInmunidad(float duracion)
    {
        inmunidadActiva = true;
        Debug.Log($"Inmunidad a pérdida de cordura activa por {duracion}s");
        yield return new WaitForSeconds(duracion);
        inmunidadActiva = false;
        Debug.Log("Inmunidad de cordura expirada.");
    }


    private void AplicarDaño(float cantidad)
    {
        sanidadActual = Mathf.Max(0f, sanidadActual - cantidad);
        ActualizarVisuales();

        if (sanidadActual <= 0f)
        {
            Debug.Log("COLAPSO MENTAL!");
            GameoverManager.Instance?.ActivarGameOver();
        }
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

        MusicaManager.Instance?.ActualizarPitch(p);
    }
}