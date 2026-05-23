using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public float spawnInterval = 2f;

    private void Start()
    {
        InvokeRepeating(nameof(SpawnObstacle), 1f, spawnInterval);
    }

    private void SpawnObstacle()
    {
        Vector3 randomOffset = new Vector3(
            Random.Range(-0.5f, 0.5f),
            Random.Range(-0.3f, 0.5f),
            0
        );

        Instantiate(obstaclePrefab, transform.position + randomOffset, Quaternion.identity);
    }
}