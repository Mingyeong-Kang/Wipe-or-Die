using System.Collections;
using UnityEngine;

[System.Serializable]
public class ObstacleData
{
    public GameObject obstaclePrefab;
    public AudioClip warningSound;
    public ObstacleType obstacleType;
}

public enum ObstacleType
{
    Falling,
    Flying
}

public class ObstacleSpawner : MonoBehaviour
{
    public Transform target;
    public float spawnInterval = 3f;
    public float warningDelay = 0.8f;
    public float spawnHeight = 2f;
    public float flySpeed = 3f;

    public AudioSource audioSource;
    public ObstacleData[] obstacles;

    private void Start()
    {
        InvokeRepeating(nameof(StartSpawnSequence), 1f, spawnInterval);
    }

    private void StartSpawnSequence()
    {
        StartCoroutine(SpawnSequence());
    }

    private IEnumerator SpawnSequence()
    {
        ObstacleData selected =
            obstacles[Random.Range(0, obstacles.Length)];

        if (audioSource != null && selected.warningSound != null)
        {
            audioSource.PlayOneShot(selected.warningSound);
        }

        yield return new WaitForSeconds(warningDelay);

        SpawnObstacle(selected);
    }

    private void SpawnObstacle(ObstacleData obstacleData)
    {
        Vector3 spawnPos = target.position;

        if (obstacleData.obstacleType == ObstacleType.Falling)
        {
            // 화분: 머리 위에서 낙하
            spawnPos += new Vector3(
                Random.Range(-0.4f, 0.4f),
                spawnHeight,
                Random.Range(-0.2f, 0.2f)
            );
        }
        else
        {
            // 새/드론: 플레이어 뒤에서 생성
            Vector3 behindDirection = -target.forward;

            spawnPos = target.position
                + behindDirection * 2.5f
                + new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(0f, 0.7f),
                    0f
                );
        }

        GameObject obstacle =
            Instantiate(
                obstacleData.obstaclePrefab,
                spawnPos,
                Quaternion.identity
            );

        Destroy(obstacle, 5f);

        Rigidbody rb = obstacle.GetComponent<Rigidbody>();

        if (rb != null)
        {
            if (obstacleData.obstacleType == ObstacleType.Falling)
            {
                rb.useGravity = true;
                rb.linearVelocity = Vector3.zero;
            }
            else
            {
                rb.useGravity = false;

                Vector3 dir =
                    (target.position - spawnPos).normalized;

                rb.linearVelocity = dir * flySpeed;
            }
        }
    }
}