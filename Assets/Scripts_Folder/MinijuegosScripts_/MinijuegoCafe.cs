using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MinijuegoCafe : MonoBehaviour
{
    [Header("Referencias UI - Secuencia")]
    public Image imagenCafe;
    public TextMeshProUGUI textoSecuencia;
    public TextMeshProUGUI textoTemporizador;

    [Header("Referencias UI - Evento Barra")]
    [Tooltip("Panel o contenedor del Slider para ocultarlo si no hay evento")]
    public GameObject panelEventoBarra;
    public Slider barraCafe;
    public TextMeshProUGUI textoEvento;

    [Header("Ajustes de la Secuencia")]
    public Sprite[] posiblesCafes;
    public int longitudSecuencia = 5;
    public float tiempoMaximo = 4f;

    [Header("Ajustes del Evento (1 de 5 veces)")]
    [Tooltip("Tiempo que se suma al minijuego si sale el evento")]
    public float tiempoExtraConEvento = 3.5f;
    public float velocidadLlenado = 0.8f;
    public float velocidadVaciado = 0.5f;
    public int vecesNecesarias = 2;
    public bool forzarEvento = false;

    private float nivelCafe = 0f;
    private int vecesCompletadas = 0;
    private bool eventoActivo = false;
    private bool eventoResuelto = false;

    private KeyCode[] teclasPermitidas = { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.W, KeyCode.Q, KeyCode.E, KeyCode.F };
    private List<KeyCode> secuenciaActual = new List<KeyCode>();
    private int indiceActual = 0;
    private float tiempoRestante = 0f;
    private bool minijuegoActivo = false;

    void OnEnable()
    {
        if (posiblesCafes != null && posiblesCafes.Length > 0)
            imagenCafe.sprite = posiblesCafes[Random.Range(0, posiblesCafes.Length)];

        secuenciaActual.Clear();
        for (int i = 0; i < longitudSecuencia; i++)
            secuenciaActual.Add(teclasPermitidas[Random.Range(0, teclasPermitidas.Length)]);

        indiceActual = 0;
        minijuegoActivo = true;

        eventoActivo = forzarEvento || (Random.Range(0, 5) == 0);
        forzarEvento = false;
        eventoResuelto = !eventoActivo; 
        vecesCompletadas = 0;
        nivelCafe = 0f;

        tiempoRestante = tiempoMaximo + (eventoActivo ? tiempoExtraConEvento : 0f);

        if (panelEventoBarra != null)
            panelEventoBarra.SetActive(eventoActivo);

        if (barraCafe != null)
        {
            barraCafe.minValue = 0f;
            barraCafe.maxValue = 1f;
            barraCafe.value = 0f;
        }

        ActualizarTextoVisual();
        ActualizarTextoEvento();
    }

    void Update()
    {
        if (!minijuegoActivo) return;

        tiempoRestante -= Time.unscaledDeltaTime;
        if (textoTemporizador != null)
            textoTemporizador.text = Mathf.Max(0f, tiempoRestante).ToString("F1") + "s";

        if (tiempoRestante <= 0f) { PerderMinijuego(); return; }

        if (eventoActivo && !eventoResuelto)
        {
            if (Input.GetKey(KeyCode.Space))
                nivelCafe += velocidadLlenado * Time.unscaledDeltaTime;
            else
                nivelCafe -= velocidadVaciado * Time.unscaledDeltaTime;

            nivelCafe = Mathf.Clamp01(nivelCafe);

            if (barraCafe != null) barraCafe.value = nivelCafe;

            if (nivelCafe >= 1f)
            {
                vecesCompletadas++;
                nivelCafe = 0f; 

                if (vecesCompletadas >= vecesNecesarias)
                {
                    eventoResuelto = true;
                    if (panelEventoBarra != null) panelEventoBarra.SetActive(false);
                }
            }

            ActualizarTextoEvento();
        }

        if (indiceActual < secuenciaActual.Count)
        {
            ComprobarTeclas();
        }

        if (indiceActual >= secuenciaActual.Count && eventoResuelto)
        {
            GanarMinijuego();
        }
        if (PauseMenu.isPaused) return;
    }

    void ActualizarTextoEvento()
    {
        if (textoEvento == null) return;

        if (!eventoActivo)
        {
            textoEvento.text = "";
            return;
        }

        if (eventoResuelto)
        {
            textoEvento.text = "<color=#00FF88>¡Presión estabilizada!</color>";
        }
        else
        {
            textoEvento.text = $"<color=#FFFF00>¡Mantén <b>SPACE</b> para presurizar!</color>\n{vecesCompletadas}/{vecesNecesarias}";
        }
    }

    void ComprobarTeclas()
    {
        foreach (KeyCode tecla in teclasPermitidas)
        {
            if (!Input.GetKeyDown(tecla)) continue;

            if (tecla == secuenciaActual[indiceActual])
            {
                indiceActual++;
                ActualizarTextoVisual();
            }
            else
            {
                indiceActual = 0;
                ActualizarTextoVisual();
            }
        }
    }

    void ActualizarTextoVisual()
    {
        string txt = "";
        for (int i = 0; i < secuenciaActual.Count; i++)
        {
            string nombre = secuenciaActual[i].ToString();
            if (i < indiceActual) txt += "<color=#00FF00>" + nombre + "</color> ";
            else if (i == indiceActual) txt += "<color=#FFFF00>" + nombre + "</color> ";
            else txt += "<color=#FFFFFF>" + nombre + "</color> ";
        }
        textoSecuencia.text = txt;
    }

    void GanarMinijuego()
    {
        minijuegoActivo = false;
        Debug.Log("Cafe perfecto!");
        MinigameManager.Instance?.CerrarMinijuego(true);
    }

    void PerderMinijuego()
    {
        minijuegoActivo = false;
        Debug.Log("Tiempo agotado!");
        MinigameManager.Instance?.CerrarMinijuego(false);
    }
}