using UnityEngine;
using UnityEngine.Events;
using TMPro;

public enum TipoMinijuego { Ninguno, Limpiar, Cocina, Cafe }

public class ZonaInteractuable : MonoBehaviour
{
    [Header("Configuracion")]
    public TipoMinijuego tipoMinijuego = TipoMinijuego.Ninguno;
    public string alertMessage = "Alerta!";
    public GameObject pantallaMinijuego;
    public UnityEvent onInteract;

    [Header("Tiempo")]
    public float tiempoLimite = 15f;

    [Header("Visuales")]
    public bool tareaActiva = false;
    public GameObject modeloVisual;
    public SpriteRenderer iconoEstado;
    public Sprite spritebueno;
    public Sprite spriteMedio;
    public Sprite spriteMalo;
    public SpriteRenderer iconoTarea;

    [HideInInspector] public string instruccionJuego = "";

    private float tiempoRestante;

    void Start()
    {
        switch (tipoMinijuego)
        {
            case TipoMinijuego.Limpiar:
                instruccionJuego = "Manten CLICK IZQUIERDO y mueve\nel mouse en todas direcciones.";
                break;
            case TipoMinijuego.Cocina:
                instruccionJuego = "Arrastra los ingredientes a los slots\nen el orden exacto de la secuencia.";
                break;
            case TipoMinijuego.Cafe:
                instruccionJuego = "Hay que hacer café! Sigue la secuencia de botones en pantalla";
                break;
            default:
                instruccionJuego = "";
                break;
        }

        ApagarTarea();
    }

    void Update()
    {
        if (!tareaActiva) return;

        tiempoRestante -= Time.deltaTime;

        if (iconoEstado != null)
        {
            float p = tiempoRestante / tiempoLimite;
            if (p > 0.5f) iconoEstado.sprite = spritebueno;
            else if (p > 0.2f) iconoEstado.sprite = spriteMedio;
            else iconoEstado.sprite = spriteMalo;
        }

        if (tiempoRestante <= 0f)
            FallarTarea();
    }

    public void ActivarTarea()
    {
        tareaActiva = true;
        tiempoRestante = tiempoLimite;

        if (modeloVisual != null) modeloVisual.SetActive(true);

        if (iconoEstado != null)
        {
            iconoEstado.sprite = spritebueno;
            iconoEstado.gameObject.SetActive(true);
        }

        if (iconoTarea != null) iconoTarea.gameObject.SetActive(true);
    }

    public void ApagarTarea()
    {
        tareaActiva = false;
        if (modeloVisual != null) modeloVisual.SetActive(false);
        if (iconoEstado != null) iconoEstado.gameObject.SetActive(false);
        if (iconoTarea != null) iconoTarea.gameObject.SetActive(false);
    }

    private void FallarTarea()
    {
        ApagarTarea();
        SanidadManager.Instance?.RecibirDañoMental();
    }

    public void Interact()
    {
        if (pantallaMinijuego != null)
            MinigameManager.Instance?.AbrirMinijuego(pantallaMinijuego);

        onInteract?.Invoke();
        ApagarTarea();
    }
}