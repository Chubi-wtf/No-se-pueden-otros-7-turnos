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