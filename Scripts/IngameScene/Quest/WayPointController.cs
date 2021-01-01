using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * 
 * 통과, 정지, 사진 찍기 등 각 퀘스트 이벤트 타입 프리팹에 모두 포함되는,
 * 기본 WayPoint 속성들을 가진 스크립트
 * 
 * */

public class WayPointController : MonoBehaviour
{
    int idx;
    bool isEnable;

    public GameObject[] edgs;

    public Material enable, disable, stay;

    private void Awake()
    {
        isEnable = false;
        foreach (GameObject v in edgs)
            v.GetComponent<Renderer>().material = disable;
    }

    public void setIdx(int i) => idx = i;

    public bool getIsEnable() => isEnable;

    // 활성화
    public void Ready()
    {
        isEnable = true;
        foreach (GameObject v in edgs)
            v.GetComponent<Renderer>().material = enable;

        if (PlayerPrefsData.instance.isTutorialFinish == 0)
            QuestManager.instance.NextTutorialStep();
    }

    public void Stay()
    {
        foreach (GameObject v in edgs)
            v.GetComponent<Renderer>().material = stay;
    }

    public void Exit()
    {
        foreach (GameObject v in edgs)
            v.GetComponent<Renderer>().material = enable;
    }

    // 클리어
    public void Clear() {
        
        // 본 wayPoints 비활성화
        isEnable = false;
        foreach (GameObject v in edgs)
            v.GetComponent<Renderer>().material = disable;

        // 비활성화?
        gameObject.SetActive(false);


        #region specific clear process

        /**
         * 
         * 각 WayPoint 통과시 특별한 과정이 필요한 경우.
         * 
         * */

        // 사진작가 퀘스트는 각 오브젝트 촬영시마다 남은 타겟의 개수를 알려주어야 한다.
        if(QuestManager.instance.GetCurQuestNpc().name == "Photographer")
        {
            PhotographQuestManager.instance.Pass();
        }

        #endregion

        // 다음 wayPoint 진행
        QuestManager.instance.PassWayPoint(idx);
        
    }

    // 실패
    public void Fail()
    {
        // 퀘스트 Fail 대화 진행
        TalkManager.instance.TalkStart("Data/Talk/"
            + QuestManager.instance.GetCurQuestNpc().name + "/"
            + QuestManager.instance.GetCurQuestNpc().name
            + "_quest_99");

        // 대화정보(step) 롤백
        GameObject.Find(QuestManager.instance.GetCurQuestNpc().name).GetComponent<NPCController>().NextStep(true);

        // 실패시 드론 자동 복귀
        PlayerController.instance.SetViewPoint(1, true);

        QuestManager.instance.QuestCancel();
    }
}
