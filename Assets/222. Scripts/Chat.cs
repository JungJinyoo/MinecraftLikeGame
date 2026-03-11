using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
// (UI 버전에서 사용)
using UnityEngine.UI;

// 이벤트 시스템
using UnityEngine.EventSystems;

// 포톤
using Photon.Pun;            // PUN2
using Photon.Realtime;       // Player, Room 등

public class Chat : MonoBehaviourPunCallbacks
{
    // 포톤 추가////////////////////////////////////////////////
    public Text txtConnect;     // 접속된 플레이어 수 표시
    public Text txtLogMsg;      // 접속 로그 표시
    public InputField inputField;   // ← 인스펙터에서 연결
    public Button sendButton;       // ← 선택: 버튼으로도 전송

    // 채팅모드 관리 추가 11.17
    public GameObject chatGroup;    // 쳇 그룹 연결
    public bool chatMode = false; // 채팅 모드 여부
    [HideInInspector] public bool escHandledThisFrame = false; // ESC 처리 플래그
                                                               //


    public CanvasGroup chatCanvasGroup;     // chatGroup에 붙인 CanvasGroup
    Coroutine fadeRoutine;      // 페이드 코루틴 핸들


    PhotonView pv;              // RPC 호출용

    #region 색상

    // 플레이어마다 사용할 색 팔레트
    static readonly Color[] playerColors = new Color[]
    {
    new Color(1f, 0.3f, 0.3f),  // 빨강 계열
    new Color(0.3f, 1f, 0.3f),  // 초록 계열
    new Color(0.3f, 0.6f, 1f),  // 파랑 계열
    new Color(1f, 0.8f, 0.3f),  // 노랑/주황
    new Color(0.9f, 0.3f, 1f),  // 보라
    new Color(0.3f, 1f, 1f),    // 청록
    };

    string GetColorCodeFor(Player p)
    {
        if (p == null) return "ffffff";

        int idx = (p.ActorNumber - 1) % playerColors.Length;
        Color c = playerColors[idx];

        // HTML용 #RRGGBB 문자열로 변환
        return ColorUtility.ToHtmlStringRGB(c);
    }

    string GetColoredNick(Player p)
    {
        string color = GetColorCodeFor(p);
        return $"<color=#{color}>{p.NickName}</color>";
    }

    #endregion


    void Awake()
    {
        // chatCanvasGroup 자동 연결 (혹시 인스펙터에서 안 넣었어도)
        if (chatCanvasGroup == null && chatGroup != null)
        {
            chatCanvasGroup = chatGroup.GetComponent<CanvasGroup>();
        }

        // 기본 상태 : 채팅 꺼져 있고, 완전 투명
        chatMode = false;
        if (chatGroup != null) chatGroup.SetActive(false);
        if (chatCanvasGroup != null) chatCanvasGroup.alpha = 0f;



        ////11.17추가
        //chatGroup.SetActive(chatMode);
        ////


        //if (chatCanvasGroup != null)
        //{
        //    chatCanvasGroup.alpha = chatMode ? 1f : 0;
        //}


        // 포톤 추가////////////////////////////////////////////////
        pv = GetComponent<PhotonView>();

        PhotonNetwork.IsMessageQueueRunning = true;

        // 룸에 입장한 후 기존 접속자 정보를 출력
        GetConnectPlayerCount();
        ////////////////////////////////////////////////////////////


        // PlayerPrefs에서 닉네임 가져오기
        string savedName = PlayerPrefs.GetString("PLAYER_NAME", "");

        if (!string.IsNullOrEmpty(savedName))
        {
            PhotonNetwork.NickName = savedName;
            Debug.Log($"[Chat] Restored NickName: {PhotonNetwork.NickName}");
        }
        else
        {
            PhotonNetwork.NickName = $"Player{Random.Range(1000, 9999)}";
            Debug.Log("[Chat] No saved name found, assigning random name.");
        }

        PhotonNetwork.IsMessageQueueRunning = true;
        GetConnectPlayerCount();
    }



    void Start()
    {
        if (sendButton) sendButton.onClick.AddListener(SendCurrentInput);
        if (txtLogMsg) txtLogMsg.supportRichText = true;

        if (inputField)
        {
            inputField.lineType = InputField.LineType.SingleLine;
            inputField.onSubmit.AddListener(OnChatSubmit);
            inputField.onValueChanged.AddListener(_ => ClampSelection());
        }

        //  마스터만 입장 로그를 보내도록 제한
        if (!PhotonNetwork.IsMasterClient)
            return;

        var me = PhotonNetwork.LocalPlayer;
        string meColored = GetColoredNick(me);

        photonView.RPC("LogMsg", RpcTarget.AllBuffered,
            $"\n\t[{meColored}] 서버 연결 완료");

    }

    void OnChatSubmit(string text)
    {
        SendCurrentInput();
    }

    void OnInputFieldSubmit(string text)
    {
        // 엔터로 제출되었을 때만 실행 (ESC는 빈 문자열로 옴)
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SendCurrentInput();
        }
    }


    void Update()
    {
        escHandledThisFrame = false; // 매 프레임 초기화 11.17추가

        if (!inputField) return;

        // ESC로 채팅 모드 빠져나오기
        if (chatMode && Input.GetKeyDown(KeyCode.Escape))
        {
            ExitChatModeInstant();
            escHandledThisFrame = true; // ESC 처리 완료 11.17추가
            //return;
        }

        // ✨ 수정: InputField가 포커스되지 않았을 때만 엔터 감지
        if (!inputField.isFocused && Input.GetKeyDown(KeyCode.T))
        {
            EnterChatMode();
        }

        // ▼ 커서/선택 인덱스 클램프(한 글자 예외 방지용, 아래 B와 함께)
        if (inputField.isFocused) ClampSelection();
    }



    public void SendCurrentInput()
    {
        if (!inputField) return;

        string message = inputField.text.Trim();
        if (string.IsNullOrEmpty(message)) return;

        Player me = PhotonNetwork.LocalPlayer;
        string nick = GetColoredNick(me);
        photonView.RPC("LogMsg", RpcTarget.AllBuffered, $"\n{nick}: {message}");


        inputField.text = string.Empty;
        ResetCaretToEnd();

        // ✨ 전송 후 채팅 모드 유지하려면 이 줄 추가
        // inputField.ActivateInputField();

        // ✨ 전송 후 채팅 모드 종료하려면 이 줄 추가
        StartAutoFadeOut();
    }

    void ResetCaretToEnd()
    {
        if (!inputField) return;
        int len = inputField.text?.Length ?? 0;
        inputField.caretPosition = len;
        inputField.selectionAnchorPosition = len;
        inputField.selectionFocusPosition = len;
    }


    //포톤 추가
    //룸 접속자 정보를 조회하는 함수
    void GetConnectPlayerCount()
    {
        // PUN2: PhotonNetwork.CurrentRoom
        Room currRoom = PhotonNetwork.CurrentRoom;
        if (currRoom == null || txtConnect == null) return;

        txtConnect.text = currRoom.PlayerCount.ToString()
                            + "/"
                            + currRoom.MaxPlayers.ToString();
    }

    // === PUN Classic 콜백 대체 ===
    // 네트워크 플레이어가 룸으로 접속했을 때 호출되는 콜백(PUN2)
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // 디버그 (기존 ToStringFull 유사 정보는 newPlayer.ToStringFull() 대신 필요한 값 출력)

        GetConnectPlayerCount();

        //  마스터만 입장 로그를 보내도록 제한
        if (!PhotonNetwork.IsMasterClient)
            return;

        // 입장 로그도 남기고 싶다면:
        string colored = GetColoredNick(newPlayer);
        photonView.RPC("LogMsg", RpcTarget.AllBuffered,
            $"\n\t[{colored}] 님이 입장");
    }

    // 네트워크 플레이어가 룸을 나갔을 때(PUN2)
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"[Leave] {otherPlayer.ActorNumber} {otherPlayer.NickName}");
        GetConnectPlayerCount();

        //  마스터만 입장 로그를 보내도록 제한
        if (!PhotonNetwork.IsMasterClient)
            return;

        string colored = GetColoredNick(otherPlayer);
        photonView.RPC("LogMsg", RpcTarget.AllBuffered,
            $"\n\t[{colored}] 님이 퇴장");
    }

    // 내가 룸에 조인 완료했을 때(PUN2)
    public override void OnJoinedRoom()
    {
        GetConnectPlayerCount();
    }

    // 포톤 추가
    [PunRPC]
    void LogMsg(string msg)
    {
        if (!txtLogMsg) return;
        txtLogMsg.text = txtLogMsg.text + msg;
    }

    // 포톤 추가
    // 룸 나가기 버튼
    public void OnClickExitRoom()
    {
        var me = PhotonNetwork.LocalPlayer;
        string colored = GetColoredNick(me);
        string msg = $"\n\t[{colored}] 님이 접속 종료";
        photonView.RPC("LogMsg", RpcTarget.AllBuffered, msg);

        PhotonNetwork.LeaveRoom();
        //(!) 서버에 통보 후 내 객체 및 RPC 정리
    }

    // 룸에서 접속 종료됐을 때 호출 (PUN2)
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("scLobby");
    }



    void EnterChatMode()
    {
        // 페이드 중이었다면 중단
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        chatMode = true;


        if (chatCanvasGroup != null) chatGroup.SetActive(true);
        if (chatCanvasGroup != null) chatCanvasGroup.alpha = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 인풋 필드에 포커스 강제
        if (inputField)
        {
            EventSystem.current?.SetSelectedGameObject(inputField.gameObject);
            inputField.ActivateInputField();
            ResetCaretToEnd();
        }

        // (선택) 플레이어 시점/이동 스크립트 비활성화
        // playerLook.enabled = false; playerMove.enabled = false;
    }

    void ExitChatMode()
    {
        chatMode = false;
        chatGroup.SetActive(chatMode);

        // (선택) 플레이어 시점/이동 스크립트 다시 활성화
        // playerLook.enabled = true; playerMove.enabled = true;

        EventSystem.current?.SetSelectedGameObject(null);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void ExitChatModeInstant()
    {
        // 페이드 중이면 중지
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        chatMode = false;

        if (chatCanvasGroup != null) chatCanvasGroup.alpha = 0f;

        if (chatGroup != null) chatGroup.SetActive(false);



        EventSystem.current?.SetSelectedGameObject(null);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    void StartAutoFadeOut()
    {
        // 이미 돌아가는 페이드 있으면 중지
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(FadeOutChat(3f, 1.8f));
        // 2f : 몇 초 있다가 사라질지 (대기 시간)
        // 0.5f : 서서히 사라지는 시간
    }

    IEnumerator FadeOutChat(float delay, float duration)
    {
        // 먼저 입력 초점 제거 + 게임으로 복귀
        chatMode = false;
        EventSystem.current?.SetSelectedGameObject(null);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yield return new WaitForSeconds(delay);

        if (chatCanvasGroup == null)
        {
            if (chatGroup != null)
                chatGroup.SetActive(false);
            fadeRoutine = null;

            yield break;
        }

        float t = 0f;
        float startAlpha = chatCanvasGroup.alpha;


        while (t < duration)
        {
            t += Time.unscaledDeltaTime;        // 시간 멈춤에 영향 안 받게
            float a = Mathf.Lerp(startAlpha, 0f, t / duration);
            chatCanvasGroup.alpha = a;
            yield return null;
        }

        chatCanvasGroup.alpha = 0f;
        if (chatGroup != null) chatGroup.SetActive(false);

        fadeRoutine = null;

    }


    void ClampSelection()
    {
        if (!inputField) return;
        int len = inputField.text?.Length ?? 0;
        // 음수/범위 밖 방지
        inputField.caretPosition = Mathf.Clamp(inputField.caretPosition, 0, len);
        inputField.selectionAnchorPosition = Mathf.Clamp(inputField.selectionAnchorPosition, 0, len);
        inputField.selectionFocusPosition = Mathf.Clamp(inputField.selectionFocusPosition, 0, len);
    }

}
