using UnityEngine;
using System.Collections.Generic;

public class ObjectPoolManager : MonoBehaviour
{
    // Assign your object prefabs in the inspector.
    public GameObject[] objectPool;

    // List to track all objects currently in the pool
    private List<GameObject> pooledObjects = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the pool by creating a set number of objects
        foreach (var obj in objectPool)
        {
            GameObject newObj = Instantiate(obj);
            newObj.SetActive(false); // Set all objects to inactive initially
            pooledObjects.Add(newObj);
        }
    }

    // Spawns an object from the pool at the given position
    public GameObject SpawnObject(Vector3 spawnPosition)
    {
        GameObject objectToSpawn = GetPooledObject();
        if (objectToSpawn != null)
        {
            objectToSpawn.transform.position = spawnPosition;
            objectToSpawn.SetActive(true);  // Activate the object and set its position
        }
        else
        {
            // If no pooled object is available, instantiate a new one
            int index = Random.Range(0, objectPool.Length);
            objectToSpawn = Instantiate(objectPool[index], spawnPosition, Quaternion.identity);
            pooledObjects.Add(objectToSpawn); // Add it to the pool for future use
        }

        return objectToSpawn;
    }

    // Retrieves an inactive object from the pool
    private GameObject GetPooledObject()
    {
        foreach (GameObject obj in pooledObjects)
        {
            if (!obj.activeInHierarchy) // Check if object is inactive
            {
                return obj;
            }
        }

        return null; // Return null if no inactive object is found
    }

    // Returns an object to the pool (disables it)
    public void ReturnObjectToPool(GameObject obj)
    {
        obj.SetActive(false);
    }
}
