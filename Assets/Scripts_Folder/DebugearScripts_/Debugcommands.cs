using UnityEngine;

public class DebugCommands : MonoBehaviour
{
    [Header("Minijuego de Cocina")]
    public GameObject canvasMinijuegoCocina;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ForzarMinijuegoCocina();
        }
    }

    void ForzarMinijuegoCocina()
    {
        if (canvasMinijuegoCocina == null)
        {
 
            return;
        }

        MinigameManager.Instance?.AbrirMinijuego(canvasMinijuegoCocina);
    }
}