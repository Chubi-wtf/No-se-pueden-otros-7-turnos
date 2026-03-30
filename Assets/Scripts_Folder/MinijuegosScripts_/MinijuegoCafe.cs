using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MinijuegoCafe : MonoBehaviour
{
    [Header("Compartido")]
    public TextMeshProUGUI textoTemporizador;
    public TextMeshProUGUI textoFase;

    [Header("Fase 1 — Llenar el café")]
    public GameObject panelFase1;
    public Slider sliderCafe;
    public TextMeshProUGUI textoInstrFase1;

    [Header("Fase 2 — Secuencia")]
    public GameObject panelFase2;
    public Image imagenCafe;
    public TextMeshProUGUI textoSecuencia;
    public TextMeshProUGUI textoInstrFase2;

    [Header("Ajustes Fase 1")]
    public float velocidadLlenado = 0.9f;
    public float velocidadVaciado = 0.4f;

    [Header("Ajustes Fase 2")]
    public Sprite[] posiblesCafes;
    public int longitudSecuencia = 5;

    [Header("Tiempo total")]
    public float tiempoMaximo = 10f;

    private enum Fase { Cafe, Secuencia, Terminado }
    private Fase faseActual;

    private float nivelCafe = 0f;
    private float tiempoRestante = 0f;
    private bool minijuegoActivo = false;

    private readonly KeyCode[] teclasPermitidas =
        { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.W, KeyCode.Q, KeyCode.E, KeyCode.F };
    private List<KeyCode> secuenciaActual = new List<KeyCode>();
    private int indiceActual = 0;

    void OnEnable()
    {
        tiempoRestante = tiempoMaximo;
        nivelCafe = 0f;
        indiceActual = 0;
        minijuegoActivo = true;
        faseActual = Fase.Cafe;
        secuenciaActual.Clear();
        for (int i = 0; i < longitudSecuencia; i++)
            secuenciaActual.Add(teclasPermitidas[Random.Range(0, teclasPermitidas.Length)]);

        if (posiblesCafes != null && posiblesCafes.Length > 0 && imagenCafe != null)
            imagenCafe.sprite = posiblesCafes[Random.Range(0, posiblesCafes.Length)];
        if (sliderCafe != null) { sliderCafe.minValue = 0f; sliderCafe.maxValue = 1f; sliderCafe.value = 0f; }

        MostrarFase(Fase.Cafe);
        ActualizarSecuencia();
    }

    void Update()
    {
        if (!minijuegoActivo || faseActual == Fase.Terminado) return;

        tiempoRestante -= Time.unscaledDeltaTime;
        if (textoTemporizador != null)
            textoTemporizador.text = Mathf.Max(0f, tiempoRestante).ToString("F1") + "s";

        if (tiempoRestante <= 0f) { Perder(); return; }

        if (faseActual == Fase.Cafe) UpdateFase1();
        else if (faseActual == Fase.Secuencia) UpdateFase2();
    }

    void UpdateFase1()
    {
        if (Input.GetKey(KeyCode.Space))
            nivelCafe += velocidadLlenado * Time.unscaledDeltaTime;
        else
            nivelCafe -= velocidadVaciado * Time.unscaledDeltaTime;

        nivelCafe = Mathf.Clamp01(nivelCafe);
        if (sliderCafe != null) sliderCafe.value = nivelCafe;

        if (sliderCafe != null)
        {
            var fill = sliderCafe.fillRect?.GetComponent<Image>();
            if (fill != null)
                fill.color = Color.Lerp(Color.white, new Color(0.6f, 0.3f, 0.1f), nivelCafe);
        }

        if (nivelCafe >= 1f)
        {
            faseActual = Fase.Secuencia;
            MostrarFase(Fase.Secuencia);
        }
    }

    void UpdateFase2()
    {
        foreach (KeyCode tecla in teclasPermitidas)
        {
            if (!Input.GetKeyDown(tecla)) continue;

            if (tecla == secuenciaActual[indiceActual])
            {
                indiceActual++;
                ActualizarSecuencia();
                if (indiceActual >= secuenciaActual.Count) Ganar();
            }
            else
            {
                indiceActual = 0;
                ActualizarSecuencia();
                if (textoSecuencia != null)
                    StartCoroutine(FlashError());
            }
        }
    }

    IEnumerator FlashError()
    {
        if (textoSecuencia != null) textoSecuencia.color = Color.red;
        yield return new WaitForSecondsRealtime(0.25f);
        if (textoSecuencia != null) textoSecuencia.color = Color.white;
    }

    void MostrarFase(Fase f)
    {
        if (panelFase1 != null) panelFase1.SetActive(f == Fase.Cafe);
        if (panelFase2 != null) panelFase2.SetActive(f == Fase.Secuencia);

        if (textoFase != null)
            textoFase.text = f == Fase.Cafe ? "Fase 1 / 2  —  Sirve el café"
                                            : "Fase 2 / 2  —  Revuelve la crema";

        if (textoInstrFase1 != null)
            textoInstrFase1.text = "Mantén <b>SPACE</b> para llenar la taza.\n¡No se te derrame!";

        if (textoInstrFase2 != null)
            textoInstrFase2.text = "Sigue la secuencia de teclas:\n<b>W A S D Q E F</b>";
    }

    void ActualizarSecuencia()
    {
        if (textoSecuencia == null) return;
        string txt = "";
        for (int i = 0; i < secuenciaActual.Count; i++)
        {
            string n = secuenciaActual[i].ToString();
            if (i < indiceActual) txt += $"<color=#00FF00>{n}</color> ";
            else if (i == indiceActual) txt += $"<color=#FFFF00>{n}</color> ";
            else txt += $"<color=#FFFFFF>{n}</color> ";
        }
        textoSecuencia.text = txt;
    }

    void Ganar()
    {
        if (faseActual == Fase.Terminado) return;
        faseActual = Fase.Terminado;
        minijuegoActivo = false;
        Debug.Log("Café listo — iniciando entrega.");

        SonidoManager.Instance?.Acierto();
        MinigameManager.Instance?.CerrarMinijuego(true);
    }

    void Perder()
    {
        if (faseActual == Fase.Terminado) return;
        faseActual = Fase.Terminado;
        minijuegoActivo = false;
        Debug.Log("Tiempo agotado en el café.");

        SonidoManager.Instance?.Fallo();
        MinigameManager.Instance?.CerrarMinijuego(false);
    }
}