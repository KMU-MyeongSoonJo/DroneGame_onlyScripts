using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * 
 * 
 * 
 * */

public class NPCManager : MonoBehaviour
{
    public static NPCManager instance;
    
    public GameObject[] wayPoints;

    // NPC와 대화할 때 표시되는 캔버스
    public Canvas talkCanvas;

    // Playable Scene Load 직후 활성화된다.
    // QuestManager가 활성화된 이후 NPC별 Exclamation 활성화 여부를 확인하기 위함
    public Transform cityNpcs;

    private void Awake()
    {
        instance = this;

        cityNpcs.gameObject.SetActive(false);
    }

    // 랜덤한 위치를 하나 반환
    public GameObject GetRandomWayPoints()
    {
        return wayPoints[Random.Range(0, wayPoints.Length)];
        //return wayPoints[Random.Range(0, 2)];
    }

    // npc와 대화 시작시 드론 시점 카메라에 대화 ui를 띄우는 함수
    public void CallTalkCanvas(bool isOn)
    {
        talkCanvas.enabled = isOn;
    }
}
