using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class ZonaInteractuable : MonoBehaviour
{
    [Header("Configuracion del Minijuego")]
    public string alertMessage = "Alerta!";
    public GameObject pantallaMinijuego;
    public UnityEvent onInteract;

    [Header("Sistema de Tiempo")]
    public bool tareaActiva = false;
    public GameObject modeloVisual;
    public float tiempoLimite = 15f;
    private float tiempoRestante;

    [Header("Icono de estado (SpriteRenderer)")]
    public SpriteRenderer iconoEstado;
    public Sprite spritebueno;
    public Sprite spriteMedio;
    public Sprite spriteMalo;

    [Header("Icono decorativo del tipo de tarea")]
    public SpriteRenderer iconoTarea;

    void Start()
    {
        if (modeloVisual != null && modeloVisual.name == "Bandeja")
            Debug.LogError($"ZonaInteractuable en '{gameObject.name}' tiene 'Bandeja' en Modelo Visual!");

        if (pantallaMinijuego != null && pantallaMinijuego.name == "Bandeja")
            Debug.LogError($"ZonaInteractuable en '{gameObject.name}' tiene 'Bandeja' en Pantalla Minijuego!");

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
        Debug.Log("Tarea fallada!");
        ApagarTarea();
        SanidadManager.Instance?.RecibirDañoMental();
    }

    public void Interact()
    {
        if (pantallaMinijuego != null)
            MinigameManager.Instance.AbrirMinijuego(pantallaMinijuego);

        onInteract.Invoke();
        ApagarTarea();
    }
}