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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!snapped && (collision.gameObject.CompareTag("FallingObject") || collision.gameObject.CompareTag("Plate")))
        {
            // Use the first contact point to verify the collision is from above.
            ContactPoint contact = collision.contacts[0];
            if (contact.normal.y <= 0.5f)
                return;

            snapped = true;
            stack.Add(gameObject);
            rb.isKinematic = true;
            
            // Save the horizontal (x and z) offset relative to the plate.
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
        // Use the current wobble offset from PlateController.
        float wobbleOffset = PlateController.Instance != null ? PlateController.Instance.currentWobble.x : 0f;
        foreach (GameObject obj in stack)
        {
            if (obj != null)
            {
                FallingObject fo = obj.GetComponent<FallingObject>();
                Vector3 pos = obj.transform.position;
                // Update the horizontal position: plate position + the object's original offset + the global wobble.
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

    // Lose condition: turn off kinematic on all stacked objects so physics takes over.
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
