using UnityEngine;
using System.Collections.Generic;

public class FallingObject : MonoBehaviour
{
    private bool snapped = false;
    // Keeps track of all snapped objects (the stack)
    private static List<GameObject> stack = new List<GameObject>();  
    private Rigidbody rb;
    
    // Stores the horizontal (x and z) offset relative to the plate at the moment of collision.
    public Vector3 originalOffset;

    // Public tolerance for collision height (only collisions within this distance from the highest point are accepted).
    public float collisionTolerance = 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        // Only handle collisions with falling objects or the plate.
        if (!snapped && (collision.gameObject.CompareTag("FallingObject") || collision.gameObject.CompareTag("Plate")))
        {
            // Use the first contact point to check that the collision comes from above.
            ContactPoint contact = collision.contacts[0];
            if (contact.normal.y <= 0.5f)
                return;

            // If there is at least one snapped object, only allow collisions at the top.
            if (GetStackCount() > 0)
            {
                float highestPoint = GetHighestPoint();
                if (collision.gameObject.CompareTag("FallingObject"))
                {
                    // Accept collision only if the contact's y is within tolerance of the highest point.
                    if (contact.point.y < highestPoint - collisionTolerance)
                        return;
                }
                else if (collision.gameObject.CompareTag("Plate"))
                {
                    // If the stack exists, ignore collisions with the plate.
                    return;
                }
            }
            
            // Accept collision: snap this object.
            snapped = true;
            stack.Add(gameObject);
            rb.isKinematic = true;
            
            // Save the horizontal offset relative to the plate.
            GameObject plate = GameObject.FindGameObjectWithTag("Plate");
            if (plate != null)
            {
                originalOffset = new Vector3(
                    transform.position.x - plate.transform.position.x,
                    0,
                    transform.position.z - plate.transform.position.z
                );
            }
            else
            {
                originalOffset = Vector3.zero;
            }
        }
    }

    public static void MoveStack(Vector3 platePosition)
    {
        float wobbleOffset = PlateController.Instance != null ? PlateController.Instance.currentWobble.x : 0f;
        foreach (GameObject obj in stack)
        {
            if (obj != null)
            {
                FallingObject fo = obj.GetComponent<FallingObject>();
                Vector3 pos = obj.transform.position;
                // Update horizontal position: plate position + object's original offset + wobble.
                pos.x = platePosition.x + fo.originalOffset.x + wobbleOffset;
                pos.z = platePosition.z + fo.originalOffset.z;
                obj.transform.position = pos;
            }
        }
    }

    public static int GetStackCount()
    {
        return stack.Count;
    }

    // Returns the highest point of the stack (based on each object's top using its collider bounds).
    public static float GetHighestPoint()
    {
        float highest = float.MinValue;
        foreach (GameObject obj in stack)
        {
            if (obj != null)
            {
                Collider col = obj.GetComponent<Collider>();
                float top = obj.transform.position.y;
                if (col != null)
                {
                    top += col.bounds.extents.y;
                }
                if (top > highest)
                {
                    highest = top;
                }
            }
        }
        return highest;
    }

    // Returns the total off-center offset (sum of the absolute x offsets) for all snapped objects.
    public static float GetTotalOffset()
    {
        float totalOffset = 0f;
        foreach (GameObject obj in stack)
        {
            if (obj != null)
            {
                FallingObject fo = obj.GetComponent<FallingObject>();
                totalOffset += Mathf.Abs(fo.originalOffset.x);
            }
        }
        return totalOffset;
    }

    // Lose condition: turn off kinematics for all snapped objects.
    public static void ReleaseStack()
    {
        foreach (GameObject obj in stack)
        {
            if (obj != null)
            {
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                }
            }
        }
    }
}
