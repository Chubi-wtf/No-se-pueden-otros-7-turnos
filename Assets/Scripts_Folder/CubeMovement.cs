using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class CubeMovement : MonoBehaviour
{
    [Header("Velocidades")]
    public float velocidadCaminar = 7f;
    public float velocidadCorrer = 10f;

    // ── buff de velocidad ─────────────────────────────────────────────────────
    private float multiplicadorBuff = 1f;
    private Coroutine rutinaBuff;

    private CharacterController cc;

    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        float h = 0f, v = 0f;

        if (Input.GetKey(KeyCode.A)) h = -1f;
        if (Input.GetKey(KeyCode.D)) h = 1f;
        if (Input.GetKey(KeyCode.W)) v = 1f;
        if (Input.GetKey(KeyCode.S)) v = -1f;

        Vector3 dir = new Vector3(h, 0f, v).normalized;
        float speed = Input.GetKey(KeyCode.LeftShift) ? velocidadCorrer : velocidadCaminar;

        cc.Move(dir * speed * multiplicadorBuff * Time.deltaTime);
        cc.Move(Vector3.down * 9.8f * Time.deltaTime);
    }

    /// <summary>
    /// Llamado por LimpiarMinijuego cuando el jugador gana el evento buff.
    /// </summary>
    public void AplicarBuff(float multiplicador, float duracion)
    {
        if (rutinaBuff != null) StopCoroutine(rutinaBuff);
        rutinaBuff = StartCoroutine(RutinaBuff(multiplicador, duracion));
    }

    IEnumerator RutinaBuff(float multiplicador, float duracion)
    {
        multiplicadorBuff = multiplicador;
        Debug.Log($"Buff activo x{multiplicador} por {duracion}s");
        yield return new WaitForSeconds(duracion);
        multiplicadorBuff = 1f;
        Debug.Log("Buff expirado.");
    }
}