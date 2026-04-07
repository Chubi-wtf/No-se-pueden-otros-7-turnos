using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameoverManager : MonoBehaviour
{
    public static GameoverManager Instance;

    [Header("Panel de Game Over")]
    public GameObject panelGameOver;

    [Header("Imagen opcional de derrota")]
    public GameObject objetoImagenDerrota;
    public Image imagenDerrota;
    public Sprite spriteDerrota;

    [Header("Textos opcionales")]
    public TextMeshProUGUI textoTitulo;
    public TextMeshProUGUI textoSubtitulo;

    private bool gameOverActivado = false;

    GameObject ObtenerObjetoImagenDerrota()
    {
        if (objetoImagenDerrota != null)
            return objetoImagenDerrota;

        if (imagenDerrota != null)
            return imagenDerrota.gameObject;

        return null;
    }

    void Awake()
    {
        Instance = this;

        if (panelGameOver != null)
            panelGameOver.SetActive(false);

        GameObject objetoImagen = ObtenerObjetoImagenDerrota();
        if (objetoImagen != null)
            objetoImagen.SetActive(false);
    }

    public void ActivarGameOver()
    {
        if (gameOverActivado)
            return;

        gameOverActivado = true;
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (textoTitulo != null)
            textoTitulo.text = "COLAPSO MENTAL";

        if (textoSubtitulo != null)
            textoSubtitulo.text = "No pudiste aguantar el turno, ERES UN MAL EMPLEADO.";

        if (imagenDerrota != null && spriteDerrota != null)
            imagenDerrota.sprite = spriteDerrota;

        GameObject objetoImagen = ObtenerObjetoImagenDerrota();
        if (objetoImagen != null)
            objetoImagen.SetActive(true);

        if (panelGameOver != null)
            panelGameOver.SetActive(true);

        MusicaManager.Instance?.ReproducirMusicaDerrota();
    }

    public void Reintentar()
    {
        Time.timeScale = 1f;
        gameOverActivado = false;
        MusicaManager.Instance?.ReproducirMusicaJuego();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f;
        gameOverActivado = false;
        MusicaManager.Instance?.ReproducirMusicaJuego();
        SceneManager.LoadScene("Menú");
    }
}
