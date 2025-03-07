using UnityEngine;

public class PlatformController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float boundary = 3.5f; // Movement limit

    void Update()
    {
        // Get player input (arrow keys or A/D keys)
        float horizontalInput = Input.GetAxis("Horizontal");

        // Calculate new position with movement constraints
        Vector3 newPosition = transform.position + Vector3.right * horizontalInput * moveSpeed * Time.deltaTime;
        newPosition.x = Mathf.Clamp(newPosition.x, -boundary, boundary); // Restrict movement within boundaries

        // Apply movement to platform
        transform.position = newPosition;

        // Move all objects in the stack together
        FallingObject.MoveStack(transform.position);
    }
}
