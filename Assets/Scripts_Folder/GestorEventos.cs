using System.Collections.Generic;
using UnityEngine;

public class GestorEventos : MonoBehaviour
{
    [Header("Todas las tareas del nivel")]
    public ZonaInteractuable[] todasLasZonas;

    [Header("Busqueda automatica")]
    public bool buscarZonasAutomaticamente = true;
    public bool incluirInactivosAlBuscar = false;

    [Header("Ritmo de Aparicion Base")]
    public float tiempoMinimoSpawn = 4f;
    public float tiempoMaximoSpawn = 8f;

    [Header("Ritmo por nivel/turno")]
    public bool usarRitmoPorNivel = true;
    public float[] tiemposMinimosPorNivel = { 4f, 3f, 2f };
    public float[] tiemposMaximosPorNivel = { 4f, 3f, 2f };

    private float timerSpawn;

    void Start()
    {
        RefrescarZonas();
        ReiniciarTimerSpawn();
    }

    void Update()
    {
        if (buscarZonasAutomaticamente)
            RefrescarZonas();

        timerSpawn -= Time.deltaTime;

        if (timerSpawn <= 0f)
        {
            ActivarEventoAleatorio();
            ReiniciarTimerSpawn();
        }
    }

    public void RefrescarZonas()
    {
        List<ZonaInteractuable> zonas = new List<ZonaInteractuable>();

        ZonaInteractuable[] encontradas = FindObjectsByType<ZonaInteractuable>(
            incluirInactivosAlBuscar ? FindObjectsInactive.Include : FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        for (int i = 0; i < encontradas.Length; i++)
        {
            ZonaInteractuable zona = encontradas[i];
            if (zona == null || !zona.gameObject.activeInHierarchy)
                continue;

            zonas.Add(zona);
        }

        todasLasZonas = zonas.ToArray();
    }

    void ActivarEventoAleatorio()
    {
        if (todasLasZonas == null || todasLasZonas.Length == 0)
            return;

        List<ZonaInteractuable> zonasDisponibles = new List<ZonaInteractuable>();

        for (int i = 0; i < todasLasZonas.Length; i++)
        {
            ZonaInteractuable zona = todasLasZonas[i];
            if (zona == null || !zona.gameObject.activeInHierarchy)
                continue;

            if (!zona.tareaActiva)
                zonasDisponibles.Add(zona);
        }

        if (zonasDisponibles.Count <= 0)
            return;

        int indice = Random.Range(0, zonasDisponibles.Count);
        zonasDisponibles[indice].ActivarTarea();
    }

    void ReiniciarTimerSpawn()
    {
        ObtenerRangoSpawnActual(out float minimo, out float maximo);
        timerSpawn = Random.Range(minimo, maximo);
    }

    void ObtenerRangoSpawnActual(out float minimo, out float maximo)
    {
        minimo = tiempoMinimoSpawn;
        maximo = tiempoMaximoSpawn;

        if (!usarRitmoPorNivel)
            return;

        int indiceNivel = 0;

        if (MinigameManager.Instance != null)
            indiceNivel = Mathf.Clamp(MinigameManager.Instance.turnoActual, 0, 2);
        else if (ManejoDenivel.Instance != null)
            indiceNivel = Mathf.Clamp(ManejoDenivel.Instance.NivelActual, 0, 2);

        if (tiemposMinimosPorNivel != null && tiemposMinimosPorNivel.Length > 0)
        {
            int indiceMin = Mathf.Clamp(indiceNivel, 0, tiemposMinimosPorNivel.Length - 1);
            minimo = tiemposMinimosPorNivel[indiceMin];
        }

        if (tiemposMaximosPorNivel != null && tiemposMaximosPorNivel.Length > 0)
        {
            int indiceMax = Mathf.Clamp(indiceNivel, 0, tiemposMaximosPorNivel.Length - 1);
            maximo = tiemposMaximosPorNivel[indiceMax];
        }

        minimo = Mathf.Max(0.1f, minimo);
        maximo = Mathf.Max(minimo, maximo);
    }
}
