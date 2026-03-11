using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class CreateRoomUI : MonoBehaviour
{
    [SerializeField] InputField roomNameInput;

    public void OnClickPlayGame()
    {
        string name = roomNameInput.text;
        NetworkManager.Instance.CreateRoom(name);
    }

    public void OnClickCreateRoom()
    {
        SceneManager.LoadScene("RoomCreate");
    }


    public void OnClicksLOBY()
    {
        // 방 안에 있으면 먼저 방 나가기
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            // OnLeftRoom 콜백에서 씬 이동하는 게 제일 깔끔
        }
        else
        {
            SceneManager.LoadScene("scLOBY");
        }
    }

    public void OnClickRoomList()
    {
        SceneManager.LoadScene("RoomList");
    }
}
