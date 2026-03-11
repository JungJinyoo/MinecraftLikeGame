using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2PlayerCtrl : MonoBehaviour {

    public float speed;      // 캐릭터 움직임 스피드.
    public float jumpSpeed; // 캐릭터 점프 힘.
    public float gravity;    // 캐릭터에게 작용하는 중력.

    private CharacterController controller; // 현재 캐릭터가 가지고있는 캐릭터 컨트롤러 콜라이더.
    private Vector3 MoveDir;                // 캐릭터의 움직이는 방향.
    private Vector3 HeadDir;

    private GameObject CharCam;

    void Awake()
    {
        CharCam = GameObject.FindGameObjectWithTag("MainCamera");
    }
    void Start()
    {
        speed = 10.0f;
        jumpSpeed = 8.0f;
        gravity = 20.0f;

        MoveDir = Vector3.zero;
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // 현재 캐릭터가 땅에 있는가?
        if (controller.isGrounded)
        {
            // 위, 아래 움직임 셋팅. 
            MoveDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            // 벡터를 로컬 좌표계 기준에서 월드 좌표계 기준으로 변환한다.
            MoveDir = transform.TransformDirection(MoveDir);

            // 스피드 증가.
            MoveDir *= speed;

            // 캐릭터 점프
            if (Input.GetButton("Jump"))
                MoveDir.y = jumpSpeed;

        }

        // 캐릭터에 중력 적용.
        MoveDir.y -= gravity * Time.deltaTime;

        // 캐릭터 움직임.
        controller.Move(MoveDir * Time.deltaTime);

        // 흰트 
        // transform.Rotate(0, 10, 0);

        float headY = Input.GetAxis("Mouse X");
        float headX = Input.GetAxis("Mouse Y") * -1;

        CharCam.transform.Rotate(headX, 0, 0);
        transform.Rotate(0, headY, 0);
        // transform.Rotate(headX, 0, 0);
        // transform.Rotate(0, headY, 0);

    }

}


/*
    Character Controller
    프로퍼티:	기능:
    Slope Limit	콜라이더가 명시된 값보다 작은 경사(단위:도)의 슬로프만 오르도록 제한합니다.
    Step Offset	명시된 값보다 계단이 땅에 가까울 경우에만 캐릭터가 계단을 오릅니다. 이 값은 캐릭터 컨트롤러의 높이보다 커서는 안됩니다. 값이 더 클 경우 오류가 발생합니다.
    Skin width	두 콜라이더가 서로 스킨 너비 만큼 관통할 수 있습니다. 스킨 너비가 클수록 지터링이 감소합니다. 스킨 너비가 작을 경우에는 캐릭터가 움직이지 못할 수 있습니다. 스킨 너비 값을 반지름의 10%로 설정하는 것이 좋습니다.
    Min Move Distance	캐릭터가 지정한 값보다 낮게 움직이려고 할 경우 아예 움직이지 않게 됩니다. 지터링을 줄이기 위해 이 옵션을 사용할 수 있습니다. 대부분의 경우 이 값은 0으로 두어야 합니다.
    Center	월드 공간에서 캡슐 콜라이더를 오프셋하며, 캐릭터의 피벗에는 영향을 주지 않습니다.
    Radius	캡슐 콜라이더의 반지름 길이입니다. 본질적으로는 콜라이더의 너비입니다.
    Height	캐릭터의 Capsule Collider 높이입니다. 이 값을 변경하면 콜라이더가 Y축을 따라 양의 방향과 음의 방향으로 스케일합니다.

    지터링 : 몬테카를로 적분 때문에 발생, 
             대표적인 몬테카를로 식 계산이 Screen Space Ambient Occlusion 이다.
             이게 몇군대 픽셀만 뽑아서 계산을 하는 것 까지는 좋은데 그게 랜덤이다 보니 화면이 움직일때마다 그 뽑히는 수치가 바뀌고 그래서 얼룩덜룩 해지면서 화면이 심하게 그 부위가 덜덜 떨리게 된다. 이걸 지터링이라 한다.
 */
