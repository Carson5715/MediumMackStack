using UnityEngine;
using System.Collections.Generic;

public class FallingObject : MonoBehaviour
{
    private bool snapped = false;
    // List tracking all snapped objects.
    private static List<GameObject> stack = new List<GameObject>();  
    private Rigidbody rb;
    
    // Horizontal offset relative to the plate at collision time.
    public Vector3 originalOffset;
    
    // Public tolerance for collision height.
    public float collisionTolerance = 0.1f;
    
    // Public variable: Factor (0 to 1) defining how close to the center on the X axis the contact must be.
    // For example, 0.5 means the contact point must be within 50% of the collider's half-width from the center.
    public float contactMarginThresholdFactor = 0.5f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (!snapped && (collision.gameObject.CompareTag("FallingObject") || collision.gameObject.CompareTag("Plate")))
        {
            ContactPoint contact = collision.contacts[0];
            if (contact.normal.y <= 0.5f)
                return;
            
            // Check if the contact is too far from the object's center on the X axis.
            Collider myCollider = GetComponent<Collider>();
            if (myCollider != null)
            {
                float centerX = myCollider.bounds.center.x;
                float halfWidth = myCollider.bounds.extents.x;
                if (Mathf.Abs(contact.point.x - centerX) > halfWidth * contactMarginThresholdFactor)
                {
                    return; // Ignore collision if the contact is outside the acceptable central region.
                }
            }
            
            // If there are already snapped objects, only accept collisions at the top.
            if (GetStackCount() > 0)
            {
                float highestPoint = GetHighestPoint();
                if (collision.gameObject.CompareTag("FallingObject"))
                {
                    if (contact.point.y < highestPoint - collisionTolerance)
                        return;
                }
                else if (collision.gameObject.CompareTag("Plate"))
                {
                    return;
                }
            }
            
            snapped = true;
            stack.Add(gameObject);
            rb.isKinematic = true;
            
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
                    top += col.bounds.extents.y;
                if (top > highest)
                    highest = top;
            }
        }
        return highest;
    }
    
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
    
    public static void ReleaseStack()
    {
        foreach (GameObject obj in stack)
        {
            if (obj != null)
            {
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.isKinematic = false;
            }
        }
    }
    
    // New method: Teleport the entire stack to a given target position.
    public static void TeleportStack(Vector3 targetPosition)
    {
        // Compute the center of the current stack.
        Vector3 center = Vector3.zero;
        int count = 0;
        foreach (GameObject obj in stack)
        {
            if (obj != null)
            {
                center += obj.transform.position;
                count++;
            }
        }
        if (count > 0)
            center /= count;
        
        // For each object, preserve its relative offset from the center.
        foreach (GameObject obj in stack)
        {
            if (obj != null)
            {
                Vector3 relativePos = obj.transform.position - center;
                obj.transform.position = targetPosition + relativePos;
            }
        }
    }
}