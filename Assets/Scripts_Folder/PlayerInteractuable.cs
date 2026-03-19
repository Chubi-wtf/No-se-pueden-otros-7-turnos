using UnityEngine;
using System.Collections;

public class PlayerInteractuable : MonoBehaviour
{
    private ZonaInteractuable zonaActual;
    private Coroutine rutinaEspera;

    void OnTriggerStay(Collider other)
    {
        if (zonaActual != null) return;

        ZonaInteractuable zona = other.GetComponentInParent<ZonaInteractuable>();

        if (zona != null && zona.tareaActiva)
        {
            zonaActual = zona;

            Time.timeScale = 0f;

            if (UI_Manager.instance != null)
                UI_Manager.instance.MostrarAlerta(zona.alertMessage);

            rutinaEspera = StartCoroutine(EsperarYAbrirMinijuego(zona));
        }
    }

    void OnTriggerExit(Collider other)
    {
        ZonaInteractuable zona = other.GetComponentInParent<ZonaInteractuable>();

        if (zona != null && zona == zonaActual)
        {
            if (rutinaEspera != null)
            {
                StopCoroutine(rutinaEspera);
                rutinaEspera = null;
            }

            if (UI_Manager.instance != null)
                UI_Manager.instance.cerrarPanel();

            zonaActual = null;

            Time.timeScale = 1f;
        }
    }

    IEnumerator EsperarYAbrirMinijuego(ZonaInteractuable zona)
    {
        yield return new WaitForSecondsRealtime(2.5f);

        if (zona != null && zonaActual == zona)
        {
            if (UI_Manager.instance != null)
                UI_Manager.instance.cerrarPanel();

            zonaActual = null;

            
            zona.Interact();
        }
    }
}