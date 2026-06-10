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

    public float minSpawnInterval = 7f;
    public float maxSpawnInterval = 15f;

    public float warningDelay = 1.2f;
    public float spawnHeight = 2f;
    public float flySpeed = 1.5f;

    public ObstacleData[] obstacles;

    [Header("Spatial Warning Sound")]
    public float soundLerpRatio = 0.6f;
    public float soundVolume = 1f;
    public float soundMinDistance = 3f;
    public float soundMaxDistance = 40f;

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
        ScheduleNextSpawn();
    }

    private IEnumerator SpawnSequence()
    {
        if (target == null) yield break;
        if (obstacles == null || obstacles.Length == 0) yield break;

        ObstacleData selected = obstacles[Random.Range(0, obstacles.Length)];
        if (selected.obstaclePrefab == null) yield break;

        Vector3 spawnPos = GetSpawnPosition(selected);

        // 장애물 위치 그대로가 아니라, 플레이어와 장애물 사이에서 소리 재생
        // 방향감은 살리고, 너무 멀어서 안 들리는 문제를 줄임
        Vector3 soundPos = Vector3.Lerp(target.position, spawnPos, soundLerpRatio);
        PlayWarningSound(selected.warningSound, soundPos);

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
        source.volume = soundVolume;

        // VR 공간감 유지
        source.spatialBlend = 1f;
        source.minDistance = soundMinDistance;
        source.maxDistance = soundMaxDistance;
        source.rolloffMode = AudioRolloffMode.Logarithmic;

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

        GameObject[] spawnedObstacles = GameObject.FindGameObjectsWithTag("Obstacle");

        foreach (GameObject obstacle in spawnedObstacles)
        {
            Destroy(obstacle);
        }
    }
}