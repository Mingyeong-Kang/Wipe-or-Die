using UnityEngine;

public class ObstacleHit : MonoBehaviour
{
    public int damage = 10;

    private void Update()
    {
        if (transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name.Contains("Controller"))
        {
            Destroy(gameObject);
            return;
        }

        PlayerHealth player = other.GetComponent<PlayerHealth>();

        if (player != null)
        {
            player.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}