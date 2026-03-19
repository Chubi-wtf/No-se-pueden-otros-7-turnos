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

    [Header("Velocidad de transicion del pitch")]
    public float velocidadPitch = 2f;   

    private AudioSource audioSource;
    private float pitchObjetivo;

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
        pitchObjetivo = pitchMaximo;
    }

    void Start()
    {
        if (musicaJuego != null)
            audioSource.Play();
    }

    void Update()
    {
        audioSource.pitch = Mathf.Lerp(audioSource.pitch, pitchObjetivo, velocidadPitch * Time.deltaTime);
    }

    
    public void ActualizarPitch(float porcentajeCordura)
    {
        pitchObjetivo = Mathf.Lerp(pitchMinimo, pitchMaximo, porcentajeCordura);
    }
}