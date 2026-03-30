using UnityEngine;
using TMPro; 
using UnityEngine.UI;

public class NarrativeIntro : MonoBehaviour
{
    [Header("Referencias UI")]
    [Tooltip("El GameObject del Panel que contiene el texto de historia")]
    public GameObject panelIntroduccion;

    [Tooltip("El botón para cerrar este panel y empezar a jugar")]
    public Button botonEmpezarTurno;

    [Header("Textos (Opcional - Puedes escribirlos en el editor)")]
  
    [TextArea(3, 10)]
    public string tituloHistoria = "TURNO INFERNAL - DÍA 1";
    [TextArea(5, 15)]
    public string cuerpoHistoria = "Hoy es viernes, 9:00 PM. El gerente desapareció misteriosamente, el cocinero principal está colapsado y los clientes llevan 20 minutos esperando mesa.\n\n" +
                                     "Eres el único mesero de turno. La cocina depende de que tú tomes los pedidos bien, la limpieza depende de ti, y el servicio de café también.\n\n" +
                                     "<color=red>¡Hazte cargo de TODO antes de que el restaurante explote!</color>";

    [Header("Referencias a TextMeshPro (Asignar si usas los textos de arriba)")]
    public TextMeshProUGUI txtTitulo;
    public TextMeshProUGUI txtCuerpo;

    void Start()
    {
        if (panelIntroduccion != null)
        {
            panelIntroduccion.SetActive(true);

            if (txtTitulo != null) txtTitulo.text = tituloHistoria;
            if (txtCuerpo != null) txtCuerpo.text = cuerpoHistoria;
        }

        Time.timeScale = 0f;

      
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (botonEmpezarTurno != null)
        {
            botonEmpezarTurno.onClick.AddListener(EmpezarGamePlay);
        }
    }

    public void EmpezarGamePlay()
    {
        if (panelIntroduccion != null)
        {
            panelIntroduccion.SetActive(false);
        }

        Time.timeScale = 1f;

      
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("¡A trabajar, mesero agobiado!");
    }
}