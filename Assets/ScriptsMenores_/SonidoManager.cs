using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SonidoManager : MonoBehaviour
{
    public static SonidoManager Instance;

    [Header("Sonidos de feedback")]
    public AudioClip sonidoAcierto;    
    public AudioClip sonidoFallo;      

    [Header("Degradación del fallo")]
    [Tooltip("Cada fallo consecutivo baja el pitch este valor")]
    public float degradacionPitch = 0.08f;
    [Tooltip("Pitch mínimo al que puede llegar el sonido de fallo")]
    public float pitchMinFallo = 0.4f;
    [Tooltip("Segundos sin fallar para que el pitch se recupere")]
    public float tiempoRecuperacion = 4f;

    private AudioSource src;
    private int fallosConsecutivos = 0;
    private float timerRecuperacion = 0f;

    void Awake()
    {
        Instance = this;
        src = GetComponent<AudioSource>();
        src.playOnAwake = false;
    }

    void Update()
    {
        if (fallosConsecutivos > 0)
        {
            timerRecuperacion += Time.deltaTime;
            if (timerRecuperacion >= tiempoRecuperacion)
            {
                fallosConsecutivos = Mathf.Max(0, fallosConsecutivos - 1);
                timerRecuperacion = 0f;
            }
        }
    }


    public void Acierto()
    {
        fallosConsecutivos = 0;
        timerRecuperacion = 0f;

        if (sonidoAcierto == null) return;
        src.pitch = 1f;
        src.volume = 1f;
        src.PlayOneShot(sonidoAcierto);
    }

    public void Fallo()
    {
        fallosConsecutivos++;
        timerRecuperacion = 0f;

        if (sonidoFallo == null) return;

        float pitchActual = Mathf.Max(
            pitchMinFallo,
            1f - (fallosConsecutivos - 1) * degradacionPitch
        );

        float volumenActual = Mathf.Min(1f, 0.8f + fallosConsecutivos * 0.05f);

        src.pitch = pitchActual;
        src.volume = volumenActual;
        src.PlayOneShot(sonidoFallo);

        Debug.Log($"Fallo #{fallosConsecutivos} — pitch: {pitchActual:F2}");
    }
}