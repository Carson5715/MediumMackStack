using UnityEngine;

public class Spawner : MonoBehaviour
{
    // Reference to the pool manager
    public ObjectPoolManager poolManager;

    // How often to spawn an object (in seconds)
    public float spawnInterval = 2.0f;

    // Range along the x-axis for random spawn positions
    public float xRange = 3.25f;

    // Y-coordinate for the spawn position (e.g., top of the screen)
    public float spawnY = 10f;

    void Start()
    {
        // Call SpawnObject repeatedly at the given interval.
        InvokeRepeating("SpawnObject", 0f, spawnInterval);
    }

    void SpawnObject()
    {
        // Determine a random spawn position.
        Vector3 spawnPosition = new Vector3(Random.Range(-xRange, xRange), spawnY, 0);

        // Call the correct method to spawn an object from the pool
        poolManager.SpawnObject(spawnPosition);  // Use SpawnObject instead of SpawnRandomObject
    }
}
