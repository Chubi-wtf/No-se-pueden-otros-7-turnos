using UnityEngine;
using System.Collections;

public class PlayerInteractuable : MonoBehaviour
{
    private ZonaInteractuable zonaActual;
    private Coroutine rutinaEspera;


    void OnTriggerEnter(Collider other)
    {
        TryEntrar(other);
    }

    void OnTriggerStay(Collider other)
    {
        if (zonaActual == null) TryEntrar(other);
    }

    void OnTriggerExit(Collider other)
    {
        ZonaInteractuable zona = other.GetComponentInParent<ZonaInteractuable>();

        if (zona != null && zona == zonaActual)
        {
            if (rutinaEspera != null) { StopCoroutine(rutinaEspera); rutinaEspera = null; }
            UI_Manager.instance?.cerrarPanel();
            zonaActual = null;
            Time.timeScale = 1f;
        }
    }


    void TryEntrar(Collider other)
    {
        if (zonaActual != null) return;
        if (GestorEventos.Instance != null && GestorEventos.Instance.EstaEnCooldownPostMinijuego) return;

        EntregaBandeja entrega = EntregaBandeja.ObtenerInstancia();
        if (entrega != null && entrega.HayEntregaActiva) return;

        ZonaInteractuable zona = other.GetComponentInParent<ZonaInteractuable>();
        if (zona == null || !zona.tareaActiva) return;

        zonaActual = zona;
        Time.timeScale = 0f;

        UI_Manager.instance?.MostrarAlerta(zona.alertMessage, zona.instruccionJuego);

        rutinaEspera = StartCoroutine(EsperarYAbrir(zona));
    }

    IEnumerator EsperarYAbrir(ZonaInteractuable zona)
    {
        yield return new WaitForSecondsRealtime(2.5f);

        bool cooldownActivo = GestorEventos.Instance != null && GestorEventos.Instance.EstaEnCooldownPostMinijuego;

        if (zona != null && zonaActual == zona && zona.tareaActiva && !cooldownActivo)
        {
            UI_Manager.instance?.cerrarPanel();
            zonaActual = null;
            zona.Interact();
        }
    }

    public void CancelarInteraccionActual()
    {
        if (rutinaEspera != null)
        {
            StopCoroutine(rutinaEspera);
            rutinaEspera = null;
        }

        zonaActual = null;
        UI_Manager.instance?.cerrarPanel();
        Time.timeScale = 1f;
    }
}
