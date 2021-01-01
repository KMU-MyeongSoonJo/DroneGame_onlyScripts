using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * 
 * Photographer[03] 퀘스트 진행 과정을 총괄하는 스크립트
 * 
 * */

public class PhotographQuestManager : MonoBehaviour
{
    static public PhotographQuestManager instance;
    
    int leftTargetCount = 3; // 남은 촬영 타겟 개수 카운트. 0이 되면 퀘스트 종료

    private void Start()
    {
        instance = this;
    }


    public void Pass()
    {
        leftTargetCount--;

        // 사진 촬영 이펙트
        IngameCanvasManager.instance.CallPhotographEff();

        switch (leftTargetCount)
        {
            case 2:
                Destroy(GameObject.Find("ACatHomeRange"));
                GameObject.Find("RangeTriggers").transform.Find("BCatHomeRange").gameObject.SetActive(true);
                TalkManager.instance.TalkStart("Data/Talk/Photographer/Photographer_quest_93");
                break;
            case 1:
                Destroy(GameObject.Find("BCatHomeRange"));
                GameObject.Find("RangeTriggers").transform.Find("CatStatueRange").gameObject.SetActive(true);
                TalkManager.instance.TalkStart("Data/Talk/Photographer/Photographer_quest_93");
                break;
            case 0:
                Destroy(GameObject.Find("CatStatueRange"));
                TalkManager.instance.TalkStart("Data/Talk/Photographer/Photographer_quest_94");

                break;
        }
    }

    
}
