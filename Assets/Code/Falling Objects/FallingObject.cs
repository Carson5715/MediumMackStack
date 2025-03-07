using UnityEngine;
using System.Collections.Generic;

public class FallingObject : MonoBehaviour
{
    private bool snapped = false;
    // Keeps track of all snapped objects (the stack)
    private static List<GameObject> stack = new List<GameObject>();  
    private Rigidbody rb;
    
    // Stores the horizontal (x and z) offset relative to the plate so the object moves with the stack.
    private Vector3 originalOffset;

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
            
            // Adjust only the vertical (y) position based on the collision point.
            Collider myCollider = GetComponent<Collider>();
            if (myCollider != null)
            {
                Vector3 pos = transform.position;
                pos.y = contact.point.y + myCollider.bounds.extents.y;
                transform.position = pos;
            }
            
            stack.Add(gameObject);
            rb.isKinematic = true;
            
            // Save only the horizontal (x and z) offset relative to the plate.
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
        foreach (GameObject obj in stack)
        {
            if (obj != null)
            {
                FallingObject fo = obj.GetComponent<FallingObject>();
                Vector3 pos = obj.transform.position;
                // Update only the horizontal coordinates relative to the plate.
                pos.x = platePosition.x + fo.originalOffset.x;
                pos.z = platePosition.z + fo.originalOffset.z;
                obj.transform.position = pos;
            }
        }
    }
}
