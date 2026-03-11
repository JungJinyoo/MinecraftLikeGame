using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
public class ButtonCtrl : MonoBehaviour
{
    public GameObject Hand;

    [Header("ESC 버튼들")]
    public GameObject ESCButtons;
    private bool isESCButtonVisible = false;
    public GameObject pnlOption;
    private bool OptionOpen = false;
    public GameObject pnlMusicSounds;
    private bool MusicSoundsOpen = false;

    private GameObject canvasUI;

    [Header("UI 패널들")]
    public GameObject Inventory;
    public GameObject RecipeBook;
    public GameObject StoveBook;
    
    public GameObject Stove;
    public GameObject Crafting;
    public GameObject StoveUI;


    private bool isInventoryVisible = false;
    private bool isRecipeBookVisible = false;
    private bool isStoveBookVisible = false;
    public bool isCraftingVisible = false;
    public bool isStoveVisible = false;
    //버튼 추가생성 금지
    private bool isMusicBtnAdded = false;

    void Awake()
    {
        canvasUI=GameObject.FindGameObjectWithTag("CanvasUI");
    }
    void Start()
    {
        //일부로 스타트에 할당.
        pnlMusicSounds=canvasUI.transform.GetChild(canvasUI.transform.childCount-1).gameObject;
    }
    // Update is called once per frame
    void Update()
    {
        // ESC 키 → ESC 버튼 표시 토글 + UI 닫기
        if (Input.GetKeyDown(KeyCode.Escape))
        {

            ToggleESCButtons();

            if (isInventoryVisible)
            {
                SetInventory(false);
                SetRecipeBook(false);
                isCraftingVisible = false;
                //isStoveVisible = false;
            }
        }
        //크래프팅박스 테스트
        //보일때
        if (isCraftingVisible)
        {
            SetInventory(true);
            Crafting.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (!isStoveVisible && !isCraftingVisible && !isInventoryVisible && !isESCButtonVisible)
        {
            Crafting.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        if (isStoveVisible)
        {
            SetInventory(true);
            Stove.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (!isStoveVisible && !isCraftingVisible && !isInventoryVisible && !isESCButtonVisible)
        {
            Stove.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // E 키 → 인벤토리 토글
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isInventoryVisible || isRecipeBookVisible)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                // 인벤토리와 레시피북 중 하나라도 열려 있으면 모두 닫기
                SetInventory(false);
                SetRecipeBook(false);
                isCraftingVisible = false;
                CloseStove();
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                // 아무 것도 안 열려 있으면 인벤토리 열기
                SetInventory(true);
            }
        }

    }
    public void OpenStove(Stove stove)
    {
        isStoveVisible = true;
        Stove.SetActive(true);

        StoveUI ui = FindObjectOfType<StoveUI>();//StoveUI.GetComponent<StoveUI>();
        ui.Open(stove);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void CloseStove()
    {
        isStoveVisible = false;
        Stove.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    private void ToggleESCButtons()
    {
        isESCButtonVisible = !isESCButtonVisible;
        CloseStove();
        isCraftingVisible = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ESCButtons.SetActive(isESCButtonVisible);
        Hand.SetActive(!isESCButtonVisible);
    }
    // ▶ 인벤토리 열고 닫기
    private void SetInventory(bool visible)
    {
        isInventoryVisible = visible;
        Inventory.SetActive(visible);
        Inventory.transform.localPosition = visible && isRecipeBookVisible
            ? new Vector3(356, 0, 0)
            : new Vector3(0, 0, 0);
    }

    // ▶ 생산창 열고 닫기
    private void SetRecipeBook(bool visible)
    {
        isRecipeBookVisible = visible;
        RecipeBook.SetActive(visible);
    }

    // ▶ 화로 열고 닫기
    public void SetStoveBook(bool visible)
    {
        isStoveBookVisible = visible;
        StoveBook.SetActive(visible);
    }

    // ▶ 인벤토리 내 버튼 클릭 시 (생산창 열고 닫기)
    public void InvenBtnClick()
    {
        bool newState = !isRecipeBookVisible;
        SetRecipeBook(newState);

        // 인벤토리 위치 조정
        Inventory.transform.localPosition = newState
            ? new Vector3(356, 0, 0)
            : new Vector3(0, 0, 0);
    }


    //게임으로 돌아가기
    public void BackToGame()
    {
        isESCButtonVisible = !isESCButtonVisible;
        ESCButtons.SetActive(isESCButtonVisible);
    }

    //저장하고 나가기 추가
    public void SaveAndExit()
    {
        // 1. 데이터 저장


        // 2. 방에서 나가기 + 로비로 이동
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            SceneManager.LoadScene("scLOBY");
        }
    }
    public void OnClickOption()
    {
        ESCButtons.SetActive(false);
        pnlOption.SetActive(true);
        OptionOpen = true;
    }

    public void OnClickDone()
    {
        if (OptionOpen)
        {
            pnlOption.SetActive(false);
            OptionOpen = false;
            ESCButtons.SetActive(true);
        }
        else if (MusicSoundsOpen)
        {
            pnlMusicSounds.SetActive(false);
            MusicSoundsOpen = false;
            pnlOption.SetActive(true);
            OptionOpen = true;
        }

    }

    //음악 및 소리...
    public void OnClickMusicSounds()
    {
        pnlOption.SetActive(false);
        OptionOpen = false;
        pnlMusicSounds.SetActive(true);
        MusicSoundsOpen = true;
        if(MusicSoundsOpen==true)
        {
            //꼼수로 버튼에 추가버튼 할당해주기
            AddMusicbtn();
        }
    }
    public void AddMusicbtn()
    {
        if (isMusicBtnAdded) return;
        Debug.Log("버튼추가");
        //내용은 해당 오브젝트의 가장 마지막 자식의 버튼 컴퍼넌트의 온클릭에 온클릭던() 을 추가한다는 내용
        pnlMusicSounds.transform.GetChild(pnlMusicSounds.transform.childCount-1).
        GetComponent<Button>().onClick.AddListener(() => OnClickDone());
        isMusicBtnAdded = true;
    }
}
