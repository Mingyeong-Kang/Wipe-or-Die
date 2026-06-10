using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// 물 분사 도구.
/// 창문이 Sponged 상태일 때 트리거를 누르면 헹굼(Rinsed)으로 전환.
/// SprayBottle과 구조는 같지만 호출하는 Window 메서드가 다름.
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class WaterSpray : MonoBehaviour
{
    [Header("Spray Settings")]
    [SerializeField] private Transform nozzle;
    [SerializeField] private float sprayRadius = 0.4f;
    [SerializeField] private LayerMask windowLayer;

    [Header("VFX")]
    [SerializeField] private ParticleSystem sprayVFX;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sprayClip;

    private XRGrabInteractable _grab;
    private bool _isGrabbed;
    private bool _isActivated;

    private void Awake()
    {
        _grab = GetComponent<XRGrabInteractable>();
        _grab.selectEntered.AddListener(_ => _isGrabbed = true);
        _grab.selectExited.AddListener(_ =>
        {
            _isGrabbed = false;
            _isActivated = false;
            sprayVFX?.Stop();
        });
        _grab.activated.AddListener(_ => _isActivated = true);
        _grab.deactivated.AddListener(_ =>
        {
            _isActivated = false;
            sprayVFX?.Stop();
        });
    }

    private void Update()
    {
        if (!_isGrabbed || !_isActivated) return;

        if (sprayVFX != null && !sprayVFX.isPlaying)
        {
            sprayVFX.Play();
            if (audioSource != null && sprayClip != null)
                audioSource.PlayOneShot(sprayClip);
        }

        Transform origin = nozzle != null ? nozzle : transform;
        Collider[] hits = windowLayer == 0
            ? Physics.OverlapSphere(origin.position, sprayRadius)
            : Physics.OverlapSphere(origin.position, sprayRadius, windowLayer);
        foreach (var hit in hits)
        {
            Window window = hit.GetComponent<Window>();
            if (window != null && window.TryApplyWater())
                break;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Transform origin = nozzle != null ? nozzle : transform;
        Gizmos.DrawWireSphere(origin.position, sprayRadius);
    }
}
