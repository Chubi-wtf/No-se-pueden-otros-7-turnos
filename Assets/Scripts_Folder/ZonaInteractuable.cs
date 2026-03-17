using UnityEngine;
using UnityEngine.Events;

public class ZonaInteractuable : MonoBehaviour
{
    public string alertMessage = "¡Alerta!";
    public GameObject pantallaMinijuego;
    public UnityEvent onInteract;

    private bool playerInRange = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            Interact();
        }
    }

    public void Interact()
    {
        if (Time.timeScale > 0f && pantallaMinijuego != null)
        {
            MinigameManager.Instance.AbrirMinijuego(pantallaMinijuego);
        }
        onInteract.Invoke();
    }
}