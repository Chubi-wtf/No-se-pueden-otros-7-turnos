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

            // 1. ¡CONGELAMOS EL JUEGO AL INSTANTE!
            Time.timeScale = 0f;

            // 2. Iniciamos el temporizador que ignora la pausa
            rutinaEspera = StartCoroutine(EsperarYAbrirMinijuego(zona));
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Como el jugador está congelado, teóricamente no puede salir de la zona,
        // pero dejamos esta comprobación de seguridad por si acaso.
        ZonaInteractuable zona = other.GetComponent<ZonaInteractuable>();
        if (zona != null && zona == zonaActual)
        {
            if (rutinaEspera != null)
            {
                StopCoroutine(rutinaEspera);
            }

            alertaPanel.SetActive(false);
            zonaActual = null;

            // Si por algún milagro se sale de la zona, le devolvemos el tiempo
            Time.timeScale = 1f;
        }
    }

    IEnumerator EsperarYAbrirMinijuego(ZonaInteractuable zona)
    {
        // ¡CRUCIAL! Usamos Realtime para que los 2.5s pasen aunque el juego esté en pausa (TimeScale = 0)
        yield return new WaitForSecondsRealtime(2.5f);

        if (zona != null && zonaActual == zona)
        {
            alertaPanel.SetActive(false);

            // Desvinculamos la zona antes de interactuar para evitar bugs con el OnTriggerExit
            zonaActual = null;

            // Abrimos el minijuego de limpieza
            zona.Interact();
        }
    }
}