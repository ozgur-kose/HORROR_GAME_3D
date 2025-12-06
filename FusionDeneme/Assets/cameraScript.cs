using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("Hareket Hýzý")]
    public float moveSpeed = 5f;

    void Update()
    {
        // Tuþ girdilerini al
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.W)) vertical += 1f;
        if (Input.GetKey(KeyCode.S)) vertical -= 1f;
        if (Input.GetKey(KeyCode.A)) horizontal -= 1f;
        if (Input.GetKey(KeyCode.D)) horizontal += 1f;

        // Hareket yönünü belirle
        Vector3 move = transform.forward * vertical + transform.right * horizontal;

        // Hareketi uygula
        transform.position += move * moveSpeed * Time.deltaTime;
    }
}

