using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// 도구 홀스터 시스템.
/// 플레이어 주변 4개 앵커에 청소 도구를 고정.
/// 도구를 잡으면 앵커에서 분리, 근처에 놓으면 자동 복귀.
/// </summary>
public class ToolHolder : MonoBehaviour
{
    [System.Serializable]
    public class ToolSlot
    {
        public Transform anchor;
        public XRGrabInteractable tool;
        [HideInInspector] public bool isGrabbed;
    }

    [SerializeField] private ToolSlot[] slots;
    [SerializeField] private float snapBackDistance = 0.4f;

    private void Start()
    {
        foreach (var slot in slots)
        {
            if (slot.tool == null) continue;

            var captured = slot;
            captured.tool.selectEntered.AddListener(_ =>
            {
                captured.isGrabbed = true;
                if (captured.tool.TryGetComponent<Rigidbody>(out var rb))
                    rb.isKinematic = false;
            });
            captured.tool.selectExited.AddListener(_ =>
            {
                captured.isGrabbed = false;
            });
        }
    }

    private void Update()
    {
        foreach (var slot in slots)
        {
            if (slot.tool == null || slot.anchor == null) continue;
            if (slot.isGrabbed) continue;

            // 잡고 있지 않을 때 앵커 위치로 부드럽게 복귀
            float dist = Vector3.Distance(slot.tool.transform.position, slot.anchor.position);
            if (dist < snapBackDistance)
            {
                slot.tool.transform.position = Vector3.Lerp(
                    slot.tool.transform.position, slot.anchor.position, Time.deltaTime * 8f);
                slot.tool.transform.rotation = Quaternion.Lerp(
                    slot.tool.transform.rotation, slot.anchor.rotation, Time.deltaTime * 8f);
            }
        }
    }
}
