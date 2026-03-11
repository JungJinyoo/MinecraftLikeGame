using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class NameEntry : MonoBehaviour
{
    [SerializeField] InputField nameField;

    void Start()
    {
        // 저장된 이름 있으면 미리 채워주기
        var saved = PlayerPrefs.GetString("player_name", "");
        if (!string.IsNullOrEmpty(saved)) nameField.text = saved;
    }

    public void OnClickConfirmName()
    {
        string playerName = nameField.text.Trim();

        if (string.IsNullOrEmpty(playerName))
            playerName = $"Player{Random.Range(1000, 9999)}";

        // Photon 닉네임 설정
        PhotonNetwork.NickName = playerName;

        // PlayerPrefs로 로컬 저장 (씬 전환 후 다시 불러오기용)
        PlayerPrefs.SetString("PLAYER_NAME", playerName);
        PlayerPrefs.Save();

        Debug.Log($"[NameEntry] Player Name set: {playerName}");

        // 여기서 '로비 접속' 또는 '방 목록 씬'으로 진행
        // PhotonNetwork.ConnectUsingSettings();  // 아직 접속 전이라면
        // SceneManager.LoadScene("Room_List_Scene");
    }
}
