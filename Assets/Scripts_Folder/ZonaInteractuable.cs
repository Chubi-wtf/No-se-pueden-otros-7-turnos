using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class ZonaInteractuable : MonoBehaviour
{
    [Header("Configuracion del Minijuego")]
    public string alertMessage = "Alerta!";
    public GameObject pantallaMinijuego;
    public UnityEvent onInteract;

    [Header("Sistema de Tiempo y Visuales")]
    public bool tareaActiva = false;
    public GameObject modeloVisual;
    public TextMeshPro exclamacionFlotante;

    [Header("Icono decorativo sobre el signo de exclamacion")]
    
    public SpriteRenderer iconoTarea;

    public float tiempoLimite = 15f;
    private float tiempoRestante;

    void Start()
    {
        ApagarTarea();
    }

    void Update()
    {
        if (!tareaActiva) return;

        tiempoRestante -= Time.deltaTime;

        if (exclamacionFlotante != null)
        {
            float p = tiempoRestante / tiempoLimite;
            if (p > 0.5f) exclamacionFlotante.color = Color.green;
            else if (p > 0.2f) exclamacionFlotante.color = Color.yellow;
            else exclamacionFlotante.color = Color.red;
        }

        if (tiempoRestante <= 0f)
            FallarTarea();
    }

    public void ActivarTarea()
    {
        tareaActiva = true;
        tiempoRestante = tiempoLimite;

        if (modeloVisual != null) modeloVisual.SetActive(true);

        if (exclamacionFlotante != null)
        {
            exclamacionFlotante.color = Color.green;
            exclamacionFlotante.gameObject.SetActive(true);
        }

        if (iconoTarea != null) iconoTarea.gameObject.SetActive(true);
    }

    public void ApagarTarea()
    {
        tareaActiva = false;

        if (modeloVisual != null) modeloVisual.SetActive(false);
        if (exclamacionFlotante != null) exclamacionFlotante.gameObject.SetActive(false);
        if (iconoTarea != null) iconoTarea.gameObject.SetActive(false);
    }

    private void FallarTarea()
    {
        Debug.Log("El cliente se fue o la mancha se seco!");
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