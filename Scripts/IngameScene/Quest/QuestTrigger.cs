using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * 
 * [ 임시 ]
 * 
 * 본 트리거를 밟았을 때 특정 Numb의 퀘스트를 호출하기위한 스크립트
 * 추후 NPC와의 대화를 통해 퀘스트 수락 여부를 결정하도록 수정
 * 
 * */

public class QuestTrigger : MonoBehaviour
{
    public int questId;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            print("quest start");
            // call Quest
            //QuestManager.instance.QuestStart(questId);
        }
    }
}
