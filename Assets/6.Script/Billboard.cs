using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Camera mainCamera; // 카메라 지정 안하면 자동으로 Camera.main 사용

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        if (mainCamera == null) return;

        // 카메라를 바라보도록 회전
        Vector3 lookPos = mainCamera.transform.position - transform.position;
        lookPos.y = 0; // 수직 회전 무시 (선택사항)
        if (lookPos.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(lookPos);
    }
}