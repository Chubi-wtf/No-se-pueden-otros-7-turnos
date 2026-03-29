using UnityEngine;
using UnityEngine.UI;

public class BandejaHud : MonoBehaviour
{
    [Header("UI")]
    public Slider sliderBandeja;

    [Header("Fisica de la bandeja")]
    public float gravedad = 0.9f;
    public float fuerzaCorrecion = 2.0f;
    public float umbralCaida = 1f;

    [Header("Penalizacion")]
    public float dañoPorCaida = 10f;
    public float tiempoEntrepenalizaciones = 3f;
    private float timerPenalizacion = 0f;

    private float balance = 0f;

    void Start()
    {
        balance = Random.Range(-0.1f, 0.1f);
        if (sliderBandeja != null)
        {
            sliderBandeja.minValue = -1f;
            sliderBandeja.maxValue = 1f;
            sliderBandeja.value = balance;
        }
    }

    void Update()
    {
        float direccion = Mathf.Sign(balance);
        if (balance == 0f) direccion = Random.Range(0, 2) == 0 ? -1f : 1f;

        float velocidadCaida = gravedad + Mathf.Abs(balance) * gravedad;
        balance += direccion * velocidadCaida * Time.deltaTime;

        if (Input.GetKey(KeyCode.Q))
            balance -= fuerzaCorrecion * Time.deltaTime;
        else if (Input.GetKey(KeyCode.E))
            balance += fuerzaCorrecion * Time.deltaTime;

        balance = Mathf.Clamp(balance, -1f, 1f);

        if (sliderBandeja != null)
            sliderBandeja.value = balance;

        if (Mathf.Abs(balance) >= umbralCaida)
        {
            timerPenalizacion -= Time.deltaTime;
            if (timerPenalizacion <= 0f)
            {
                timerPenalizacion = tiempoEntrepenalizaciones;

                if (SanidadManager.Instance != null)
                    SanidadManager.Instance.RecibirDañoPersonalizado(dañoPorCaida);

                Debug.Log("Bandeja caida! -10 sanidad");
                balance = Random.Range(-0.15f, 0.15f);
            }
        }
        else
        {
            timerPenalizacion = 0f;
        }
    }
}