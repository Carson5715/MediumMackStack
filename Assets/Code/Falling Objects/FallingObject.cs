using UnityEngine;
using System.Collections.Generic;

public class FallingObject : MonoBehaviour
{
    private bool snapped = false;
    private static List<GameObject> stack = new List<GameObject>();  // Keeps track of all snapped objects
    private Rigidbody rb;

    // Store the initial Y and Z positions from the collision
    private float originalYPosition;
    private float originalZPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        // Only handle collisions if the object hasn't already snapped
        if (!snapped && collision.gameObject.CompareTag("FallingObject"))
        {
            snapped = true;

            // Add this object to the stack
            stack.Add(this.gameObject);

            // Mark this object as kinematic to stop it from falling after it snaps
            rb.isKinematic = true;

            // Store the original Y and Z positions when the object collided
            originalYPosition = transform.position.y;
            originalZPosition = transform.position.z;
        }
    }

    // This method is called by the player controller to move all stacked objects together
    public static void MoveStack(Vector3 platePosition)
    {
        if (stack.Count > 0)
        {
            // Move all the objects in the stack together based on the plate's X position
            foreach (var obj in stack)
            {
                if (obj != null)
                {
                    // Only update the X position, keeping the Y and Z positions the same as when the object collided
                    obj.transform.position = new Vector3(platePosition.x, obj.GetComponent<FallingObject>().originalYPosition, obj.GetComponent<FallingObject>().originalZPosition);
                }
            }
        }
    }
}
