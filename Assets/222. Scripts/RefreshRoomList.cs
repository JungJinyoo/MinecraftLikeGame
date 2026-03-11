using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefreshRoomList : MonoBehaviour
{
    public void RefreshRoomList0()
    {
        if (PhotonNetwork.InLobby)
        {
            Debug.Log("[RoomListDisplay] Refresh requested!");
            PhotonNetwork.LeaveLobby();
            PhotonNetwork.JoinLobby(TypedLobby.Default);
        }
    }
}
