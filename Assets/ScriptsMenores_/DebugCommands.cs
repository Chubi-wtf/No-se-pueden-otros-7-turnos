using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugCommands : MonoBehaviour
{
    [Header("Canvas de Minijuegos")]
    public GameObject canvasMinijuegoCocina;
    public GameObject canvasMinijuegoCafe;
    public GameObject canvasMinijuegoLimpiar;

    [Header("Reinicio")]
    public float tiempoParaReiniciar = 3f;
    private float timerR = 0f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) Forzar(canvasMinijuegoCocina, "Cocina");
        if (Input.GetKeyDown(KeyCode.F2)) Forzar(canvasMinijuegoCafe, "Cafe");
        if (Input.GetKeyDown(KeyCode.F3)) Forzar(canvasMinijuegoLimpiar, "Limpiar");
        if (Input.GetKeyDown(KeyCode.F4)) ActivarTodasLasZonas();
        if (Input.GetKeyDown(KeyCode.F5)) GameoverManager.Instance?.ActivarGameOver();

        if (Input.GetKey(KeyCode.R))
        {
            timerR += Time.unscaledDeltaTime;
            Debug.Log($"Reiniciando... {(int)(timerR / tiempoParaReiniciar * 100)}%");
            if (timerR >= tiempoParaReiniciar) Reiniciar();
        }
        else timerR = 0f;
    }

    void Forzar(GameObject canvas, string nombre)
    {
        if (canvas == null) { Debug.LogWarning($"DebugCommands: '{nombre}' no asignado."); return; }
        MinigameManager.Instance?.AbrirMinijuego(canvas);
    }

    void ActivarTodasLasZonas()
    {
        var zonas = FindObjectsByType<ZonaInteractuable>(FindObjectsSortMode.None);
        foreach (var z in zonas) if (!z.tareaActiva) z.ActivarTarea();
        Debug.Log($"DEBUG: {zonas.Length} zonas activadas.");
    }

    void Reiniciar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}