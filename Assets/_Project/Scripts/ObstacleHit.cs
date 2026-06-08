using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class ObstacleHit : MonoBehaviour
{
    public int damage = 10;

    public float hapticAmplitude = 0.5f;
    public float hapticDuration = 0.15f;

    public GameObject hitEffectPrefab;
    public AudioClip hitSound;

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
            HapticImpulsePlayer haptic =
                other.GetComponentInParent<HapticImpulsePlayer>();

            if (haptic != null)
                haptic.SendHapticImpulse(hapticAmplitude, hapticDuration);

            if (hitEffectPrefab != null)
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

            if (hitSound != null)
                AudioSource.PlayClipAtPoint(hitSound, transform.position);

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