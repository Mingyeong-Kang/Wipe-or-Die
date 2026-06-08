using UnityEngine;

/// <summary>
/// Canvas가 카메라를 항상 따라다니게 하는 스크립트.
/// Canvas에 붙이고 Cam에 Main Camera 연결.
/// </summary>
public class FollowCamera : MonoBehaviour
{
    public Transform cam;
    public Vector3 offset = new Vector3(0, 0, 2f);

    void LateUpdate()
    {
        transform.position = cam.position + cam.TransformDirection(offset);
        transform.rotation = cam.rotation;
    }
}
