using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// 마른 천 도구.
/// 창문이 Rinsed 상태일 때 문지르면 최종 Clean 상태로 전환.
/// Sponge와 구조는 같지만 호출하는 Window 메서드가 다름.
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class DryCloth : MonoBehaviour
{
    [Header("Wipe Settings")]
    [SerializeField] private float progressPerMeter = 1.5f;
    [SerializeField] private float minVelocityThreshold = 0.02f;

    [Header("VFX")]
    [SerializeField] private ParticleSystem wipeVFX;

    private XRGrabInteractable _grab;
    private bool _isGrabbed;
    private Window _currentWindow;
    private Vector3 _prevPosition;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        _grab = GetComponent<XRGrabInteractable>();
        _grab.selectEntered.AddListener(_ =>
        {
            _isGrabbed = true;
            _prevPosition = transform.position;
        });
        _grab.selectExited.AddListener(_ =>
        {
            _isGrabbed = false;
            _currentWindow = null;
            wipeVFX?.Stop();
        });
    }

    private void Update()
    {
        if (!_isGrabbed || _currentWindow == null) return;

        float moved = (transform.position - _prevPosition).magnitude;
        _prevPosition = transform.position;

        float velocity = moved / Time.deltaTime;
        if (velocity >= minVelocityThreshold)
        {
            _currentWindow.AddWipeProgress(progressPerMeter * moved);

            if (wipeVFX != null && !wipeVFX.isPlaying) wipeVFX.Play();
        }
        else
        {
            wipeVFX?.Stop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isGrabbed) return;
        Window w = other.GetComponent<Window>();
        if (w != null) _currentWindow = w;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Window>() == _currentWindow)
        {
            _currentWindow = null;
            wipeVFX?.Stop();
        }
    }
}
