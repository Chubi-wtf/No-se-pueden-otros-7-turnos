using UnityEngine;
using System.Collections.Generic;

public class GestorEventos : MonoBehaviour
{
    [Header("Todas las tareas del nivel")]
    public ZonaInteractuable[] todasLasZonas; 

    [Header("Ritmo de Aparici�n")]
    public float tiempoMinimoSpawn = 4f;
    public float tiempoMaximoSpawn = 8f;

    private float timerSpawn;

    void Start()
    {
        timerSpawn = Random.Range(tiempoMinimoSpawn, tiempoMaximoSpawn);
    }

    void Update()
    {
        timerSpawn -= Time.deltaTime;

        if (timerSpawn <= 0f)
        {
            ActivarEventoAleatorio();
            timerSpawn = Random.Range(tiempoMinimoSpawn, tiempoMaximoSpawn);
        }
    }

    void ActivarEventoAleatorio()
    {
        
        List<ZonaInteractuable> zonasDisponibles = new List<ZonaInteractuable>();

        foreach (ZonaInteractuable zona in todasLasZonas)
        {
            if (!zona.tareaActiva)
            {
                zonasDisponibles.Add(zona);
            }
        }

        if (zonasDisponibles.Count > 0)
        {
            int indice = Random.Range(0, zonasDisponibles.Count);
            zonasDisponibles[indice].ActivarTarea();
        }
    }
}