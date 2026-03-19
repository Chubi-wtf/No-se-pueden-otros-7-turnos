using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LimpiarMinijuego : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image manchaImagen; 
    public TextMeshProUGUI textoTemporizador;

    [Header("Ajustes del Minijuego")]
    public float suciedadTotal = 100f;
    public float multiplicadorFrotar = 150f; 
    public float tiempoMaximo = 5f; 

    private float suciedadActual;
    private float tiempoRestante;
    private bool minijuegoActivo = false;

    
    void OnEnable()
    {
        suciedadActual = suciedadTotal;
        tiempoRestante = tiempoMaximo;
        minijuegoActivo = true;

        
        if (manchaImagen != null)
        {
            Color c = manchaImagen.color;
            c.a = 1f;
            manchaImagen.color = c;
        }
    }

    void Update()
    {
        if (!minijuegoActivo) return;

        tiempoRestante -= Time.unscaledDeltaTime;
        textoTemporizador.text = tiempoRestante.ToString("F1") + "s"; 

        if (tiempoRestante <= 0)
        {
            PerderMinijuego();
            return;
        }

        if (Input.GetMouseButton(0))
        {
            float movimientoY = Mathf.Abs(Input.GetAxis("Mouse Y"));

            suciedadActual -= movimientoY * multiplicadorFrotar * Time.unscaledDeltaTime;

            float porcentaje = suciedadActual / suciedadTotal;
            Color c = manchaImagen.color;
            c.a = porcentaje;
            manchaImagen.color = c;

            // 3.  de Victoria
            if (suciedadActual <= 0)
            {
                GanarMinijuego();
            }
        }
    }

    void GanarMinijuego()
    {
        minijuegoActivo = false;
        Debug.Log("¡Mesa limpia a tiempo!");
        MinigameManager.Instance.CerrarMinijuego(this.gameObject);
    }

    void PerderMinijuego()
    {
        minijuegoActivo = false;
        Debug.Log("¡Tiempo agotado! La mesa sigue sucia.");
        MinigameManager.Instance.CerrarMinijuego(this.gameObject);

    }
}