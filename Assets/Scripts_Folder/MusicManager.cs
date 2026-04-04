using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicaManager : MonoBehaviour
{
    public static MusicaManager Instance;

    [Header("Audio")]
    public AudioClip musicaJuego;

    [Header("Pitch segun cordura")]
    public float pitchMaximo = 1.0f;
    public float pitchMinimo = 0.6f;

    [Header("Pitch adicional por turno")]
    public float pitchExtraMinimoTurno = 0f;
    public float pitchExtraMaximoTurno = 0.3f;

    [Header("Velocidad de transicion del pitch")]
    public float velocidadPitch = 2f;

    private AudioSource audioSource;
    private float pitchBaseCordura;
    private float pitchExtraTurno;

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

        pitchBaseCordura = pitchMaximo;
        pitchExtraTurno = 0f;
    }

    void Start()
    {
        if (musicaJuego != null)
            audioSource.Play();
    }

    void Update()
    {
        float pitchObjetivo = Mathf.Clamp(
            pitchBaseCordura + pitchExtraTurno,
            pitchMinimo,
            pitchMaximo + pitchExtraMaximoTurno);

        audioSource.pitch = Mathf.Lerp(audioSource.pitch, pitchObjetivo, velocidadPitch * Time.deltaTime);
    }

    public void ActualizarPitch(float porcentajeCordura)
    {
        pitchBaseCordura = Mathf.Lerp(pitchMinimo, pitchMaximo, porcentajeCordura);
    }

    public void ActualizarProgresoTurno(float progreso)
    {
        pitchExtraTurno = Mathf.Lerp(
            pitchExtraMinimoTurno,
            pitchExtraMaximoTurno,
            Mathf.Clamp01(progreso));
    }
}
