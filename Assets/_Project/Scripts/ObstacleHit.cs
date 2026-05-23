using UnityEngine;

public class ObstacleHit : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.name.Contains("Controller"))
        {
            Destroy(gameObject);
        }
    }
}
