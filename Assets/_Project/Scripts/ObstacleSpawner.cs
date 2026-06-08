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

    // 랜덤 생성 간격
    public float minSpawnInterval = 7f;
    public float maxSpawnInterval = 15f;

    public float warningDelay = 1.2f;
    public float spawnHeight = 2f;
    public float flySpeed = 1.5f;

    public ObstacleData[] obstacles;

    private void Start()
    {
        ScheduleNextSpawn();
    }

    private void ScheduleNextSpawn()
    {
        float nextTime = Random.Range(minSpawnInterval, maxSpawnInterval);
        Invoke(nameof(StartSpawnSequence), nextTime);
    }

    private void StartSpawnSequence()
    {
        StartCoroutine(SpawnSequence());

        // 다음 장애물 생성 예약
        ScheduleNextSpawn();
    }

    private IEnumerator SpawnSequence()
    {
        ObstacleData selected = obstacles[Random.Range(0, obstacles.Length)];

        Vector3 spawnPos = GetSpawnPosition(selected);

        PlayWarningSound(selected.warningSound, spawnPos);

        yield return new WaitForSeconds(warningDelay);

        SpawnObstacle(selected, spawnPos);
    }

    private Vector3 GetSpawnPosition(ObstacleData obstacleData)
    {
        if (obstacleData.obstacleType == ObstacleType.Falling)
        {
            return target.position + new Vector3(
                Random.Range(-0.4f, 0.4f),
                spawnHeight,
                Random.Range(-0.2f, 0.2f)
            );
        }

        // 고개 방향이 아니라 ObstacleSpawner 오브젝트 방향 기준
        Vector3 behindDirection = -transform.forward;

        return target.position
            + behindDirection * 6f
            + new Vector3(
                Random.Range(-0.5f, 0.5f),
                Random.Range(0f, 0.7f),
                0f
            );
    }

    private void PlayWarningSound(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;

        GameObject soundObject = new GameObject("WarningSound");
        soundObject.transform.position = position;

        AudioSource source = soundObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.spatialBlend = 1f;
        source.minDistance = 0.2f;
        source.maxDistance = 8f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.playOnAwake = false;

        source.Play();

        Destroy(soundObject, clip.length + 0.2f);
    }

    private void SpawnObstacle(ObstacleData obstacleData, Vector3 spawnPos)
    {
        GameObject obstacle = Instantiate(
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

                Vector3 dir = (target.position - spawnPos).normalized;
                rb.linearVelocity = dir * flySpeed;
            }
        }
    }

    public void StopSpawning()
    {
        CancelInvoke();
        StopAllCoroutines();

        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");

        foreach (GameObject obstacle in obstacles)
        {
            Destroy(obstacle);
        }
    }
}