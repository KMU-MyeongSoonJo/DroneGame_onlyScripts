using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum NPCStatus { Idle, Walk, Talk,  }

public class NPCController : MonoBehaviour
{
    [HideInInspector] Rigidbody rb;
    [HideInInspector] public NavMeshAgent nv;
    [HideInInspector] public Animator anim;
    
    [SerializeField] NPCStatus status;

    GameObject moveTarget;
    GameObject interactiveTarget;
    
    float moveTargetChangeTimer;

    [SerializeField] bool isStayNPC;
    public string NPCId; // NPC 고유번호
    public string questStep; // 본 NPC와의 퀘스트(대화) 진행 정도
    public string maxStep; // 최대 진행 정도

    private void Start()
    {
        status = NPCStatus.Idle;

        rb = GetComponent<Rigidbody>();
        nv = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    public void SetStep(string _step)
    {
        questStep = _step;
    }

    /// <summary>
    /// 매 대화의 끝마다 다음 대화로 진행하기 위해 호출
    /// </summary>
    /// <param name="isReverse">이전 대화로 돌아가기를 원하는 경우</param>
    public void NextStep(bool isReverse = false, int reverseAmount = 1)
    {
        int step = int.Parse(questStep);


        if (isReverse)
        {
            step -= reverseAmount;
        }
        else if (step < int.Parse(maxStep))
            step++;
        questStep = step.ToString("D2");

        GetComponentInChildren<NPCInteractiveController>().ExclamationMarkCheck();
    }

    private void Update()
    {
        switch (status)
        {
            case NPCStatus.Idle:

                if (!isStayNPC)
                    WalkStart();

                break;

            case NPCStatus.Walk:

                float curSpeed = nv.velocity.magnitude / nv.speed;

                //anim.speed = curSpeed;
                anim.SetFloat("Blend", curSpeed);

                // 거의 도착했다면
                //if (nv.velocity.magnitude <= 0.1f || nv.remainingDistance <= 0.2f)
                if (nv.remainingDistance <= 0.2f)
                {
                    moveTarget = null;

                    anim.SetBool("isWalk", false);

                    status = NPCStatus.Idle;
                }

                //// 도착하지 않았는데 정지하는 경우(사람과의 충돌 등)
                //else if ( nv.velocity.magnitude <= 0.5f)
                //{
                //    transform.Translate(Vector3.right * Time.deltaTime * 5, Space.Self);
                //}

                break;

            case NPCStatus.Talk:
                
                Vector3 v = interactiveTarget.transform.position - transform.position;
                v.x = 0;v.z = 0;
                
                break;
        }
    }

    public void WalkStart()
    {
        // 이동 목표 지점 획득
        moveTarget = NPCManager.instance.GetRandomWayPoints();

        anim.SetBool("isWalk", true);

        nv.SetDestination(moveTarget.transform.position);
        nv.isStopped = false;

        status = NPCStatus.Walk;
    }

    // 상호작용 등으로 npc 이동 중지시 호출
    public void WalkStop(GameObject _interactiveTarget, bool isShock = false)
    {
        interactiveTarget = _interactiveTarget;

        // 걷는 애니메이션 중지
        if (!isStayNPC)
        {
            moveTarget = null;
            anim.SetBool("isWalk", false);
            nv.isStopped = true;
        }

        if (isShock)
            anim.SetBool("isShock", true);

        // 상호작용 애니메이션
        else
            anim.SetBool("isTalk", true);

        transform.LookAt(interactiveTarget.transform);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

        status = NPCStatus.Talk;
    }

    /// <summary>
    /// NPC 대화 시작시 호출되는 함수
    /// </summary>
    /// <param name="_interactiveTarget">대화 주체(드론, 플레이어 등)</param>
    public void TalkStart(GameObject _interactiveTarget)
    {

        WalkStop(_interactiveTarget);

        // 대화 시작

        #region 안내 대사 출력 - 정상 대화 진행 불가

        // 아직 튜토리얼이 진행되지 않은 경우.
        // 튜토리얼을 먼저 진행해야 함.
        if (PlayerPrefsData.instance.isTutorialFinish == 0 && gameObject.name != "Examiner")
        {
            print($"[SYSTEM] 튜토리얼이 선행되어야 합니다.");
            TalkManager.instance.TalkStart("Data/Talk/" + "System" + "/" + "System" + "_notice_" + "00");
        }


        // 다른 NPC의 퀘스트가 진행중인 경우.
        // 해당 퀘스트를 먼저 수행해야 함.
        else if(QuestManager.instance.GetCurQuestNpc() != null && gameObject.name != QuestManager.instance.GetCurQuestNpc().name)
        {
            print($"현재 상호작용 NPC : {gameObject.name}");
            print($"[TEST] 퀘스트 중인 NPC : {QuestManager.instance.GetCurQuestNpc().name}");
            TalkManager.instance.TalkStart("Data/Talk/" + "Drone"+ "/" + "Drone"+ "_notice_" + "01");
        }
        
        #endregion

        // 정상 대화 진행 가능. 대상 NPC와 대화 시작.
        else
        {
            print($"[SYSTEM] {gameObject.name} 와의 대화를 진행합니다.");
            TalkManager.instance.TalkStart("Data/Talk/" + gameObject.name + "/" + gameObject.name + "_quest_" + questStep);
        }
    }



    public void TalkStop()
    {

        interactiveTarget = null;

        anim.SetBool("isShock", false);
        anim.SetBool("isTalk", false);

        status = NPCStatus.Idle;
        
    }
    
   
}
