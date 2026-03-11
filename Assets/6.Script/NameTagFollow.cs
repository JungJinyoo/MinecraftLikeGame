using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NameTagFollow : MonoBehaviour
{

    public Transform target;        // 플레이어 본체 (steve rig)
    public Vector3 offset = new Vector3(0, 2f, 0);        // (플레이어 기준) 이름 뜨는 위치
    public Text nameText;

    Camera cam;

    void Start()
    {
        cam = Camera.main;

        if (target == null && transform.parent != null)
        {
            target = transform.parent;
        }
    }

    void LateUpdate()
    {
        if (cam == null) cam = Camera.main;
        if (target == null || cam == null) return;


        // 머리 위 이름 위치
        transform.position = target.position + offset;

        // 항상 카메라를 바라보게
        transform.LookAt(
            transform.position + cam.transform.rotation * Vector3.forward,
            cam.transform.rotation * Vector3.up);
    }

    public void SetName(string nickname)
    {
        if (nameText != null)
        {
            nameText.text = nickname;
        }
    }
}
