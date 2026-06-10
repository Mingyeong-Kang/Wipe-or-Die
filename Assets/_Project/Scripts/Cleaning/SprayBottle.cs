using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// 세정제 스프레이 도구.
/// XRGrabInteractable의 Activate(트리거) 이벤트로 창문에 세정제 분사.
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class SprayBottle : MonoBehaviour
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
            StopSpray();
        });
        _grab.activated.AddListener(_ => _isActivated = true);
        _grab.deactivated.AddListener(_ =>
        {
            _isActivated = false;
            StopSpray();
        });
    }

    private void Update()
    {
        if (!_isGrabbed || !_isActivated) return;

        // 분사 VFX 유지
        if (sprayVFX != null && !sprayVFX.isPlaying)
        {
            sprayVFX.Play();
            if (audioSource != null && sprayClip != null)
                audioSource.PlayOneShot(sprayClip);
        }

        // 노즐 앞 범위 내 창문 탐색 (windowLayer 미설정 시 전체 레이어 fallback)
        Transform origin = nozzle != null ? nozzle : transform;
        Collider[] hits = windowLayer == 0
            ? Physics.OverlapSphere(origin.position, sprayRadius)
            : Physics.OverlapSphere(origin.position, sprayRadius, windowLayer);
        foreach (var hit in hits)
        {
            Window window = hit.GetComponent<Window>();
            if (window != null && window.TryApplySpray())
                break;
        }
    }

    private void StopSpray()
    {
        sprayVFX?.Stop();
    }

    private void OnDrawGizmosSelected()
    {
        // 에디터에서 분사 범위 시각화
        Gizmos.color = Color.cyan;
        Transform origin = nozzle != null ? nozzle : transform;
        Gizmos.DrawWireSphere(origin.position, sprayRadius);
    }
}
