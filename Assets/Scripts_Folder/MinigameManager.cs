using UnityEngine;
using TMPro;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance;

    [Header("Progreso del Juego")]
    public TextMeshProUGUI textoProgreso;
    public int minijuegosNecesarios = 5;
    private int minijuegosCompletados = 0;

    private GameObject panelActual;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ActualizarTextoProgreso();
    }

    public void AbrirMinijuego(GameObject pantallaMinijuego)
    {
        panelActual = pantallaMinijuego;
        Time.timeScale = 0f;
        pantallaMinijuego.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CerrarMinijuego(bool completado = false)
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (panelActual != null)
        {
            panelActual.SetActive(false);
            panelActual = null;
        }

        if (completado)
        {
            SanidadManager.Instance?.RecuperarSanidad();
            RegistrarMinijuegoCompletado();
        }
        else
        {
            SanidadManager.Instance?.RecibirDañoMental();
        }
    }

    public void CerrarMinijuegoPausa()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (panelActual != null)
        {
            panelActual.SetActive(false);
            panelActual = null;
        }
    }

    private void RegistrarMinijuegoCompletado()
    {
        minijuegosCompletados++;
        ActualizarTextoProgreso();
        Debug.Log($"Minijuegos completados: {minijuegosCompletados}/{minijuegosNecesarios}");

        if (minijuegosCompletados >= minijuegosNecesarios)
            JuegoCompletado();
    }

    private void ActualizarTextoProgreso()
    {
        if (textoProgreso != null)
            textoProgreso.text = $"{minijuegosCompletados}/{minijuegosNecesarios}";
    }

    private void JuegoCompletado()
    {
        Time.timeScale = 0f;
        Debug.Log("JUEGO COMPLETADO!");
    }
}