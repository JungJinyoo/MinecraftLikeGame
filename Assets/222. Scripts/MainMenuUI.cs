using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.Collections;

public class MainMenuUI : MonoBehaviour
{
    public GameObject pnlLOBY;
    public GameObject pnlOption;
    private bool OptionOpen = false;
    public GameObject pnlMusicSounds;
    public bool MusicSoundsOpen = false;
    public void OnClickSingle()
    {
        PhotonNetwork.OfflineMode = true; // �̱� ���
        SceneManager.LoadScene("Minecrafttest");
    }
    IEnumerator Loading()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("RoomList");
    }

    public void OnClickMulti()
    {
        PhotonNetwork.OfflineMode = false; // �¶���
        StartCoroutine(Loading());
    }

    //���������߰�
    public void QuitGame()
    {
        Debug.Log("���� ����");
        Application.Quit();

        // ����Ƽ �����Ϳ��� �׽�Ʈ�� ���� ���� ���ᰡ �� �ǹǷ�:
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    //���� �߰�
    public void OnClickOption()
    {
        pnlLOBY.SetActive(false);
        pnlOption.SetActive(true);
        OptionOpen = true;
    }

    public void OnClickDone()
    {
        if (OptionOpen)
        {
            if(pnlOption!=null&&pnlLOBY!=null)
            {
            pnlOption.SetActive(false);
            OptionOpen = false;
            pnlLOBY.SetActive(true);
            }
        }
        else if (MusicSoundsOpen)
        {
            pnlMusicSounds.SetActive(false);
            MusicSoundsOpen = false;
            pnlOption.SetActive(true);
            OptionOpen = true;
        }

    }
    // ���� �� �Ҹ�...
    public void OnClickMusicSounds()
    {
        if(pnlOption!=null&&pnlLOBY!=null)
        {
        pnlOption.SetActive(false);
        OptionOpen = false;
        }
        pnlMusicSounds.SetActive(true);
        MusicSoundsOpen = true;
    }
}
