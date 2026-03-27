using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("El nombre exacto de la escena de tu juego")]
    public string nombreEscenaJuego = "GameScene";

    public void Jugar()
    {
       
        Time.timeScale = 1f;

        SceneManager.LoadScene(nombreEscenaJuego);
    }

    public void CerrarJuego()
    {

        Application.Quit();
    }
}