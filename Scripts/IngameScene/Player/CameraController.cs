using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/************************************
* 
* 버전: 
* 작성자: 진민준     최초 작성날짜:             최근 수정날짜:       
* 설명:
*   플레이어 기준 시점을 다루는 스크립트
*   플레이어 이동, 드론 이동(플레이어 시점) 두 상황을 제어
* 
************************************/

public class CameraController : MonoBehaviour
{
    public GameObject player;
    public GameObject drone;

    GameObject target; // 바라보는 대상?
   
    public bool isLookDrone = false; // 플레이어 시점 드론 조작시

    private void FixedUpdate()
    {
        transform.position = player.transform.position;

        if (isLookDrone) // 플레이어 시점 드론 조작
        {
            target = drone;
            transform.LookAt(target.transform);
        }
        else // 플레이어 조작
        {
            target = player;
            transform.eulerAngles = player.transform.eulerAngles;
        }
    }
    
}
