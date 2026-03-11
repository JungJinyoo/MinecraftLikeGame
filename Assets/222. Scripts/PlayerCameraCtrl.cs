using System.Collections;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class PlayerCameraCtrl : MonoBehaviourPun
{
    [SerializeField] private Camera playerCam;
    [SerializeField] private RawImage miniMapImage;

    private RenderTexture myRenderTex;

    IEnumerator Start()
    {
        yield return null; //  씬 초기화 후 실행 (중요)

        if (!photonView.IsMine)
        {
            if (playerCam != null)
                playerCam.enabled = false;
            yield break;
        }

        myRenderTex = new RenderTexture(2048, 2048, 24, RenderTextureFormat.ARGB32);
        myRenderTex.name = "RT_" + PhotonNetwork.LocalPlayer.ActorNumber;

        playerCam.targetTexture = myRenderTex;

        if (miniMapImage != null)
            miniMapImage.texture = myRenderTex;

        playerCam.enabled = true;
    }

    void OnDestroy()
    {
        if (myRenderTex != null)
        {
            myRenderTex.Release();
            Destroy(myRenderTex);
            myRenderTex = null;
        }
    }
}
