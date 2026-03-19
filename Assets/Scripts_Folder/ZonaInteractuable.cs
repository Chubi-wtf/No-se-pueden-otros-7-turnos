using UnityEngine;
using UnityEngine.Events;

public class ZonaInteractuable : MonoBehaviour
{
    public string alertMessage = "¡Alerta!";
    public GameObject pantallaMinijuego;
    public UnityEvent onInteract;

    public void Interact()
    {
        if (pantallaMinijuego != null)
        {
            MinigameManager.Instance.AbrirMinijuego(pantallaMinijuego);
        }

        onInteract.Invoke();

        gameObject.SetActive(false);
    }
}