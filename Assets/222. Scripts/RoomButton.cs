using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class RoomButton : MonoBehaviour
{
    [SerializeField] private Text roomNameText;
    public string RoomName { get; private set; }
    public bool Updated { get; set; }

    public void SetInfo(RoomInfo info)
    {
        RoomName = info.Name;
        roomNameText.text = $"{info.Name} ({info.PlayerCount}/{info.MaxPlayers})";
    }

    public void OnClickJoin()
    {
        Debug.Log("[PUN] Joining Room: " + RoomName);
        PhotonNetwork.JoinRoom(RoomName);
    }
}
