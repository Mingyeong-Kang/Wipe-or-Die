using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// 스펀지 도구.
/// 잡고 창문에 닿은 상태에서 손을 움직이면 세정 진행도가 쌓임.
/// 창문이 Sprayed 상태일 때만 동작.
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class Sponge : MonoBehaviour
{
    [Header("Rubbing Settings")]
    [SerializeField] private float progressPerMeter = 1.5f;
    [SerializeField] private float minVelocityThreshold = 0.02f;

    [Header("VFX")]
    [SerializeField] private ParticleSystem rubVFX;

    private XRGrabInteractable _grab;
    private bool _isGrabbed;
    private Window _currentWindow;
    private Vector3 _prevPosition;

    private void Awake()
    {
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
            rubVFX?.Stop();
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
            _currentWindow.AddSpongeProgress(progressPerMeter * moved);

            if (rubVFX != null && !rubVFX.isPlaying) rubVFX.Play();
        }
        else
        {
            rubVFX?.Stop();
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
            rubVFX?.Stop();
        }
    }
}
