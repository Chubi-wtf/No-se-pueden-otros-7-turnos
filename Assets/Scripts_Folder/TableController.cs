using UnityEngine;
using TMPro;

public class TableController : MonoBehaviour
{
    [Header("Referencias Visuales")]
    public TextMeshProUGUI exclamationMark;

    [Header("Ajustes de Tiempo")]
    public float maxWaitTime = 15f;
    private float timeRemaining;
    public bool isWaitingForFood = false;

    [Header("Colores de Estrés")]
    public Color colorGood = Color.green;
    public Color colorWarning = Color.yellow;
    public Color colorCritical = Color.red;

    void Start()
    {
        if (exclamationMark != null)
            exclamationMark.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isWaitingForFood) return;

        timeRemaining -= Time.deltaTime;
        float timePercentage = timeRemaining / maxWaitTime;

        if (timePercentage > 0.5f)
            exclamationMark.color = colorGood;
        else if (timePercentage > 0.2f)
            exclamationMark.color = colorWarning;
        else if (timePercentage > 0f)
            exclamationMark.color = colorCritical;
        else
            FailOrder();
    }

    public void ReceiveNewOrder()
    {
        isWaitingForFood = true;
        timeRemaining = maxWaitTime;
        exclamationMark.color = colorGood;
        exclamationMark.gameObject.SetActive(true);
    }

    public void CompleteOrder()
    {
        isWaitingForFood = false;
        exclamationMark.gameObject.SetActive(false);
        Debug.Log("¡Pedido entregado!");
    }

    private void FailOrder()
    {
        isWaitingForFood = false;
        exclamationMark.gameObject.SetActive(false);
        Debug.Log("El cliente se fue. ¡Penalización!");

        // Ahora sí penaliza la sanidad
        if (SanidadManager.Instance != null)
        {
            SanidadManager.Instance.RecibirDañoMental();
        }
    }
}