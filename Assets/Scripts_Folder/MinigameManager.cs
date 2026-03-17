using UnityEngine;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance;

    void Awake()
    {
        Instance = this;
    }

    public void AbrirMinijuego(GameObject pantallaMinijuego)
    {
        Time.timeScale = 0f;
        pantallaMinijuego.SetActive(true);
    }

    public void CerrarMinijuego(GameObject pantallaMinijuego)
    {
        Time.timeScale = 1f;
        pantallaMinijuego.SetActive(false);
    }
}