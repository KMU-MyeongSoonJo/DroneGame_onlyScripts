using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * 
 * 카메라의 일부 기능을 서포트하는 스크립트
 * IngameScene에서 카메라가 벽을 뚫지 못하게 하는 기능 지원
 * 
 * */

public class CameraSupporter : MonoBehaviour
{
    public static CameraSupporter instance;

    Transform target;

    public float maxDistance; // 목표와 카메라간의 최대 거리 (Character View 전용)
    public float height; // 카메라의 높이 (Character View 전용)
    public float smoothSpeed; // 부드러운 카메라 이동을 위한 변수

    private RaycastHit hit; // 카메라 Raycast (벽 감지용). Target에서 this 방향으로 레이를 발사한다.
    
    private void Awake()
    {
        instance = this;
    }

    private void FixedUpdate()
    {
        CameraPosGetter();
    }
    
    /// <summary>
    /// 카메라가 벽을 뚫고 들어가지 않게 보정한다.
    /// </summary>
    public Vector3 CameraPosGetter()
    {
        target = PlayerController.instance.gameObject.transform; // 플레이어 기준으로 카메라 설정
        
        if (Physics.Raycast(target.position, transform.forward * -1, out hit, maxDistance)) // 레이케스트 검사. 무언가에 닿지 않으면 최대 거리에 카메라 설정
        {
            // 무언가에 닿아서 막혔다면
            if (!hit.transform.CompareTag(target.tag))
            {
                Camera.main.transform.position = hit.point; // Ray가 충돌한 위치로 카메라 이동
            }
        }

        // 무엇에도 막히지 않았다면
        else
        {
            Camera.main.transform.position = this.transform.position; // 카메라를 자기 부모 오브젝트의 위치로 이동
        }

        //transform.LookAt(target); // 카메라가 목표를 바라보기

        return Vector3.zero;
    }
    
}

