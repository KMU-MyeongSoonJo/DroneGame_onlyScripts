using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/************************************
* 
* 설명:
*   오브젝트가 있는 위치에서 인테리어 배치를 방해하는 스크립트 (플레이어, 드론에 사용)
* 
************************************/

public class InteriorPlacementInterferer : MonoBehaviour
{
    private void OnTriggerExit(Collider other)
    {
        if (other.name == "Cursor Cube")
            MyRoomManager.instance.interfererAtCursor = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.name == "Cursor Cube")
            MyRoomManager.instance.interfererAtCursor = true; // 해당 오브젝트가 있는 곳에 가구 배치 불가
    }
}
