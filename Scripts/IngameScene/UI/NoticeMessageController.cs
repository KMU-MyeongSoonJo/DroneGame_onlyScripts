using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * 
 * 알림 메시지를 일정 시간 이후 제거하는 스크립트
 * 
 * */

public class NoticeMessageController : MonoBehaviour
{
    public GameObject parent;

    float t = 0f;
    public float timer;

    // 활성화와 함께 시간 카운트 시작
    private void OnEnable()
    {
        t = 0f;
    }
    

    private void Update()
    {
        if (timer > 0)
        {
            t += Time.deltaTime;

            if (t > timer)
            {
                // 알림 메시지 종료
                IngameCanvasManager.instance.OffNoticeMessage();
            }
        }
    }
}
