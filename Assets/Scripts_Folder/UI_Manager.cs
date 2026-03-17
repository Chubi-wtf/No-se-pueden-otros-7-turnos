using TMPro;
using UnityEngine;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager instance;
    [Header("Ui Referencias")]
    public GameObject AlertPanel;
    public TextMeshProUGUI AlertText;

    void awake()
    {
        instance = this;
    }

    public void MostrarAlerta(string Mensaje)
    {
        AlertText.text = Mensaje;
        AlertPanel.SetActive(true);
    }

    public void cerrarPanel()
    { 
        AlertPanel.SetActive(false); 
    }
}
