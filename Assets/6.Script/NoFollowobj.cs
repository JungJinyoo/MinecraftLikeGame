using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//오브젝트 풀링에 사용되는 오브젝트를 플레이어에게 상속시키고싶지만, 플레이어를 따라가게 하고싶지는 않을때.

public class NoFollowobj : MonoBehaviour
{

    private Vector3 currentpos;
    private Quaternion currentrot;

    private bool isattach = false;
  
    public void attach(Vector3 pos,Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;

        currentpos = transform.position;
        currentrot = transform.rotation;
        isattach = true;
    }
    // Update is called once per frame
    void LateUpdate()
    {
        if(isattach)
        {
            transform.SetPositionAndRotation(currentpos, currentrot);
        }
    }
}
