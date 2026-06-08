using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 로프 이동 시스템.
/// 플레이어(XR Origin)를 건물 외벽 평면 위에서 상하좌우로만 이동시킴.
/// 깊이(Z) 이동 없음 — 항상 벽면에 붙어 있는 느낌.
/// </summary>
public class RopeMovement : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionProperty moveInput; // XRI Default: Left Joystick

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.5f;

    [Header("Wall Bounds (이동 가능 범위)")]
    [SerializeField] private float minX = -4f;
    [SerializeField] private float maxX = 4f;
    [SerializeField] private float minY = 0f;
    [SerializeField] private float maxY = 6f;

    [Header("Difficulty Scaling")]
    [SerializeField] private float speedIncreasePerMinute = 0.1f;
    private float _elapsedTime;

    private bool _movementEnabled = true;

    public void SetMovementEnabled(bool enabled) => _movementEnabled = enabled;

    private void Update()
    {
        if (!_movementEnabled) return;
        if (GameManager.Instance != null && GameManager.Instance.State != GameManager.GameState.Playing) return;

        _elapsedTime += Time.deltaTime;
        float currentSpeed = moveSpeed + speedIncreasePerMinute * (_elapsedTime / 60f);

        Vector2 input = moveInput.action?.ReadValue<Vector2>() ?? Vector2.zero;
        if (input.sqrMagnitude < 0.01f) return;

        Vector3 delta = new Vector3(input.x, input.y, 0f) * currentSpeed * Time.deltaTime;
        Vector3 next = transform.position + delta;

        next.x = Mathf.Clamp(next.x, minX, maxX);
        next.y = Mathf.Clamp(next.y, minY, maxY);
        // Z 고정 — 벽면에서 이탈 방지
        next.z = transform.position.z;

        transform.position = next;
    }

    private void OnDrawGizmosSelected()
    {
        // 에디터에서 이동 가능 구역 시각화
        Gizmos.color = new Color(0f, 0.8f, 1f, 0.3f);
        Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, transform.position.z);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, 0.1f);
        Gizmos.DrawCube(center, size);
    }
}
