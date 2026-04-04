using UnityEngine;

public class controladorVistaJugador : MonoBehaviour
{
    [Header("Referencias")]
    public Transform cuerpoJugador;

    [Header("Sensibilidad")]
    public float sensibilidadX = 180f;
    public float sensibilidadY = 180f;

    [Header("Modo 2.5D")]
    public bool permitirRotacionHorizontal = false;
    public bool rotarCuerpoJugador = false;

    [Header("Limites verticales")]
    public float anguloMinimo = -80f;
    public float anguloMaximo = 80f;

    [Header("Cursor")]
    public bool bloquearCursorAlIniciar = true;

    private float rotacionVertical = 0f;

    void Start()
    {
        Vector3 angulos = transform.localEulerAngles;
        rotacionVertical = angulos.x;

        if (rotacionVertical > 180f)
            rotacionVertical -= 360f;

        if (bloquearCursorAlIniciar)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensibilidadX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadY * Time.deltaTime;

        rotacionVertical -= mouseY;
        rotacionVertical = Mathf.Clamp(rotacionVertical, anguloMinimo, anguloMaximo);

        transform.localRotation = Quaternion.Euler(rotacionVertical, 0f, 0f);

        if (!permitirRotacionHorizontal) return;

        if (rotarCuerpoJugador)
        {
            if (cuerpoJugador != null)
                cuerpoJugador.Rotate(Vector3.up * mouseX);
            else
                transform.parent?.Rotate(Vector3.up * mouseX);
        }
        else
        {
            transform.Rotate(Vector3.up * mouseX, Space.Self);
        }
    }
}
