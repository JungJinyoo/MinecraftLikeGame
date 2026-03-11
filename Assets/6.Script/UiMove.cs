using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiMove : MonoBehaviour
{
    public Camera renderCam;
    public Camera mainCam;
    // Start is called before the first frame update

    void LateUpdate()
    {
        renderCam.transform.rotation = mainCam.transform.rotation;
    }
}
