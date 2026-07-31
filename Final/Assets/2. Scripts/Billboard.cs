using UnityEngine;

public class Billboard : MonoBehaviour
{
    [Tooltip("빌보드가 바라볼 대상 카메라 (미지정 시 Camera.main 자동 할당)")]
    public Camera targetCamera;

    [Tooltip("true일 경우 카메라의 회전값만 복사, false일 경우 LookAt 방식")]
    public bool useCameraRotation = true;

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    void LateUpdate()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null) return;
        }

        if (useCameraRotation)
        {
            transform.rotation = targetCamera.transform.rotation;
        }
        else
        {
            Vector3 targetPosition = transform.position + targetCamera.transform.rotation * Vector3.forward;
            Vector3 targetUp = targetCamera.transform.rotation * Vector3.up;
            transform.LookAt(targetPosition, targetUp);
        }
    }
}
