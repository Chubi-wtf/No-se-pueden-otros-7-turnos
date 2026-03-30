using TMPro;
using UnityEngine;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager instance;

    [Header("UI Referencias")]
    public GameObject AlertPanel;
    public TextMeshProUGUI AlertText;
    public TextMeshProUGUI InstruccionText; 

    void Awake() { instance = this; }

    public void MostrarAlerta(string titulo, string instruccion = "")
    {
        AlertText.text = titulo;

        if (InstruccionText != null)
        {
            InstruccionText.text = instruccion;
            InstruccionText.gameObject.SetActive(instruccion != "");
        }

        AlertPanel.SetActive(true);
    }

    public void cerrarPanel()
    {
        AlertPanel.SetActive(false);
        if (InstruccionText != null) InstruccionText.gameObject.SetActive(false);
    }
}