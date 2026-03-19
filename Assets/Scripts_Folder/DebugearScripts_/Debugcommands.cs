using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugCommands : MonoBehaviour
{
    [Header("Minijuego de Cocina")]
    public GameObject canvasMinijuegoCocina;

    [Header("Minijuego del Café")]
    public GameObject canvasMinijuegoCafe;

    [Header("Reinicio")]
    public float tiempoParaReiniciar = 3f;
    private float timerR = 0f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            ForzarMinijuegoCocina();

        if (Input.GetKeyDown(KeyCode.F2))
            ForzarMinijuegoCafe();

        if (Input.GetKey(KeyCode.R))
        {
            timerR += Time.unscaledDeltaTime;

            float progreso = timerR / tiempoParaReiniciar;
            Debug.Log($"Reiniciando... {(int)(progreso * 100)}%");

            if (timerR >= tiempoParaReiniciar)
                Reiniciar();
        }
        else
        {
            timerR = 0f;
        }
    }

    void ForzarMinijuegoCocina()
    {
        if (canvasMinijuegoCocina == null) { Debug.LogWarning("DebugCommands: Canvas Cocina no asignado."); return; }
        MinigameManager.Instance?.AbrirMinijuego(canvasMinijuegoCocina);
    }

    void ForzarMinijuegoCafe()
    {
        if (canvasMinijuegoCafe == null) { Debug.LogWarning("DebugCommands: Canvas Café no asignado."); return; }
        MinigameManager.Instance?.AbrirMinijuego(canvasMinijuegoCafe);
    }

    void Reiniciar()
    {
        Time.timeScale = 1f; // por si estaba pausado
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}