using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerInteractuable : MonoBehaviour
{
    [Header("Referencias UI (Generales)")]
    public GameObject alertaPanel;
    public TextMeshProUGUI alertaText;

    private ZonaInteractuable zonaActual;
    private Coroutine rutinaEspera;

    void Start()
    {
        if (alertaPanel != null) alertaPanel.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        ZonaInteractuable zona = other.GetComponent<ZonaInteractuable>();

        if (zona != null)
        {
            zonaActual = zona;
            alertaText.text = zona.alertMessage;
            alertaPanel.SetActive(true);

           
            Time.timeScale = 0f;

            rutinaEspera = StartCoroutine(EsperarYAbrirMinijuego(zona));
        }
    }

    void OnTriggerExit(Collider other)
    {
      
        ZonaInteractuable zona = other.GetComponent<ZonaInteractuable>();
        if (zona != null && zona == zonaActual)
        {
            if (rutinaEspera != null)
            {
                StopCoroutine(rutinaEspera);
            }

            alertaPanel.SetActive(false);
            zonaActual = null;

            Time.timeScale = 1f;
        }
    }

    IEnumerator EsperarYAbrirMinijuego(ZonaInteractuable zona)
    {
        yield return new WaitForSecondsRealtime(2.5f);

        if (zona != null && zonaActual == zona)
        {
            alertaPanel.SetActive(false);

            zonaActual = null;

            zona.Interact();
        }
    }
}