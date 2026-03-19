using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    public string EscenaJuego = "GameScene";

    public void BotonPlay()
    {
        SceneManager.LoadScene(EscenaJuego);
    }
}