using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugCommands : MonoBehaviour
{
    [Header("Minijuego de Cocina")]
    public GameObject canvasMinijuegoCocina;

    [Header("Minijuego del Cafe")]
    public GameObject canvasMinijuegoCafe;

    [Header("Minijuego de Limpiar")]
    public GameObject canvasMinijuegoLimpiar;

    [Header("Reinicio")]
    public float tiempoParaReiniciar = 3f;
    private float timerR = 0f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) ForzarMinijuego(canvasMinijuegoCocina, "Cocina");
        if (Input.GetKeyDown(KeyCode.F2)) ForzarMinijuego(canvasMinijuegoCafe, "Cafe");
        if (Input.GetKeyDown(KeyCode.F3)) ForzarMinijuego(canvasMinijuegoLimpiar, "Limpiar");

        if (Input.GetKeyDown(KeyCode.F4)) ActivarTodasLasZonas();

        // F5 — Cocina forzando el microjuego de nervios (ignora el random 1/5)
        if (Input.GetKeyDown(KeyCode.F5)) ForzarCocinaConNervios();

        if (Input.GetKey(KeyCode.R))
        {
            timerR += Time.unscaledDeltaTime;
            Debug.Log($"Reiniciando... {(int)(timerR / tiempoParaReiniciar * 100)}%");
            if (timerR >= tiempoParaReiniciar) Reiniciar();
        }
        else
        {
            timerR = 0f;
        }
    }

    void ForzarMinijuego(GameObject canvas, string nombre)
    {
        if (canvas == null) { Debug.LogWarning($"DebugCommands: Canvas {nombre} no asignado."); return; }
        MinigameManager.Instance?.AbrirMinijuego(canvas);
    }

    void ForzarCocinaConNervios()
    {
        if (canvasMinijuegoCocina == null)
        {
            Debug.LogWarning("DebugCommands: Canvas Cocina no asignado.");
            return;
        }

        CookingMinigame cm = canvasMinijuegoCocina.GetComponentInChildren<CookingMinigame>(true);
        if (cm == null)
        {
            Debug.LogWarning("DebugCommands: No se encontró CookingMinigame en el canvas de Cocina.");
            return;
        }

        // Forzar el flag ANTES de que OnEnable lo tire al azar
        cm.forzarEventoNervios = true;
        Debug.Log("DEBUG [F5]: Cocina + microjuego de nervios forzado.");

        MinigameManager.Instance?.AbrirMinijuego(canvasMinijuegoCocina);
    }

    void ActivarTodasLasZonas()
    {
        ZonaInteractuable[] zonas = FindObjectsByType<ZonaInteractuable>(FindObjectsSortMode.None);

        foreach (ZonaInteractuable zona in zonas)
        {
            if (!zona.tareaActiva)
                zona.ActivarTarea();
        }

        Debug.Log($"DEBUG: {zonas.Length} zonas activadas.");
    }

    void Reiniciar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}