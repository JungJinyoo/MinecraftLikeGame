using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public float speed;
    private bool walking;
    public float walkspeed;
    public float runspeed;
    public float jumpspeed;
    public float gravity;
    public AudioClip[] Jumosound;
    public AudioClip[] Worksound;

    private Vector3 MoveDir;
    private Vector3 HeadDir;

    public Camera CharCam;

    private CharacterController charcon;
    private AudioSource audioSource;

    void Awake()
    {
        charcon = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
    }
    void Start()
    {
        walkspeed = 6.0f;
        runspeed = 8.0f;
        jumpspeed = 8.0f;
        gravity = 20.0f;

        MoveDir = Vector3.zero;
    }
    void Update()
    {
        if (charcon.isGrounded)
        {
            MoveDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            MoveDir = transform.TransformDirection(MoveDir);

            MoveDir *= speed;

            if (Input.GetKeyDown(KeyCode.Space))
            { MoveDir.y = jumpspeed; }
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            Debug.Log("달리기");
            speed = runspeed;
        }
        else
        { speed = walkspeed; }

        // 캐릭터에 중력 적용.
        MoveDir.y -= gravity * Time.deltaTime;

        // 캐릭터 움직임.
        charcon.Move(MoveDir * Time.deltaTime);

        float headY = Input.GetAxis("Mouse X");
        float headX = Input.GetAxis("Mouse Y") * -1;

        CharCam.transform.Rotate(headX, 0, 0);
        transform.Rotate(0, headY, 0);

    }
}
