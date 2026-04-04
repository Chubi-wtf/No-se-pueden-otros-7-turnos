using UnityEngine;
using TMPro;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance;

    [Header("Progreso del Juego")]
    public TextMeshProUGUI textoProgreso;

    [Header("Turnos")]
    public int[] eventosPorTurno = { 5, 6, 7 };
    public float[] multiplicadoresTiempoPorTurno = { 1f, 0.9f, 0.8f };
    public int turnoActual = 0;
    public int eventosCompletadosEnTurno = 0;

    [Header("Panel de cambio de turno")]
    public GameObject panelCambioTurno;
    public TextMeshProUGUI textoCambioTurno;
    [TextArea(2, 5)]
    public string plantillaTurnoCompletado =
        "Bien hecho.\nTurno {turno} de {total_turnos} completado.\nPresiona {tecla} para comenzar el siguiente turno.";
    [TextArea(2, 5)]
    public string plantillaJuegoCompletado =
        "Bien hecho.\nCompletaste los {total_turnos} turnos.\nPresiona {tecla} para cerrar este panel.";
    public KeyCode teclaSiguienteTurno = KeyCode.F;

    [Header("Campana")]
    public AudioSource audioSourceCampana;
    public AudioClip sonidoCampana;

    [Header("Musica por turno")]
    public float duracionTurnoParaMusica = 90f;

    private GameObject panelActual;
    private bool esperandoInicioSiguienteTurno = false;
    private bool juegoCompletado = false;
    private float tiempoTurnoActual = 0f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (eventosPorTurno == null || eventosPorTurno.Length == 0)
            eventosPorTurno = new[] { 5, 6, 7 };

        turnoActual = Mathf.Clamp(turnoActual, 0, eventosPorTurno.Length - 1);
        eventosCompletadosEnTurno = 0;
        tiempoTurnoActual = 0f;
        ActualizarTextoProgreso();
        ActualizarMusicaTurno();

        if (panelCambioTurno != null)
            panelCambioTurno.SetActive(false);
    }

    void Update()
    {
        if (esperandoInicioSiguienteTurno)
        {
            if (Input.GetKeyDown(teclaSiguienteTurno))
                ContinuarDespuesDeTurno();

            return;
        }

        if (!juegoCompletado)
        {
            tiempoTurnoActual += Time.deltaTime;
            ActualizarMusicaTurno();
        }
    }

    public void AbrirMinijuego(GameObject pantallaMinijuego)
    {
        if (pantallaMinijuego == null) return;

        if (panelActual != null && panelActual != pantallaMinijuego)
            panelActual.SetActive(false);

        if (pantallaMinijuego.activeSelf)
            pantallaMinijuego.SetActive(false);

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
        eventosCompletadosEnTurno++;
        ActualizarTextoProgreso();

        if (eventosCompletadosEnTurno >= ObtenerEventosNecesariosTurnoActual())
            CompletarTurno();
    }

    private void CompletarTurno()
    {
        ReproducirCampana();

        if (turnoActual >= eventosPorTurno.Length - 1)
        {
            juegoCompletado = true;
            MostrarPanelCambioTurno(
                plantillaJuegoCompletado
                    .Replace("{total_turnos}", eventosPorTurno.Length.ToString())
                    .Replace("{tecla}", teclaSiguienteTurno.ToString()));
            return;
        }

        MostrarPanelCambioTurno(
            plantillaTurnoCompletado
                .Replace("{turno}", (turnoActual + 1).ToString())
                .Replace("{total_turnos}", eventosPorTurno.Length.ToString())
                .Replace("{tecla}", teclaSiguienteTurno.ToString()));
    }

    void ContinuarDespuesDeTurno()
    {
        if (juegoCompletado)
        {
            OcultarPanelCambioTurno();
            return;
        }

        turnoActual = Mathf.Clamp(turnoActual + 1, 0, eventosPorTurno.Length - 1);
        eventosCompletadosEnTurno = 0;
        tiempoTurnoActual = 0f;
        esperandoInicioSiguienteTurno = false;

        OcultarPanelCambioTurno();
        ActualizarTextoProgreso();
        ActualizarMusicaTurno();
    }

    void MostrarPanelCambioTurno(string mensaje)
    {
        esperandoInicioSiguienteTurno = true;
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (panelCambioTurno != null)
            panelCambioTurno.SetActive(true);

        if (textoCambioTurno != null)
            textoCambioTurno.text = mensaje;
    }

    void OcultarPanelCambioTurno()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (panelCambioTurno != null)
            panelCambioTurno.SetActive(false);
    }

    void ReproducirCampana()
    {
        if (audioSourceCampana != null && sonidoCampana != null)
            audioSourceCampana.PlayOneShot(sonidoCampana);
    }

    private void ActualizarTextoProgreso()
    {
        if (textoProgreso != null)
            textoProgreso.text = $"{eventosCompletadosEnTurno}/{ObtenerEventosNecesariosTurnoActual()}";
    }

    private int ObtenerEventosNecesariosTurnoActual()
    {
        if (eventosPorTurno == null || eventosPorTurno.Length == 0)
            return 5;

        int indice = Mathf.Clamp(turnoActual, 0, eventosPorTurno.Length - 1);
        return Mathf.Max(1, eventosPorTurno[indice]);
    }

    public float ObtenerMultiplicadorTiempoTurnoActual()
    {
        if (multiplicadoresTiempoPorTurno == null || multiplicadoresTiempoPorTurno.Length == 0)
            return 1f;

        int indice = Mathf.Clamp(turnoActual, 0, multiplicadoresTiempoPorTurno.Length - 1);
        return Mathf.Max(0.1f, multiplicadoresTiempoPorTurno[indice]);
    }

    void ActualizarMusicaTurno()
    {
        float duracion = Mathf.Max(1f, duracionTurnoParaMusica);
        float progreso = Mathf.Clamp01(tiempoTurnoActual / duracion);
        MusicaManager.Instance?.ActualizarProgresoTurno(progreso);
    }
}
