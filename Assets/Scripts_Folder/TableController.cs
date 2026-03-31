using UnityEngine;
using TMPro;

public class TableController : MonoBehaviour
{
    [Header("Indicador 3D (objeto en la escena, no UI)")]
    [Tooltip("Arrastra aquí el GameObject con el componente TextMeshPro (3D, no UGUI)")]
    public TextMeshPro exclamationMark;     

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
        float pct = timeRemaining / maxWaitTime;

        if (exclamationMark != null)
        {
            if (pct > 0.5f) exclamationMark.color = colorGood;
            else if (pct > 0.2f) exclamationMark.color = colorWarning;
            else if (pct > 0f) exclamationMark.color = colorCritical;
        }

        if (timeRemaining <= 0f)
            FailOrder();
    }

    public void ReceiveNewOrder()
    {
        isWaitingForFood = true;
        timeRemaining = maxWaitTime;

        if (exclamationMark != null)
        {
            exclamationMark.color = colorGood;
            exclamationMark.gameObject.SetActive(true);
        }
    }

    public void CompleteOrder()
    {
        isWaitingForFood = false;

        if (exclamationMark != null)
            exclamationMark.gameObject.SetActive(false);

        Debug.Log("¡Pedido entregado!");
    }

    public void CancelOrder(bool penalizar = false)
    {
        isWaitingForFood = false;

        if (exclamationMark != null)
            exclamationMark.gameObject.SetActive(false);

        if (penalizar)
            SanidadManager.Instance?.RecibirDañoMental();
    }

    private void FailOrder()
    {
        isWaitingForFood = false;

        if (exclamationMark != null)
            exclamationMark.gameObject.SetActive(false);

        Debug.Log("El cliente se fue. ¡Penalización!");
        SanidadManager.Instance?.RecibirDañoMental();
    }
}