using UnityEngine;
using UnityEngine.Events;

public class ZonaInteractuable : MonoBehaviour
{
    public string alertMessage = "¡Alerta!";
    public GameObject pantallaMinijuego;
    public UnityEvent onInteract;

    public void Interact()
    {
        // Le quitamos la restricción de Time.timeScale > 0f
        if (pantallaMinijuego != null)
        {
            MinigameManager.Instance.AbrirMinijuego(pantallaMinijuego);
        }

        onInteract.Invoke();

        // El objeto desaparece de la escena
        gameObject.SetActive(false);
    }
}