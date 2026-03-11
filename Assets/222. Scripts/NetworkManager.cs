using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;
    public string gameVersion = "1.0";
    public string gameSceneName = "Minecrafttest";

    private bool isLoadingScene = false; //  씬 로드 중복 방지

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.GameVersion = gameVersion;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("[PUN] Connected to Master Server");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("[PUN] Joined Lobby");
    }

    public void CreateRoom(string roomName)
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("[NetworkManager] Not connected to master server yet!");
            return;
        }

        if (string.IsNullOrWhiteSpace(roomName))
            roomName = "Room_" + Random.Range(1000, 9999);

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = 4,
            IsVisible = true,
            IsOpen = true
        };

        PhotonNetwork.CreateRoom(roomName, options);
    }

    public override void OnJoinedRoom()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("[NetworkManager] Not connected to master server yet!");
            return;
        }

        Debug.Log($"[NetworkManager] OnJoinedRoom");
        Debug.Log($"[NetworkManager] Room: {PhotonNetwork.CurrentRoom.Name}");
        Debug.Log($"[NetworkManager] Players: {PhotonNetwork.CurrentRoom.PlayerCount}");

        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"[NetworkManager] Current Scene: {currentScene}");

        if (currentScene == gameSceneName)
        {
            Debug.Log("[NetworkManager] Already in game scene");
            return;
        }

        if (isLoadingScene)
        {
            Debug.Log("[NetworkManager] Scene already loading");
            return;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(LoadGameScene());
        }
        else
        {
            Debug.Log("[NetworkManager] Waiting for Master to load scene...");
            isLoadingScene = true; // 중복 호출 방지
        }
    }

    IEnumerator LoadGameScene()
    {
        isLoadingScene = true;

        Debug.Log($"[PUN] Loading scene: {gameSceneName}");
        yield return new WaitForSeconds(0.5f);

        //  마스터는 PhotonNetwork.LoadLevel 사용 (다른 플레이어도 같이 로드됨)
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(gameSceneName);
        }
        else
        {
            //  참가자는 일반 씬 로드 (또는 마스터 씬 로드 대기)
            // 방법 1: 직접 로드
            SceneManager.LoadScene(gameSceneName);

            // 방법 2: 마스터가 로드할 때까지 대기 (AutoSync가 작동한다면)
            // yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(1f);
        isLoadingScene = false;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"[PUN] Player Entered: {newPlayer.NickName}, Total: {PhotonNetwork.CurrentRoom.PlayerCount}");
    }

    public override void OnLeftRoom()
    {
        Debug.Log("[PUN] Left Room");
        isLoadingScene = false;

        //  로비로 돌아가기
        if (SceneManager.GetActiveScene().name != "RoomList")
        {
            SceneManager.LoadScene("RoomList");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            Debug.Log("=== Photon Debug Info ===");
            Debug.Log($"Connected: {PhotonNetwork.IsConnected}");
            Debug.Log($"InRoom: {PhotonNetwork.InRoom}");
            Debug.Log($"IsMaster: {PhotonNetwork.IsMasterClient}");
            Debug.Log($"Current Scene: {SceneManager.GetActiveScene().name}");
            Debug.Log($"Player Count: {PhotonNetwork.CurrentRoom?.PlayerCount ?? 0}");
            Debug.Log($"IsLoadingScene: {isLoadingScene}");
        }
    }
}