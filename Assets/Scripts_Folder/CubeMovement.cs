using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class CubeMovement : MonoBehaviour
{
    [Header("Velocidades")]
    public float velocidadCaminar = 7f;
    public float velocidadCorrer = 10f;

    [Header("Animacion 2D por sprites")]
    public SpriteRenderer spriteRendererJugador;
    public Sprite frameIdle;
    public Sprite[] framesCaminar;
    public Sprite[] framesCaminarArriba;
    public float framesPorSegundo = 10f;
    public bool voltearSegunDireccion = true;

    private float multiplicadorBuff = 1f;
    private Coroutine rutinaBuff;
    private CharacterController cc;
    private float temporizadorAnimacion = 0f;
    private int indiceFrameActual = 0;
    private bool estabaMoviendose = false;
    private Vector3 ultimaDireccionMovimiento = Vector3.zero;
    private bool ultimoEstadoMovimiento = false;

    void Start()
    {
        cc = GetComponent<CharacterController>();

        ActualizarSpriteIdle();
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
        bool estaMoviendose = direccion.sqrMagnitude > 0.0001f;
        ultimaDireccionMovimiento = direccion;
        ultimoEstadoMovimiento = estaMoviendose;

        // Determinar velocidad actual
        float velocidadActual = Input.GetKey(KeyCode.LeftShift) ? velocidadCorrer : velocidadCaminar;

        // Calcular movimiento final (incluyendo buff)
        Vector3 movimiento = direccion * velocidadActual * multiplicadorBuff;

        // Aplicar gravedad constante
        movimiento.y = -9.81f;

        // Aplicar el movimiento
        cc.Move(movimiento * Time.deltaTime);
    }

    void LateUpdate()
    {
        if (Time.deltaTime <= 0f)
        {
            ActualizarAnimacion(Vector3.zero, false);
            return;
        }

        ActualizarAnimacion(ultimaDireccionMovimiento, ultimoEstadoMovimiento);
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

    void ActualizarAnimacion(Vector3 direccion, bool estaMoviendose)
    {
        if (spriteRendererJugador == null)
            return;

        bool moviendoHaciaArriba = Input.GetKey(KeyCode.W)
                                   && !Input.GetKey(KeyCode.S)
                                   && Mathf.Abs(direccion.z) >= Mathf.Abs(direccion.x);
        Sprite[] framesActivos = moviendoHaciaArriba &&
                                 framesCaminarArriba != null &&
                                 framesCaminarArriba.Length > 0
            ? framesCaminarArriba
            : framesCaminar;

        if (voltearSegunDireccion)
        {
            if (direccion.x < -0.01f)
                spriteRendererJugador.flipX = true;
            else if (direccion.x > 0.01f)
                spriteRendererJugador.flipX = false;
        }

        if (!estaMoviendose)
        {
            temporizadorAnimacion = 0f;
            indiceFrameActual = 0;

            if (estabaMoviendose)
                ActualizarSpriteIdle();

            estabaMoviendose = false;
            return;
        }

        estabaMoviendose = true;

        if (framesActivos == null || framesActivos.Length == 0)
        {
            ActualizarSpriteIdle();
            return;
        }

        temporizadorAnimacion += Time.deltaTime;
        float duracionFrame = framesPorSegundo > 0f ? 1f / framesPorSegundo : 0.1f;

        while (temporizadorAnimacion >= duracionFrame)
        {
            temporizadorAnimacion -= duracionFrame;
            indiceFrameActual = (indiceFrameActual + 1) % framesActivos.Length;
        }

        if (framesActivos[indiceFrameActual] != null)
            spriteRendererJugador.sprite = framesActivos[indiceFrameActual];
    }

    void ActualizarSpriteIdle()
    {
        if (spriteRendererJugador == null)
            return;

        if (frameIdle != null)
        {
            spriteRendererJugador.sprite = frameIdle;
            return;
        }

        if (framesCaminar != null && framesCaminar.Length > 0 && framesCaminar[0] != null)
            spriteRendererJugador.sprite = framesCaminar[0];
    }
}
