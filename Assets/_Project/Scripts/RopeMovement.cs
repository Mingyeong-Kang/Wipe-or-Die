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
    [SerializeField] private float rangeHalfWidth = 4f; // 건물 중심 기준 ±범위(m)

    private XROrigin _xrOrigin;
    private float _centerX = 0f; // 레벨 시작 시 SetCenter로 갱신

    private void Awake()
    {
        _xrOrigin = GetComponent<XROrigin>();
        if (_xrOrigin == null)
            _xrOrigin = FindFirstObjectByType<XROrigin>();
        _centerX = _xrOrigin != null ? _xrOrigin.transform.position.x : 0f;
    }

    /// LevelManager가 텔레포트 후 호출
    public void SetCenter(float centerX) => _centerX = centerX;

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

        // 건물 중심 기준 범위 제한
        newPos.x = Mathf.Clamp(newPos.x, _centerX - rangeHalfWidth, _centerX + rangeHalfWidth);
        _xrOrigin.transform.position = newPos;
    }
}
