using UnityEngine;
using Photon.Pun;
using Unity.VisualScripting;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerSetup : MonoBehaviour
{
    public Camera playerCamera;
    public GameObject fpsArms; // 1인칭 팔/총 모델 ROOT
    public GameObject fpsTools;
    public GameObject otherCam;
    public GameObject worldBody; // 월드 3D 모델 ROOT
    private GameObject handholder;
    public HealthBarUI HpUi;
    public HungerBarUI HungerUi;
    public HotbarUI hotbarUI;
    public NameTagFollow nameTag; // 플레이어 이름(NameTag) 오브젝트 연결할 곳

    PhotonView pv;

    void Awake()
    {
        handholder = transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
        fpsArms = handholder.transform.GetChild(1).gameObject;
        fpsTools = handholder.transform.GetChild(0).gameObject;
        otherCam = transform.GetChild(2).gameObject;

        pv = GetComponent<PhotonView>();

        if (pv.IsMine) //나 자신이라면
        {
            SetLayerRecursively(fpsArms, LayerMask.NameToLayer("Tool"));
            SetLayerRecursively(fpsTools, LayerMask.NameToLayer("Tool"));
            HpUi = GameObject.FindGameObjectWithTag("HpUi").GetComponent<HealthBarUI>();
            HungerUi = GameObject.FindGameObjectWithTag("HgUi").GetComponent<HungerBarUI>();
            hotbarUI = GameObject.FindGameObjectWithTag("HotBarUI").GetComponent<HotbarUI>();

            HpUi.player=this.gameObject.GetComponent<FirstPersonController>();
            HungerUi.player=this.gameObject.GetComponent<FirstPersonController>();
            hotbarUI.playerAnimator = this.gameObject.GetComponent<Animator>();

            int local = LayerMask.NameToLayer("LocalPlayerOnly");
            int world = LayerMask.NameToLayer("World");
            int def = LayerMask.NameToLayer("Default");
            playerCamera.cullingMask = (1 << local) | (1 << world) | (1 << def);
            //내 몸 렌더러 꺼버리기
            foreach (var r in worldBody.GetComponentsInChildren<Renderer>())
                r.enabled = false;
        }

        else
        {
            fpsArms.SetActive(false);
            fpsTools.SetActive(false);
            otherCam.SetActive(false);
            playerCamera.enabled = false;
            var audio = playerCamera.GetComponent<AudioListener>();
            if (audio) audio.enabled = false;
        }
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    void Start()
    {
        if (nameTag != null)
        {
            // 이 PhotonView를 소유한 사람의 닉네임
            string nick = pv.Owner.NickName;
            nameTag.SetName(nick);
        }
    }
}
