using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class CubeMovement : MonoBehaviour
{
    [Header("Velocidades")]
    public float velocidadCaminar = 7f;
    public float velocidadCorrer = 10f;

    private float multiplicadorBuff = 1f;
    private Coroutine rutinaBuff;
    private CharacterController cc;

    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Verificación simple al inicio
        if (cc == null)
        {
            cc = GetComponent<CharacterController>();
            if (cc == null) return;
        }

        // Si el CharacterController está desactivado, no hacer nada
        if (!cc.enabled) return;

        // Si el juego está pausado o Time.deltaTime es 0, no mover
        if (Time.deltaTime <= 0f) return;

        // Leer inputs
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.A)) horizontal = -1f;
        if (Input.GetKey(KeyCode.D)) horizontal = 1f;
        if (Input.GetKey(KeyCode.W)) vertical = 1f;
        if (Input.GetKey(KeyCode.S)) vertical = -1f;

        // Calcular dirección de movimiento
        Vector3 direccion = new Vector3(horizontal, 0f, vertical).normalized;

        // Determinar velocidad actual
        float velocidadActual = Input.GetKey(KeyCode.LeftShift) ? velocidadCorrer : velocidadCaminar;

        // Calcular movimiento final (incluyendo buff)
        Vector3 movimiento = direccion * velocidadActual * multiplicadorBuff;

        // Aplicar gravedad constante
        movimiento.y = -9.81f;

        // Aplicar el movimiento
        cc.Move(movimiento * Time.deltaTime);
    }

    public void AplicarBuff(float multiplicador, float duracion)
    {
        if (rutinaBuff != null)
            StopCoroutine(rutinaBuff);

        rutinaBuff = StartCoroutine(RutinaBuff(multiplicador, duracion));
    }

    IEnumerator RutinaBuff(float multiplicador, float duracion)
    {
        multiplicadorBuff = multiplicador;
        yield return new WaitForSeconds(duracion);
        multiplicadorBuff = 1f;
    }
}
