using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * 
 * NPC 주변에서 드론과 플레이어로부터 상호작용 입력을 받아 발생되는
 * 상호작용(대화 등)을 진행하기위한 스크립트
 * 
 * */

public class NPCInteractiveController : MonoBehaviour
{
    NPCController npcc;

    [SerializeField] GameObject interactiveTarget;
    [SerializeField] GameObject questionMark; // 물음표
    [SerializeField] GameObject exclamationMark; // 느낌표
    bool isQuestionMarkOn;

    public GameObject talkDronePos;

    IEnumerator coroutine;

    private void Start()
    {
        npcc = GetComponentInParent<NPCController>();

        ExclamationMarkCheck();
    }
    
    /// <summary>
    /// NPC에 상호작용을 시작함을 알리는 함수
    /// </summary>
    /// <param name="target"> 대화를 거는 주체(Player or Drone)를 전달 </param>
    public void InteractiveStart(GameObject target)
    {
        GetComponentInParent<NPCController>().nv.velocity = Vector3.zero;

        //transform.parent.parent.LookAt(PlayerController.instance.transform);

        //ExclamationOn();
        interactiveTarget = target;
        npcc.TalkStart(interactiveTarget);
    }

    /// <summary>
    /// NPC에 상호작용을 종료함을 알리는 함수
    /// </summary>
    public void InteractiveStop()
    {
        //ExclamationOff();
        interactiveTarget = null;
        npcc.TalkStop();
    }

    /// <summary>
    /// 현재 진행 가능한 퀘스트가 있다면 느낌표 활성화
    /// </summary>
    public void ExclamationMarkCheck()
    {
        string questId = npcc.NPCId + npcc.questStep;
        if (QuestManager.instance.questList.ContainsKey(questId)
            || questId == "0000" // Examiner 최초 튜토리얼
            )
        {
            if (transform.Find("ExclamationMark(Clone)") == null)
            {
                //Instantiate(exclamationMark, transform);
                var v = Instantiate(exclamationMark, Vector3.zero, Quaternion.identity);
                v.transform.parent = transform;
                v.transform.position = transform.position;
            }
        }
        else
        {
            if (transform.Find("ExclamationMark(Clone)") != null)
                Destroy(transform.Find("ExclamationMark(Clone)").gameObject);
        }
    }

    /// <summary>
    /// 대화 가능 거리
    /// 물음표 On
    /// </summary>
    public void QuestionMarkOn()
    {
        if (transform.Find("ExclamationMark(Clone)") != null)
        {
            // 퀘스트 진행이 가능해 느낌표가 떠 있는 경우라면
            // 물음표를 띄우지 않는다.
        }
        

        else if (!isQuestionMarkOn)
        {
            isQuestionMarkOn = true;
            Instantiate(questionMark, transform);
        }
    }

    /// <summary>
    /// 대화 가능 거리를 벗어난 경우
    /// 물음표 Off
    /// </summary>
    public void QuestionMarkOff()
    {
        if (transform.Find("ExclamationMark(Clone)") != null)
        {
            // 퀘스트 진행이 가능해 느낌표가 떠 있는 경우라면
            // 물음표를 띄우지 않는다.
        }

        isQuestionMarkOn = false;

        if (transform.Find("QuestionMark(Clone)") != null)
            Destroy(transform.Find("QuestionMark(Clone)").gameObject);
    }
}
