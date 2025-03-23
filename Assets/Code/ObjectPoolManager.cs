using UnityEngine;
using System.Collections.Generic;

public class ObjectPoolManager : MonoBehaviour
{
    // Assign your object prefabs in the inspector.
    public GameObject[] objectPool;

    // List to track all objects currently in the pool.
    private List<GameObject> pooledObjects = new List<GameObject>();

    void Start()
    {
        // Initialize the pool by creating a set number of objects for each type.
        // For example, instantiate 3 of each prefab.
        foreach (var obj in objectPool)
        {
            for (int i = 0; i < 3; i++)
            {
                GameObject newObj = Instantiate(obj);
                newObj.SetActive(false); // Set all objects to inactive initially.
                pooledObjects.Add(newObj);
            }
        }
    }

    // Spawns an object from the pool at the given position.
    public GameObject SpawnObject(Vector3 spawnPosition)
    {
        // Randomly choose one of the prefab types.
        int randomIndex = Random.Range(0, objectPool.Length);
        GameObject prefab = objectPool[randomIndex];

        // Attempt to get an inactive object of this specific type.
        GameObject objectToSpawn = GetPooledObjectOfType(prefab);
        if (objectToSpawn != null)
        {
            objectToSpawn.transform.position = spawnPosition;
            objectToSpawn.SetActive(true);  // Activate the object.
        }
        else
        {
            // If no inactive object is available for this type, instantiate a new one.
            objectToSpawn = Instantiate(prefab, spawnPosition, Quaternion.identity);
            pooledObjects.Add(objectToSpawn);
        }

        return objectToSpawn;
    }

    // Retrieves an inactive object from the pool that matches the given prefab type.
    private GameObject GetPooledObjectOfType(GameObject prefab)
    {
        // Since instantiated objects have names like "PrefabName (Clone)", we check if their name contains the prefab's name.
        foreach (GameObject obj in pooledObjects)
        {
            if (!obj.activeInHierarchy && obj.name.Contains(prefab.name))
            {
                return obj;
            }
        }
        return null; // Return null if no inactive object of the desired type is found.
    }

    // Returns an object to the pool (disables it).
    public void ReturnObjectToPool(GameObject obj)
    {
        obj.SetActive(false);
    }
}