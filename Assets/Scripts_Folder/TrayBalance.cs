using UnityEngine;
using UnityEngine.UI;

public class TrayBalance : MonoBehaviour
{
    [Header("UI Reference")]
    public Slider balanceBar;

    [Header("Balance Settings")]
    public float currentBalance = 0f; // -1 (Izquierda) a 1 (Derecha)

    // Qué tan rápido acelera la caída mientras más lejos esté del centro
    public float gravityMultiplier = 1.5f;

    // Una velocidad mínima para que nunca se quede trabada en el 0 exacto
    public float baseTiltSpeed = 0.1f;

    // Qué tan fuerte corrige el jugador con Q y E
    public float recoverSpeed = 1.2f;

    void Start()
    {
        // Le damos un micro-empujón inicial aleatorio para que empiece a caer
        float initialDirection = Random.Range(0, 2) == 0 ? -1f : 1f;
        currentBalance = 0.05f * initialDirection;

        balanceBar.value = currentBalance;
    }

    void Update()
    {
        // 1. Calculamos hacia dónde está cayendo (1 para derecha, -1 para izquierda)
        float currentDirection = Mathf.Sign(currentBalance);

        // 2. Calculamos la velocidad de caída: velocidad base + (peso por inercia)
        // Usamos Mathf.Abs para tener el valor positivo de la inclinación
        float fallSpeed = baseTiltSpeed + (Mathf.Abs(currentBalance) * gravityMultiplier);

        // 3. Aplicamos la caída en la dirección correspondiente
        currentBalance += currentDirection * fallSpeed * Time.deltaTime;

        // 4. El jugador presiona Q y E para intentar estabilizar la bandeja
        if (Input.GetKey(KeyCode.Q))
        {
            // Q empuja el peso hacia la izquierda (negativo)
            currentBalance -= recoverSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            // E empuja el peso hacia la derecha (positivo)
            currentBalance += recoverSpeed * Time.deltaTime;
        }

        // 5. Limitamos el valor entre -1 y 1 para que no se rompa la UI
        currentBalance = Mathf.Clamp(currentBalance, -1f, 1f);
        balanceBar.value = currentBalance;

        // 6. Comprobar si se cayó por completo
        if (currentBalance >= 1f || currentBalance <= -1f)
        {
            DropTray();
        }
    }

    void DropTray()
    {
        Debug.Log("¡PUM! Se cayó la bandeja.");

        // Aquí puedes reiniciar el minijuego con un nuevo empujón
        float randomDirection = Random.Range(0, 2) == 0 ? -1f : 1f;
        currentBalance = 0.05f * randomDirection;
    }
}