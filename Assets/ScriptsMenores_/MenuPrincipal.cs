using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    [Header("Nombre exacto de la escena del juego")]
    public string EscenaJuego = "GameScene";

    public void BotonPlay()
    {
        SceneManager.LoadScene(EscenaJuego);
    }
}