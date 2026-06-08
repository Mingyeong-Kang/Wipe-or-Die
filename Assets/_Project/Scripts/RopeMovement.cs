using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;

/// <summary>
/// 로프 좌우 이동 스크립트.
/// 왼쪽 조이스틱 X축으로 플레이어를 좌우로만 이동.
/// XR Origin의 Move는 비활성화하고 이 스크립트 사용.
/// </summary>
public class RopeMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float minX = -5f;
    [SerializeField] private float maxX = 5f;

    private XROrigin _xrOrigin;

    private void Awake()
    {
        _xrOrigin = GetComponent<XROrigin>();
        if (_xrOrigin == null)
            _xrOrigin = FindFirstObjectByType<XROrigin>();
    }

    private void Update()
    {
        // 왼쪽 컨트롤러 조이스틱 X축 읽기
        InputDevice leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 joystick);

        float horizontal = joystick.x;
        if (Mathf.Abs(horizontal) < 0.1f) return; // 데드존

        // X축으로만 이동
        Vector3 move = new Vector3(horizontal * moveSpeed * Time.deltaTime, 0f, 0f);
        Vector3 newPos = _xrOrigin.transform.position + move;

        // 범위 제한
        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        _xrOrigin.transform.position = newPos;
    }
}
