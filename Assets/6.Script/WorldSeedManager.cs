using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public static class NetKeys
{
    public const string WORLD_SEED = "WORLD_SEED";
}

public class WorldSeedManager : MonoBehaviourPunCallbacks
{
    public static int WorldSeed { get; private set; }
    public static bool HasSeed { get; private set; }

    private bool waitingForSeed = false;

    private void Awake()
    {
        var exists = FindObjectsOfType<WorldSeedManager>();
        if (exists.Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        Debug.Log($"[WorldSeedManager] Scene loaded: {s.name}");
        Debug.Log($"[WorldSeedManager] InRoom: {PhotonNetwork.InRoom}, HasSeed: {HasSeed}");

        if (!PhotonNetwork.InRoom) return;

        // 게임 씬이고 시드가 있으면 즉시 월드 생성
        if (s.name == "Minecrafttest" && HasSeed)
        {
            var world = FindObjectOfType<World>();
            if (world != null)
            {
                Debug.Log($"[WorldSeedManager] Generating world immediately with seed {WorldSeed}");
                world.BeginGenerate(WorldSeed);
            }
        }
        // 게임 씬인데 시드가 없으면 대기 시작
        else if (s.name == "Minecrafttest" && !HasSeed)
        {
            Debug.Log("[WorldSeedManager] No seed yet, starting to wait...");
            StartCoroutine(WaitForSeedAndGenerate());
        }
    }

    // ⭐ 시드를 받을 때까지 적극적으로 대기
    IEnumerator WaitForSeedAndGenerate()
    {
        if (waitingForSeed) yield break; // 이미 대기 중
        waitingForSeed = true;

        float waitTime = 0f;
        float maxWaitTime = 10f;

        while (!HasSeed && waitTime < maxWaitTime && PhotonNetwork.InRoom)
        {
            // 0.2초마다 Room Properties 체크
            TryApplySeedFromRoom();

            yield return new WaitForSeconds(0.2f);
            waitTime += 0.2f;

            if (waitTime % 1f < 0.2f)
            {
                Debug.Log($"[WorldSeedManager] Waiting for seed... ({waitTime:F1}s)");
            }
        }

        if (HasSeed)
        {
            Debug.Log($"[WorldSeedManager] Seed received after {waitTime:F1}s: {WorldSeed}");
            var world = FindObjectOfType<World>();
            if (world != null)
            {
                world.BeginGenerate(WorldSeed);
            }
        }
        else
        {
            Debug.LogError($"[WorldSeedManager] Failed to get seed after {waitTime:F1}s!");
            Debug.LogError($"InRoom: {PhotonNetwork.InRoom}, IsMaster: {PhotonNetwork.IsMasterClient}");
        }

        waitingForSeed = false;
    }

    public override void OnLeftRoom()
    {
        HasSeed = false;
        WorldSeed = 0;
        waitingForSeed = false;
        Debug.Log("[WorldSeedManager] Left room, reset seed");
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("[WorldSeedManager] OnCreatedRoom - Setting seed as Master");
        SetSeedAsMaster();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"[WorldSeedManager] OnJoinedRoom - IsMaster: {PhotonNetwork.IsMasterClient}");

        if (PhotonNetwork.IsMasterClient)
        {
            // 방장은 즉시 시드 설정
            SetSeedAsMaster();
        }
        else
        {
            // 클라이언트는 Room Properties에서 읽기 시도
            TryApplySeedFromRoom();
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        Debug.Log("[WorldSeedManager] OnRoomPropertiesUpdate");

        if (propertiesThatChanged != null && propertiesThatChanged.ContainsKey(NetKeys.WORLD_SEED))
        {
            Debug.Log($"[WorldSeedManager] Seed property changed: {propertiesThatChanged[NetKeys.WORLD_SEED]}");
            TryApplySeedFromRoom();
        }
    }

    void SetSeedAsMaster()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("[WorldSeedManager] Not master, cannot set seed");
            return;
        }

        // 이미 시드가 설정되어 있으면 스킵
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(NetKeys.WORLD_SEED))
        {
            Debug.Log("[WorldSeedManager] Seed already exists in room");
            TryApplySeedFromRoom();
            return;
        }

        int seed = Random.Range(int.MinValue, int.MaxValue);
        var props = new Hashtable { { NetKeys.WORLD_SEED, seed } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        WorldSeed = seed;
        HasSeed = true;
        Debug.Log($"[WorldSeedManager] Master set seed: {seed}");
    }

    void TryApplySeedFromRoom()
    {
        var room = PhotonNetwork.CurrentRoom;
        if (room == null)
        {
            Debug.LogWarning("[WorldSeedManager] CurrentRoom is null");
            return;
        }

        if (room.CustomProperties == null)
        {
            Debug.LogWarning("[WorldSeedManager] CustomProperties is null");
            return;
        }

        if (!room.CustomProperties.ContainsKey(NetKeys.WORLD_SEED))
        {
            Debug.LogWarning("[WorldSeedManager] WORLD_SEED not in CustomProperties");
            Debug.Log($"[WorldSeedManager] Available keys: {string.Join(", ", room.CustomProperties.Keys)}");
            return;
        }

        int receivedSeed = (int)room.CustomProperties[NetKeys.WORLD_SEED];

        if (HasSeed && WorldSeed == receivedSeed)
        {
            Debug.Log($"[WorldSeedManager] Already have this seed: {receivedSeed}");
            return;
        }

        WorldSeed = receivedSeed;
        HasSeed = true;
        Debug.Log($"[WorldSeedManager] Applied seed: {WorldSeed}");

        // 게임 씬에서 시드를 받았으면 즉시 월드 생성
        if (SceneManager.GetActiveScene().name == "Minecrafttest")
        {
            var world = FindObjectOfType<World>();
            if (world != null)
            {
                Debug.Log($"[WorldSeedManager] Calling BeginGenerate with seed {WorldSeed}");
                world.BeginGenerate(WorldSeed);
            }
        }
    }
}