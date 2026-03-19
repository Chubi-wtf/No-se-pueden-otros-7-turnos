using UnityEngine;
using UnityEngine.UI;

public class TrayBalance : MonoBehaviour
{
    [Header("UI Reference")]
    public Slider balanceBar;

    [Header("Balance Settings")]
    public float currentBalance = 0f;
    public float gravityMultiplier = 1.5f;
    public float baseTiltSpeed = 0.1f;
    public float recoverSpeed = 1.2f;

    private bool minijuegoActivo = false;

    void OnEnable()
    {
        float initialDirection = Random.Range(0, 2) == 0 ? -1f : 1f;
        currentBalance = 0.05f * initialDirection;
        balanceBar.value = currentBalance;
        minijuegoActivo = true;
    }

    void Update()
    {
        if (!minijuegoActivo) return;

        float currentDirection = Mathf.Sign(currentBalance);

        float fallSpeed = baseTiltSpeed + (Mathf.Abs(currentBalance) * gravityMultiplier);

        currentBalance += currentDirection * fallSpeed * Time.unscaledDeltaTime;

        if (Input.GetKey(KeyCode.Q))
        {
            currentBalance -= recoverSpeed * Time.unscaledDeltaTime;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            currentBalance += recoverSpeed * Time.unscaledDeltaTime;
        }

        currentBalance = Mathf.Clamp(currentBalance, -1f, 1f);
        balanceBar.value = currentBalance;

        if (currentBalance >= 1f || currentBalance <= -1f)
        {
            DropTray();
        }
    }

    void DropTray()
    {
        minijuegoActivo = false;

        if (SanidadManager.Instance != null)
        {
            SanidadManager.Instance.RecibirDañoMental();
        }

        if (MinigameManager.Instance != null)
        {
            MinigameManager.Instance.CerrarMinijuego(this.gameObject);
        }
    }
}