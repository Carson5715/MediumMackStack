using UnityEngine;

public class PlateController : MonoBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        // Move the plate left and right with arrow keys
        float horizontalInput = Input.GetAxis("Horizontal"); // Left and right arrow keys or A/D keys

        // Move the plate accordingly
        Vector3 newPosition = transform.position + Vector3.right * horizontalInput * moveSpeed * Time.deltaTime;
        transform.position = newPosition;

        // Move all the objects in the stack together
        FallingObject.MoveStack(transform.position);
    }
}
