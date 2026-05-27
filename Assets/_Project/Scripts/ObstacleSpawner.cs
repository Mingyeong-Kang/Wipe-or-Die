using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public Transform target; // Main Camera
    public float spawnInterval = 2f;
    public float spawnHeight = 2f;

    private void Start()
    {
        InvokeRepeating(nameof(SpawnObstacle), 1f, spawnInterval);
    }

    private void SpawnObstacle()
    {
        Vector3 spawnPos = target.position + new Vector3(
            Random.Range(-0.4f, 0.4f),
            spawnHeight,
            Random.Range(-0.2f, 0.2f)
        );

        GameObject obstacle = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);

        Rigidbody rb = obstacle.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
        }
    }
}