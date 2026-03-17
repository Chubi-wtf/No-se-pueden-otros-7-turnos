using UnityEngine;
[RequireComponent(typeof(CharacterController))]
public class CubeMovement : MonoBehaviour
{
    [Header("Velocidades")]
    public float velocidadCaminar = 5f;
    public float velocidadCorrer = 10f;

    private CharacterController cc;

    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        float h = 0f;
        float v = 0f;

        if (Input.GetKey(KeyCode.A)) h = -1f;
        if (Input.GetKey(KeyCode.D)) h = 1f;
        if (Input.GetKey(KeyCode.W)) v = 1f;
        if (Input.GetKey(KeyCode.S)) v = -1f;

        Vector3 dir = new Vector3(h, 0f, v).normalized;

        float speed = Input.GetKey(KeyCode.LeftShift) ? velocidadCorrer : velocidadCaminar;

        cc.Move(dir * speed * Time.deltaTime);

        cc.Move(Vector3.down * 9.8f * Time.deltaTime);
    }
}