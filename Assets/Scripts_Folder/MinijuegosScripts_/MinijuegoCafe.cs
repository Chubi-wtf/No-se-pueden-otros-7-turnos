using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Behaviour = UnityEngine.Behaviour;

public class MinijuegoCafe : MonoBehaviour
{
    [Header("Compartido")]
    public Slider barraTemporizador;
    public TextMeshProUGUI textoFase;
    public Transform puntoCamaraMinijuego;
    public Behaviour controladorVistaJugador;

    [Header("Fase 1 - Llenar el cafe")]
    public GameObject panelFase1;
    public Slider sliderCafe;
    public TextMeshProUGUI textoInstrFase1;
    public Transform objetoVertido;
    public GameObject prefabLiquidoCafe;
    public Transform puntoSalidaLiquido;

    [Header("Fase 2 - Secuencia")]
    public GameObject panelFase2;
    public Image imagenCafe;
    public TextMeshProUGUI textoSecuencia;
    public TextMeshProUGUI textoInstrFase2;

    [Header("Ajustes Fase 1")]
    public float velocidadLlenado = 0.9f;
    public float velocidadVaciado = 0.4f;
    public float velocidadInclinacion = 1.75f;
    public float anguloMaximoInclinacion = 75f;
    [Range(0f, 1f)] public float porcentajeInicioVertido = 0.7f;
    public float intervaloLiquido = 0.06f;
    public float vidaLiquido = 2f;
    public float fuerzaInicialLiquido = 1.5f;

    [Header("Ajustes Fase 2")]
    public Sprite[] posiblesCafes;
    public Sprite[] imagenesProgresoSecuencia;
    public int longitudSecuencia = 5;

    [Header("Tiempo total")]
    public float tiempoMaximo = 10f;

    private enum Fase
    {
        Cafe,
        Secuencia,
        Terminado
    }

    private Fase faseActual;
    private float nivelCafe = 0f;
    private float inclinacionActual = 0f;
    private float tiempoRestante = 0f;
    private bool minijuegoActivo = false;
    private float timerLiquido = 0f;
    private Quaternion rotacionBaseVertido = Quaternion.identity;
    private Camera camaraPrincipal;
    private Vector3 posicionOriginalCamara;
    private Quaternion rotacionOriginalCamara;
    private bool restaurarCamaraPendiente = false;
    private bool controladorVistaOriginalActivo = false;
    private List<Transform> cubosLiquido = new List<Transform>();
    private Vector3 posicionLocalOriginalVertido = Vector3.zero;
    private bool vertidoOriginalGuardado = false;

    private readonly KeyCode[] teclasPermitidas =
        { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.W, KeyCode.Q, KeyCode.E, KeyCode.F };
    private List<KeyCode> secuenciaActual = new List<KeyCode>();
    private int indiceActual = 0;

    void OnEnable()
    {
        PrepararCamaraMinijuego();

        float multiplicadorTiempo = MinigameManager.Instance != null
            ? MinigameManager.Instance.ObtenerMultiplicadorTiempoTurnoActual()
            : 1f;
        tiempoRestante = tiempoMaximo * multiplicadorTiempo;
        nivelCafe = 0f;
        inclinacionActual = 0f;
        timerLiquido = 0f;
        indiceActual = 0;
        minijuegoActivo = true;
        faseActual = Fase.Cafe;
        secuenciaActual.Clear();
        cubosLiquido.Clear();

        for (int i = 0; i < longitudSecuencia; i++)
            secuenciaActual.Add(teclasPermitidas[Random.Range(0, teclasPermitidas.Length)]);

        if (barraTemporizador != null)
        {
            barraTemporizador.maxValue = tiempoRestante;
            barraTemporizador.value = tiempoRestante;
        }

        ActualizarImagenCafe();

        if (sliderCafe != null)
        {
            sliderCafe.minValue = 0f;
            sliderCafe.maxValue = 1f;
            sliderCafe.value = 0f;
        }

        if (objetoVertido != null)
        {
            if (!vertidoOriginalGuardado)
            {
                posicionLocalOriginalVertido = objetoVertido.localPosition;
                rotacionBaseVertido = objetoVertido.localRotation;
                vertidoOriginalGuardado = true;
            }

            objetoVertido.localPosition = posicionLocalOriginalVertido;
            rotacionBaseVertido = objetoVertido.localRotation;
            objetoVertido.localRotation = rotacionBaseVertido;
        }

        MostrarFase(Fase.Cafe);
        ActualizarSecuencia();
    }

    void OnDisable()
    {
        RestaurarObjetoVertido();
        RestaurarCamaraJugador();
    }

    void Update()
    {
        for (int i = cubosLiquido.Count - 1; i >= 0; i--)
        {
            if (cubosLiquido[i] == null)
            {
                cubosLiquido.RemoveAt(i);
            }
            else
            {
                cubosLiquido[i].position += (Vector3.down + puntoSalidaLiquido.forward * 0.5f) * fuerzaInicialLiquido * Time.unscaledDeltaTime;
            }
        }

        if (!minijuegoActivo || faseActual == Fase.Terminado) return;

        tiempoRestante -= Time.unscaledDeltaTime;

        if (barraTemporizador != null)
        {
            barraTemporizador.value = tiempoRestante;

            if (barraTemporizador.fillRect != null)
            {
                Image fillImage = barraTemporizador.fillRect.GetComponent<Image>();
                if (fillImage != null && barraTemporizador.maxValue > 0)
                {
                    float porcentaje = barraTemporizador.value / barraTemporizador.maxValue;
                    fillImage.color = Color.Lerp(Color.red, Color.green, porcentaje);
                }
            }
        }

        if (tiempoRestante <= 0f)
        {
            Perder();
            return;
        }

        if (faseActual == Fase.Cafe) UpdateFase1();
        else if (faseActual == Fase.Secuencia) UpdateFase2();
    }

    void UpdateFase1()
    {
        if (Input.GetKey(KeyCode.Space))
            inclinacionActual += velocidadInclinacion * Time.unscaledDeltaTime;
        else
            inclinacionActual -= velocidadInclinacion * Time.unscaledDeltaTime;

        inclinacionActual = Mathf.Clamp01(inclinacionActual);

        bool estaVertiendo = Input.GetKey(KeyCode.Space) && inclinacionActual >= porcentajeInicioVertido;

        if (estaVertiendo)
        {
            nivelCafe += velocidadLlenado * Time.unscaledDeltaTime;
            ActualizarLiquidoVisual();
        }
        else
        {
            nivelCafe -= velocidadVaciado * Time.unscaledDeltaTime;
            timerLiquido = 0f;
        }

        nivelCafe = Mathf.Clamp01(nivelCafe);

        if (sliderCafe != null)
            sliderCafe.value = nivelCafe;

        if (sliderCafe != null)
        {
            Image fill = sliderCafe.fillRect?.GetComponent<Image>();
            if (fill != null)
                fill.color = Color.Lerp(Color.white, new Color(0.6f, 0.3f, 0.1f), nivelCafe);
        }

        if (objetoVertido != null)
        {
            float anguloActual = Mathf.Lerp(0f, anguloMaximoInclinacion, inclinacionActual);
            objetoVertido.localRotation = rotacionBaseVertido * Quaternion.Euler(0f, 0f, -anguloActual);
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
                ActualizarImagenCafe();
                ActualizarSecuencia();
                if (indiceActual >= secuenciaActual.Count) Ganar();
            }
            else
            {
                indiceActual = 0;
                ActualizarImagenCafe();
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
            textoFase.text = f == Fase.Cafe ? "Fase 1 / 2 - Sirve el cafe"
                                            : "Fase 2 / 2 - Revuelve la crema";

        if (textoInstrFase1 != null)
            textoInstrFase1.text = "Manten Espacio para dejar caer el cafe";

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

    void ActualizarImagenCafe()
    {
        if (imagenCafe == null)
            return;

        Sprite[] spritesFuente = null;

        if (imagenesProgresoSecuencia != null && imagenesProgresoSecuencia.Length > 0)
            spritesFuente = imagenesProgresoSecuencia;
        else if (posiblesCafes != null && posiblesCafes.Length > 0)
            spritesFuente = posiblesCafes;

        if (spritesFuente == null || spritesFuente.Length == 0)
            return;

        if (spritesFuente.Length == 1)
        {
            imagenCafe.sprite = spritesFuente[0];
            return;
        }

        float progreso = longitudSecuencia > 0
            ? Mathf.Clamp01((float)indiceActual / longitudSecuencia)
            : 0f;

        int indiceSprite = Mathf.Clamp(
            Mathf.RoundToInt(progreso * (spritesFuente.Length - 1)),
            0,
            spritesFuente.Length - 1);

        imagenCafe.sprite = spritesFuente[indiceSprite];
    }

    void Ganar()
    {
        if (faseActual == Fase.Terminado) return;

        faseActual = Fase.Terminado;
        minijuegoActivo = false;

        SonidoManager.Instance?.Acierto();
        RestaurarObjetoVertido();
        RestaurarCamaraJugador();

        EntregaBandeja entrega = EntregaBandeja.ObtenerInstancia();
        bool entregaIniciada = entrega != null && entrega.IniciarEntrega("cafe");

        if (entregaIniciada)
            MinigameManager.Instance?.CerrarMinijuegoPausa();
        else
            MinigameManager.Instance?.CerrarMinijuego(true);
    }

    void Perder()
    {
        if (faseActual == Fase.Terminado) return;

        faseActual = Fase.Terminado;
        minijuegoActivo = false;
        RestaurarObjetoVertido();
        RestaurarCamaraJugador();
        EntregaBandeja.ObtenerInstancia()?.LimpiarEstadoResidualSinEntrega();

        SonidoManager.Instance?.Fallo();
        MinigameManager.Instance?.CerrarMinijuego(false);
    }

    void ActualizarLiquidoVisual()
    {
        if (prefabLiquidoCafe == null || puntoSalidaLiquido == null) return;

        timerLiquido -= Time.unscaledDeltaTime;
        if (timerLiquido > 0f) return;

        timerLiquido = intervaloLiquido;

        GameObject liquido = Instantiate(
            prefabLiquidoCafe,
            puntoSalidaLiquido.position,
            puntoSalidaLiquido.rotation);

        cubosLiquido.Add(liquido.transform);
        Destroy(liquido, vidaLiquido);
    }

    void PrepararCamaraMinijuego()
    {
        camaraPrincipal = Camera.main;
        if (camaraPrincipal == null) return;

        posicionOriginalCamara = camaraPrincipal.transform.position;
        rotacionOriginalCamara = camaraPrincipal.transform.rotation;
        restaurarCamaraPendiente = true;

        if (controladorVistaJugador != null)
        {
            controladorVistaOriginalActivo = controladorVistaJugador.enabled;
            controladorVistaJugador.enabled = false;
        }

        if (puntoCamaraMinijuego != null)
        {
            camaraPrincipal.transform.SetPositionAndRotation(
                puntoCamaraMinijuego.position,
                puntoCamaraMinijuego.rotation);
        }
    }

    void RestaurarCamaraJugador()
    {
        if (!restaurarCamaraPendiente) return;

        if (camaraPrincipal == null)
            camaraPrincipal = Camera.main;

        if (camaraPrincipal != null)
            camaraPrincipal.transform.SetPositionAndRotation(posicionOriginalCamara, rotacionOriginalCamara);

        if (controladorVistaJugador != null)
            controladorVistaJugador.enabled = controladorVistaOriginalActivo;

        restaurarCamaraPendiente = false;
    }

    void RestaurarObjetoVertido()
    {
        if (objetoVertido == null || !vertidoOriginalGuardado) return;

        objetoVertido.localPosition = posicionLocalOriginalVertido;
        objetoVertido.localRotation = rotacionBaseVertido;
        inclinacionActual = 0f;
    }
}
