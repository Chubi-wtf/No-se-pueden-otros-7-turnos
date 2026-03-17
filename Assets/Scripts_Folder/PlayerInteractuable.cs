using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    public GameObject alertUI;
    public TextMeshProUGUI alertText;
    private ZonaInteractuable currentInteractable;

    void Start()
    {
        alertUI.SetActive(false);
    }

    void Update()
    {
        if (currentInteractable != null && Input.GetKeyDown(KeyCode.F))
        {
            currentInteractable.Interact();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        ZonaInteractuable interactable = other.GetComponent<ZonaInteractuable>();
        if (interactable != null)
        {
            currentInteractable = interactable;
            alertText.text = interactable.alertMessage;
            alertUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<ZonaInteractuable>() != null)
        {
            currentInteractable = null;
            alertUI.SetActive(false);
        }
    }
}