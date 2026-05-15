using UnityEngine;
using CustomMath;

public class BSPCameraController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Camera")]
    public float lookSpeed = 2f;

    private float yaw = 0f;

    void Start()
    {
        yaw = transform.eulerAngles.y;
    }

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vec3 move = new Vec3(moveX, 0, moveZ) * moveSpeed * Time.deltaTime;

        transform.Translate(move, Space.Self);

        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X");
            yaw += mouseX * lookSpeed;

            transform.eulerAngles = new Vec3(transform.eulerAngles.x, yaw, transform.eulerAngles.z);
        }
    }
}