using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameoverManager : MonoBehaviour
{
    public static GameoverManager Instance;

    [Header("Panel de Game Over")]
    public GameObject panelGameOver;

    [Header("Textos opcionales")]
    public TextMeshProUGUI textoTitulo;
    public TextMeshProUGUI textoSubtitulo;

    private bool gameOverActivado = false;

    void Awake()
    {
        Instance = this;
        if (panelGameOver != null) panelGameOver.SetActive(false);
    }

    public void ActivarGameOver()
    {
        if (gameOverActivado) return;
        gameOverActivado = true;

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (textoTitulo != null) textoTitulo.text = "COLAPSO MENTAL";
        if (textoSubtitulo != null) textoSubtitulo.text = "No pudiste aguantar el turno, ERES UN MAL EMPLEADO.";

        if (panelGameOver != null) panelGameOver.SetActive(true);

        Debug.Log("GAME OVER activado.");
    }

    public void Reintentar()
    {
        Time.timeScale = 1f;
        gameOverActivado = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f;
        gameOverActivado = false;
        SceneManager.LoadScene("Menú");
    }
}