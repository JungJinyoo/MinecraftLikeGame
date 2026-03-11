using UnityEngine;
using Photon.Pun;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public Transform spawnPoint;

    private bool hasSpawned = false; // ✅ static 제거

    void Start()
    {
        Debug.Log($"[GameManager] Start");
        Debug.Log($"[GameManager] InRoom: {PhotonNetwork.InRoom}");
        Debug.Log($"[GameManager] Connected: {PhotonNetwork.IsConnectedAndReady}");
        Debug.Log($"[GameManager] Current Scene: {SceneManager.GetActiveScene().name}");

        // ✅ 로컬 테스트 모드
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogWarning("[GameManager] Not connected to Photon. Local test mode.");
            Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
            return;
        }

        // ✅ 게임 씬에서 방에 있으면 바로 스폰 시도
        if (PhotonNetwork.InRoom && PhotonNetwork.IsConnectedAndReady)
        {
            StartCoroutine(WaitAndSpawn());
        }
    }

    IEnumerator WaitAndSpawn()
    {
        Debug.Log("[GameManager] WaitAndSpawn started");

        // ✅ 포톤이 완전히 준비될 때까지 대기
        float waitTime = 0f;
        while ((!PhotonNetwork.InRoom || !PhotonNetwork.IsConnectedAndReady) && waitTime < 5f)
        {
            Debug.Log($"[GameManager] Waiting... InRoom:{PhotonNetwork.InRoom}, Ready:{PhotonNetwork.IsConnectedAndReady}");
            yield return new WaitForSeconds(0.2f);
            waitTime += 0.2f;
        }

        if (!PhotonNetwork.InRoom)
        {
            Debug.LogError("[GameManager] Still not in room after waiting!");
            yield break;
        }

        // ✅ 추가 대기 (씬 완전 로드 보장)
        yield return new WaitForSeconds(0.5f);

        // ✅ 내 플레이어가 이미 있는지 확인
        if (FindMyPlayer() != null)
        {
            Debug.Log("[GameManager] My player already exists in scene");
            hasSpawned = true;
            yield break;
        }

        // ✅ 플레이어 스폰
        TrySpawnPlayer();
    }

    void TrySpawnPlayer()
    {
        if (hasSpawned)
        {
            Debug.Log("[GameManager] Already spawned this session");
            return;
        }

        // ✅ 다시 한번 확인
        if (FindMyPlayer() != null)
        {
            Debug.Log("[GameManager] Player found before spawn, skipping");
            hasSpawned = true;
            return;
        }

        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : new Vector3(0, 100, 0);

        Debug.Log($"[GameManager] Spawning player at {spawnPos}");

        try
        {
            GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, Quaternion.identity);

            if (player != null)
            {
                hasSpawned = true;
                PhotonView pv = player.GetComponent<PhotonView>();
                Debug.Log($"[GameManager] ✓ Player spawned! ViewID: {pv?.ViewID}, IsMine: {pv?.IsMine}");
            }
            else
            {
                Debug.LogError("[GameManager] PhotonNetwork.Instantiate returned null!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] Spawn failed: {e.Message}");
        }
    }

    // ✅ 내 플레이어 찾기
    GameObject FindMyPlayer()
    {
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (var p in allPlayers)
        {
            PhotonView pv = p.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                return p;
            }
        }
        return null;
    }

    // ✅ 새 플레이어가 방에 들어왔을 때
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log($"[GameManager] New player joined: {newPlayer.NickName}, Total: {PhotonNetwork.CurrentRoom.PlayerCount}");
    }

    // ✅ 방에 입장했을 때 (씬 로드 후에도 호출됨)
    public override void OnJoinedRoom()
    {
        Debug.Log($"[GameManager] OnJoinedRoom callback");

        // ✅ 아직 스폰 안 했으면 다시 시도
        if (!hasSpawned && FindMyPlayer() == null)
        {
            Debug.Log("[GameManager] OnJoinedRoom: Attempting spawn...");
            StartCoroutine(WaitAndSpawn());
        }
    }

    // ✅ 디버깅용 GUI
    void OnGUI()
    {
        // GUIStyle style = new GUIStyle(GUI.skin.label);
        // style.fontSize = 16;
        // style.normal.textColor = Color.white;

        // GUILayout.BeginArea(new Rect(10, 10, 300, 200));

        // GUILayout.Label($"InRoom: {PhotonNetwork.InRoom}", style);
        // GUILayout.Label($"Connected: {PhotonNetwork.IsConnectedAndReady}", style);
        // GUILayout.Label($"HasSpawned: {hasSpawned}", style);
        // GUILayout.Label($"PlayerCount: {PhotonNetwork.CurrentRoom?.PlayerCount ?? 0}", style);

        // GameObject myPlayer = FindMyPlayer();
        // GUILayout.Label($"MyPlayer: {(myPlayer != null ? "Found" : "NULL")}", style);

        // GUILayout.Space(10);

        // if (GUILayout.Button("Force Spawn Player", GUILayout.Height(40)))
        // {
        //     hasSpawned = false;
        //     TrySpawnPlayer();
        // }

        // GUILayout.EndArea();
    }

    // ✅ 키보드 디버깅
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F4))
        {
            Debug.Log("=== GameManager Debug ===");
            Debug.Log($"HasSpawned: {hasSpawned}");
            Debug.Log($"InRoom: {PhotonNetwork.InRoom}");
            Debug.Log($"Connected: {PhotonNetwork.IsConnectedAndReady}");
            Debug.Log($"Scene: {SceneManager.GetActiveScene().name}");

            GameObject myPlayer = FindMyPlayer();
            Debug.Log($"MyPlayer exists: {myPlayer != null}");

            GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
            Debug.Log($"Total Players in scene: {allPlayers.Length}");
        }

        // ✅ F5로 강제 스폰 (테스트용)
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Debug.Log("F5 - Force spawn attempt");
            hasSpawned = false;
            TrySpawnPlayer();
        }
    }
}