using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicaManager : MonoBehaviour
{
    public static MusicaManager Instance;

    [Header("Audio")]
    public AudioClip musicaJuego;
    public AudioClip musicaDerrota;

    [Header("Pitch segun cordura")]
    public float pitchMaximo = 1.0f;
    public float pitchMinimo = 0.6f;

    [Header("Pitch adicional por nivel")]
    public float pitchExtraPorNivel1 = 0f;
    public float pitchExtraPorNivel2 = 0.08f;
    public float pitchExtraPorNivel3 = 0.16f;

    [Header("Pitch adicional por progreso del turno")]
    public float pitchExtraMinimoTurno = 0f;
    public float pitchExtraMaximoTurno = 0.3f;

    [Header("Velocidad de transicion del pitch")]
    public float velocidadPitch = 2f;

    private AudioSource audioSource;
    private float porcentajeCordura = 1f;
    private float pitchExtraNivel = 0f;
    private float pitchExtraTurno = 0f;
    private bool reproduciendoMusicaDerrota = false;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.clip = musicaJuego;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        ReproducirMusicaJuego();
    }

    void Update()
    {
        if (reproduciendoMusicaDerrota)
        {
            if (audioSource != null)
                audioSource.pitch = Mathf.Lerp(audioSource.pitch, 1f, velocidadPitch * Time.unscaledDeltaTime);
            return;
        }

        float pitchMinNivel = pitchMinimo + pitchExtraNivel;
        float pitchMaxNivel = pitchMaximo + pitchExtraNivel;

        float pitchPorCordura = Mathf.Lerp(
            pitchMinNivel,
            pitchMaxNivel,
            Mathf.Clamp01(porcentajeCordura));

        float pitchObjetivo = Mathf.Clamp(
            pitchPorCordura + pitchExtraTurno,
            pitchMinimo,
            pitchMaximo + pitchExtraPorNivel3 + pitchExtraMaximoTurno);

        audioSource.pitch = Mathf.Lerp(
            audioSource.pitch,
            pitchObjetivo,
            velocidadPitch * Time.unscaledDeltaTime);
    }

    public void ActualizarPitch(float nuevoPorcentajeCordura)
    {
        porcentajeCordura = Mathf.Clamp01(nuevoPorcentajeCordura);
    }

    public void ActualizarProgresoTurno(float progreso)
    {
        pitchExtraTurno = Mathf.Lerp(
            pitchExtraMinimoTurno,
            pitchExtraMaximoTurno,
            Mathf.Clamp01(progreso));
    }

    public void ActualizarNivelTurno(int turnoActual)
    {
        switch (turnoActual)
        {
            case 0:
                pitchExtraNivel = pitchExtraPorNivel1;
                break;
            case 1:
                pitchExtraNivel = pitchExtraPorNivel2;
                break;
            default:
                pitchExtraNivel = pitchExtraPorNivel3;
                break;
        }

        if (!reproduciendoMusicaDerrota && audioSource != null && musicaJuego != null && !audioSource.isPlaying)
            audioSource.Play();
    }

    public void ReproducirMusicaJuego()
    {
        reproduciendoMusicaDerrota = false;

        if (audioSource == null)
            return;

        if (musicaJuego == null)
        {
            audioSource.Stop();
            return;
        }

        if (audioSource.clip != musicaJuego)
            audioSource.clip = musicaJuego;

        audioSource.loop = true;
        audioSource.pitch = 1f;

        if (!audioSource.isPlaying)
            audioSource.Play();
    }

    public void ReproducirMusicaDerrota()
    {
        reproduciendoMusicaDerrota = true;

        if (audioSource == null)
            return;

        if (musicaDerrota == null)
        {
            return;
        }

        if (audioSource.clip != musicaDerrota)
            audioSource.clip = musicaDerrota;

        audioSource.loop = true;
        audioSource.pitch = 1f;
        audioSource.Play();
    }
}
