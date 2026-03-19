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

    [Header("Zona segura para ganar")]
    public float safeZone = 0.25f;
    public float tiempoRequeridoEnZona = 1.5f;
    private float tiempoEnZonaSegura = 0f;

    private bool minijuegoActivo = false;

    void OnEnable()
    {
        float initialDirection = Random.Range(0, 2) == 0 ? -1f : 1f;
        currentBalance = 0.05f * initialDirection;
        balanceBar.value = currentBalance;
        tiempoEnZonaSegura = 0f;
        minijuegoActivo = true;
    }

    void Update()
    {
        if (!minijuegoActivo) return;

        float currentDirection = Mathf.Sign(currentBalance);
        float fallSpeed = baseTiltSpeed + (Mathf.Abs(currentBalance) * gravityMultiplier);
        currentBalance += currentDirection * fallSpeed * Time.unscaledDeltaTime;

        if (Input.GetKey(KeyCode.Q))
            currentBalance -= recoverSpeed * Time.unscaledDeltaTime;
        else if (Input.GetKey(KeyCode.E))
            currentBalance += recoverSpeed * Time.unscaledDeltaTime;

        currentBalance = Mathf.Clamp(currentBalance, -1f, 1f);
        balanceBar.value = currentBalance;

        if (currentBalance >= 1f || currentBalance <= -1f)
        {
            DropTray();
            return;
        }

        if (Mathf.Abs(currentBalance) <= safeZone)
        {
            tiempoEnZonaSegura += Time.unscaledDeltaTime;
            if (tiempoEnZonaSegura >= tiempoRequeridoEnZona)
                CompleteTray();
        }
        else
        {
            tiempoEnZonaSegura = 0f;
        }
    }

    void CompleteTray()
    {
        minijuegoActivo = false;
        Debug.Log("Bandeja equilibrada!");
        MinigameManager.Instance?.CerrarMinijuego(true);
    }

    void DropTray()
    {
        minijuegoActivo = false;
        Debug.Log("Bandeja caida!");
        SanidadManager.Instance?.RecibirDañoMental();
        MinigameManager.Instance?.CerrarMinijuego(false);
    }
}