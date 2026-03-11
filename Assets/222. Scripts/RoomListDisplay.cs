using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomListDisplay : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject roomButtonPrefab;
    [SerializeField] private Transform content;

    [SerializeField] private InputField searchInput;   //  검색 입력칸


    // 방 목록 캐시 (실제 PUN 방 정보 저장)
    private readonly List<RoomInfo> cachedRooms = new();

    // 현재 화면에 만들어진 버튼들
    private readonly List<RoomButton> buttons = new();

    void Start()
    {
        // 검색어 바뀔 때마다 호출
        if (searchInput != null)
        {
            searchInput.onValueChanged.AddListener(OnSearchChanged);
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log($"[RoomListDisplay] Room list updated: {roomList.Count}");

        // 1) 방 정보 캐시 갱신
        foreach (var room in roomList)
        {
            Debug.Log($" - {room.Name} ({room.PlayerCount}/{room.MaxPlayers}) Visible={room.IsVisible}");

            int idx = cachedRooms.FindIndex(r => r.Name == room.Name);

            // 삭제/숨김된 방 처리
            if (room.RemovedFromList || !room.IsVisible || room.PlayerCount == 0)
            {
                if (idx != -1)
                    cachedRooms.RemoveAt(idx);
                continue;
            }

            // 새 방 or 기존 방 갱신
            if (idx == -1)
                cachedRooms.Add(room);
            else
                cachedRooms[idx] = room;
        }

        // 2) 현재 검색어 기준으로 버튼 다시 그림
        RebuildButtons();
    }

    //void Receive(RoomInfo room)
    //{
    //    int idx = buttons.FindIndex(b => b.RoomName == room.Name);

    //    if (idx == -1)
    //    {
    //        if (room.IsVisible && room.PlayerCount < room.MaxPlayers)
    //        {
    //            var obj = Instantiate(roomButtonPrefab, content);
    //            var btn = obj.GetComponent<RoomButton>();
    //            buttons.Add(btn);
    //            idx = buttons.Count - 1;
    //        }
    //    }

    //    if (idx != -1)
    //    {
    //        buttons[idx].SetInfo(room);
    //        buttons[idx].Updated = true;
    //    }
    //}

    //void Prune()
    //{
    //    var dead = new List<RoomButton>();

    //    foreach (var b in buttons)
    //    {
    //        if (!b.Updated) dead.Add(b);
    //        else b.Updated = false;
    //    }

    //    foreach (var b in dead)
    //    {
    //        buttons.Remove(b);
    //        Destroy(b.gameObject);
    //    }
    //}

    void OnSearchChanged(string _)
    {
        // 검색어가 바뀔 때마다 전체 버튼 다시 그림
        RebuildButtons();
    }

    void RebuildButtons()
    {
        string keyword = "";
        if (searchInput != null)
            keyword = searchInput.text.Trim();

        // 1) 기존 버튼 전부 제거
        foreach (Transform child in content)
            Destroy(child.gameObject);
        buttons.Clear();

        // 2) 캐시된 방들 중에서 검색어에 맞는 것만 버튼 생성
        foreach (var room in cachedRooms)
        {
            // 검색어가 있고, 방 이름에 그 글자가 없으면 스킵
            if (!string.IsNullOrEmpty(keyword) &&
                !room.Name.Contains(keyword))     // 한글도 OK, "마" 입력 → "마인크래프트", "마크"만 통과
                continue;

            var obj = Instantiate(roomButtonPrefab, content);
            var btn = obj.GetComponent<RoomButton>();
            btn.SetInfo(room);
            buttons.Add(btn);
        }
    }

    // 수동 새로고침 버튼 (버튼 연결용)
    public void RefreshRoomList()
    {
        if (PhotonNetwork.InLobby)
        {
            Debug.Log("[RoomListDisplay] Refresh requested");
            PhotonNetwork.LeaveLobby();
            PhotonNetwork.JoinLobby(TypedLobby.Default);
        }
        else
        {
            Debug.LogWarning("[RoomListDisplay] Not in lobby, cannot refresh");
        }
    }
}
