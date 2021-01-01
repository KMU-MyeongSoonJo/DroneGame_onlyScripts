using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/************************************
* 
* 설명:
*   여기에 적힌 함수들은 대화 엑셀파일(CSV)에서 실행시킬 수 잇습니다.
* 
************************************/

public class TalkTriggers : MonoBehaviour
{
    public static TalkTriggers instance;
    IEnumerator coroutine;

    private void Awake()
    {
        instance = this;
    }

    public void WriteLog(string text) // 로그창에 텍스트 출력
    {
        print(text);
    }

    public void ForceSkip() // 대화 출력 완료하자마자 강제로 다음 대화로 넘기기
    {
        TalkManager.instance.forceSkip = true;
    }


    // .csv 내에서 자주 호출될 함수들
    #region usable


    /// <summary>
    /// 대화를 통해 퀘스트 시작을 호출하는 함수
    /// TalkDone 이전에 호출되어야 한다.
    /// </summary>
    /// <param name="NPCNameAndQuestId">questId + , + NpName</param>
    public void QuestStart(string NPCName)
    {
        //string[] str = NPCNameAndQuestId.Split(',');
        //QuestManager.instance.QuestStart(GameObject.Find(str[0]),int.Parse(str[1]));
        QuestManager.instance.QuestStart(GameObject.Find(NPCName));
    }

    /// <summary>
    /// 플레이어(혹은 드론)의 움직임을 허가하는 함수
    /// </summary>
    /// <param name="isOn">false 전달시, 전혀 움직이지 못하도록 제한.</param>
    public void MoveAllower(string _isOn = "true")
    {

        bool isOn = bool.Parse(_isOn);
        print("isOn : " + isOn);
        // 대화 종료 시그널 전달
        // 플레이어 or 드론
        if (isOn)
        {
            if (PlayerController.instance.GetStatus() == PlayerStatus.Talk)
            {
                PlayerController.instance.GetComponentInChildren<PlayerInterativeController>().TalkSystem(true);
            }
            else if (PlayerController.instance.dm.GetStatus() == DroneStatus.Talk)
            {
                PlayerController.instance.drone.GetComponent<DroneInteractiveController>().TalkSystem(true);
            }
            
        }
        else
        {
            // 대화가 끝나지 않아서 움직일 수 없는 상황.
            // 대화와 대화 사이 애니메이션 등에 사용
            TalkManager.instance.talking = true;
        }

    }
    

    /// <summary>
    /// 대화가 종료될 시점에 호출. 해당 NPC와의 대화 진행 정도를 +1 한다.
    /// 다음 대화 내용을 진행하고자 한다면 반드시 호출되어야 한다.
    /// 본 함수가 호출되지 않으면 같은 대화를 반복한다.
    /// Quest Start 이후 호출되어야 한다.
    /// </summary>
    /// <param name="param">구분자(,)로 파라미터들을 전달.
    /// 1. 상호작용하는 대상 NPC의 이름
    /// 2. 상호작용 주체의 행동 허가 여부를 제어
    /// </param>
    public void TalkDone(string param)
    {
        string[] tmp = param.Split(',');
        string NPCName = tmp[0];
        string isAllowMove = tmp[1];

        if (NPCName != "null")
        {
            // 본 대화에서 진행되어야하는 퀘스트가 있다면 퀘스트 진행
            QuestManager.instance.QuestStart(GameObject.Find(NPCName));
            GameObject.Find(NPCName).GetComponent<NPCController>().NextStep();
        }

        if (bool.Parse(isAllowMove))
            MoveAllower();
    }

    

    /// <summary>
    /// 대화 진행 중 시점 전환이 필요할 때 호출하는 함수
    /// </summary>
    /// <param name="key">키보드의 1234 자판에 대응</param>
    public void SetViewPoint(string key)
    {
        PlayerController.instance.SetViewPoint(int.Parse(key), true);
    }

    /// <summary>
    /// 드론의 Follow 시스템의 활성화/비활성화를 제어
    /// </summary>
    /// <param name="isOn">true: 활성화, false: 비활성화</param>
    public void SetDroneFollowActive(string isOn)
    {
        PlayerController.instance.drone.GetComponent<DroneMovement>().SetDroneFollowActive(bool.Parse(isOn));
    }

    /// <summary>
    /// 안내 문구를 출력하는 재귀함수
    /// </summary>
    /// <param name="param">
    /// "msg",time(float)    // ex. Hello,3
    /// 안내 문구 msg를 time 초 동안 출력
    /// </param>
    public void NoticeMessage(string param)
    {
        string[] tmp = param.Split(',');

        IngameCanvasManager.instance.OpenNoticeMessage(tmp[0], float.Parse(tmp[1]));
    }

    /// <summary>
    /// 카메라 퀘스트 스탭 시작 시점에 호출되어 네비게이션을 노란색으로 바꿔 주는 함수
    /// </summary>
    [ContextMenu("Set Yellow")]
    public void CameraQuestStart()
    {
        QuestManager.instance.questPointer.SetPointerColor(QuestPointer.matColor.YELLOW);
    }

    /// <summary>
    /// 카메라 퀘스트 스탭 종료 시점에 호출되어 네비게이션을 녹색으로 되돌리는 함수
    /// </summary>
    [ContextMenu("Set Green")]
    public void CameraQuestDone()
    {
        QuestManager.instance.questPointer.SetPointerColor(QuestPointer.matColor.GREEN);
    }

    public void RemoveTargetMarker()
    {
        TargetMarker.instance.SetNoTarget();
    }

    #endregion



    // 퀘스트마다 특수한 상황을 구현하기위한 함수.
    // 대화 등에서 직접 호출
    #region specific Quest functions

    #region Drone[x]

    public void Func02(string str)
    {
        bool isTalkStart = bool.Parse(str);

        if (isTalkStart)
        {
            QuestManager.instance.QuestCancel();

            SetViewPoint("1");

            PlayerController.instance.dm.SetStatus(DroneStatus.Rest);
        }
        else
        {
            TalkDone("null,true");

            DataManager.instance.SavePlayerCurPos(new Vector3(95.5f, 0, -297.5f));
            LoadingManager.LoadScene("Decoration Room", "CityScene");
            // 씬 전환?
        }
    }

    #endregion
        
    #region Examiner[00]

    /// <summary>
    /// Drone 첫 등장. 플레이어에게 도달했는지 체크하기위한 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator Func0000(bool isSkip = false)
    {
        GameObject npc = GameObject.Find("Examiner");

        // 안내 메시지
        IngameCanvasManager.instance.OpenNoticeMessage("드론이 접근하고 있습니다.", 5);

        // npc의 머리 위에 위치하도록
        PlayerController.instance.drone.GetComponent<DroneFollowController>().dronePos
            = npc.transform.Find("DronePos").gameObject;

        while (Vector3.Distance(
            npc.transform.position,
            PlayerController.instance.drone.transform.position
            ) > 5f)
        {
            // talking이 true여야 다른 조작의 개입을 막을 수 있다.
            TalkManager.instance.talking = true;
            
            yield return null;
        }

        // 다음 대화 곧바로 진행
        if (!isSkip)
            // 일반 대화
            TalkManager.instance.TalkStart("Data/Talk/Examiner/Examiner_quest_01");

        else
            // 스킵 대화
            TalkManager.instance.TalkStart("Data/Talk/Examiner/Examiner_quest_98");
    }

    public void CallFunc0000(string isSkip)
    {
        bool _isSkip = bool.Parse(isSkip);
        
        StartCoroutine(Func0000(_isSkip));
    }

    /// <summary>
    /// 튜토리얼 퀘스트 시작.
    /// 플레이어와 드론 위치 변경 및 시점 전환
    /// </summary>
    public void Func0001()
    {
        // 플레이어 위치
        PlayerController.instance.transform.position = new Vector3(-2, 0, -120);
        PlayerController.instance.transform.eulerAngles = new Vector3(0, 0, 0);

        // 드론 위치
        PlayerController.instance.dm.transform.position = new Vector3(0, 2, -118);
        PlayerController.instance.dm.DirectDroneRecoverMoment(0);

        // 드론 및 플레이어 상태 조정
        PlayerController.instance.dm.GetComponent<DroneFollowController>().StopRecall();
        PlayerController.instance.setStatus(PlayerStatus.Stay);
        PlayerController.instance.dm.SetStatus(DroneStatus.Control);


        // 플레이 시점
        SetViewPoint("3");

        TalkManager.instance.TalkStart("Data/Talk/Examiner/Examiner_quest_02");
    }

    public void Func0004()
    {
        // 튜토리얼 준비
        GameObject.Find("TutorialCube").GetComponent<Collider>().enabled = true;
        GameObject.Find("TutorialCube(1)").GetComponent<Collider>().enabled = true;
        //GameObject.Find("TutorialCube").GetComponent<Renderer>().enabled = true;
        //GameObject.Find("TutorialCube(1)").GetComponent<Renderer>().enabled = true;

        // 플레이어 위치
        PlayerController.instance.transform.position = new Vector3(-2, 0, -122);
        PlayerController.instance.transform.eulerAngles = new Vector3(0, 0, 0);

        // 드론 위치
        PlayerController.instance.dm.transform.position = new Vector3(0, 2, -118);
        PlayerController.instance.dm.DirectDroneRecoverMoment(0);

        SetViewPoint("3");
    }

    public void Func0006()
    {
        // 튜토리얼 준비
        GameObject.Find("TutorialCube").GetComponent<Collider>().enabled = false;
        GameObject.Find("TutorialCube(1)").GetComponent<Collider>().enabled = false;
        GameObject.Find("TutorialCube").GetComponent<Renderer>().enabled = false;
        GameObject.Find("TutorialCube(1)").GetComponent<Renderer>().enabled = false;


        // 플레이어 위치
        PlayerController.instance.transform.position = new Vector3(-2, 0, -120);
        PlayerController.instance.transform.eulerAngles = new Vector3(0, 0, 0);

        // 드론 위치
        PlayerController.instance.dm.transform.position = new Vector3(0, 2, -118);
        PlayerController.instance.dm.DirectDroneRecoverMoment(0);

        SetViewPoint("3");
    }

    /// <summary>
    /// 튜토리얼 종료.
    /// </summary>
    public void Func0008()
    {
        // 드론이 플레이어를 따라다니도록 설정
        PlayerController.instance.drone.GetComponent<DroneFollowController>().dronePos
        = PlayerController.instance.dronePos;
        PlayerController.instance.drone.GetComponent<DroneFollowController>().followFlag = true;

        // 튜토리얼 종료 시그널 전달
        // 드론으로 시점 변환이 가능
        PlayerPrefsData.instance.isTutorialFinish = 1;
        PlayerPrefs.SetInt("isTutorialFinish", 1);
    }

    /// <summary>
    /// 반복 퀘스트
    /// </summary>
    public void Func0009(string goStep)
    {
        string NpcName = "Examiner";

        if (goStep == "-1")
        {
            MoveAllower(); // 이동 허가
        }
        else if(goStep == "9")
        {
            goStep = "09";

            // goStep 대사로 연결
            GameObject.Find(NpcName).GetComponent<NPCController>().SetStep(goStep);
            MoveAllower(); // 이동 허가
        }
        else
        {
            // goStep 대사로 연결
            var npc = GameObject.Find(NpcName).GetComponent<NPCController>();
            npc.SetStep(goStep);

            // 바로 대사로 연결
            TalkManager.instance.TalkStart("Data/Talk/Examiner/Examiner_quest_" + npc.questStep);
            //PlayerController.instance.dm.SetStatus(DroneStatus.Talk);
        }
    }
    
    /// <summary>
    /// 호버링 및 이동 플레이어 및 드론 위치 설정
    /// </summary>
    public void Func0010()
    {
        // 드론 착륙 상태일 땐 퀘스트 진행 불가
        if (!PlayerController.instance.dm.IsControlable())
        {
            PlayerController.instance.SetViewPoint(1, true, true);
            GameObject.Find("Examiner").GetComponent<NPCController>().NextStep(true);
            
            IngameCanvasManager.instance.OpenNoticeMessage("드론이 착륙상태일 땐 진행할 수 없습니다.", 3);
            return;
       }

        // 플레이어 위치
        PlayerController.instance.transform.position = new Vector3(0, 0, -120);
        PlayerController.instance.transform.eulerAngles = new Vector3(0, 0, 0);

        // 드론 위치
        PlayerController.instance.dm.transform.position = new Vector3(8, 3f, -107.5f);
        int yAngle = Random.Range(0, 360);
        PlayerController.instance.dm.DirectDroneRecoverMoment(yAngle);

        // 드론 회전(랜덤)
        //PlayerController.instance.dm.transform.eulerAngles = new Vector3(0, yAngle, 0);
        PlayerController.instance.drone.transform.Rotate(new Vector3(0, yAngle, 0));

        PlayerController.instance.SetViewPoint(2, true, true);

        TalkDone("Examiner,true");
    }

    /// <summary>
    /// 삼각 비행 플레이어 및 드론 위치 설정
    /// </summary>
    public void Func0013()
    {
        // 드론 착륙 상태일 땐 퀘스트 진행 불가
        if (!PlayerController.instance.dm.IsControlable())
        {
            PlayerController.instance.SetViewPoint(1, true, true);
            GameObject.Find("Examiner").GetComponent<NPCController>().NextStep(true, 4);
            IngameCanvasManager.instance.OpenNoticeMessage("드론이 착륙상태일 땐 진행할 수 없습니다.", 3);
            return;
        }

        // 플레이어 위치
        PlayerController.instance.transform.position = new Vector3(0, 0, -120);
        PlayerController.instance.transform.eulerAngles = new Vector3(0, 0, 0);

        // 드론 위치
        PlayerController.instance.dm.transform.position = new Vector3(0, 1.5f, -115);
        PlayerController.instance.dm.DirectDroneRecoverMoment(0);

        PlayerController.instance.SetViewPoint(2, true, true);

        TalkDone("Examiner,true");
    }

    /// <summary>
    /// 원주 비행 플레이어 및 드론 위치 설정
    /// </summary>
    public void Func0016()
    {
        // 드론 착륙 상태일 땐 퀘스트 진행 불가
        if (!PlayerController.instance.dm.IsControlable())
        {
            PlayerController.instance.SetViewPoint(1, true, true);
            GameObject.Find("Examiner").GetComponent<NPCController>().NextStep(true, 7);
            IngameCanvasManager.instance.OpenNoticeMessage("드론이 착륙상태일 땐 진행할 수 없습니다.", 3);
            return;
        }

        // 플레이어 위치
        PlayerController.instance.transform.position = new Vector3(0, 0, -120);
        PlayerController.instance.transform.eulerAngles = new Vector3(0, 0, 0);

        // 드론 위치
        PlayerController.instance.dm.transform.position = new Vector3(0, 1.5f, -115);
        PlayerController.instance.dm.DirectDroneRecoverMoment(0);

        PlayerController.instance.SetViewPoint(2, true, true);

        TalkDone("Examiner,true");
    }

    /// <summary>
    /// 튜토리얼 스킵 대화 이후 진행될 step 설정
    /// </summary>
    public void Func0098()
    {
        GameObject.Find("Examiner").GetComponent<NPCController>().SetStep("09");
    }


    ///// <summary>
    ///// 마을 순찰 임무 수행
    ///// </summary>
    ///// <param name="param">선택지 idx. 0 or 1 </param>
    //public void Func0099(string param)
    //{
    //    TalkDone("Examiner,false");

    //    int idx = int.Parse(param);

    //    if (idx == 0) TalkManager.instance.TalkStart("Data/Talk/Examiner/Examiner_quest_05");
    //    else if (idx == 1) TalkManager.instance.TalkStart("Data/Talk/Examiner/Examiner_quest_08");
    //}

    #endregion

    #region Deliver[01]


    /// <summary>
    /// 배달 퀘스트 취소 or 시작
    /// 취소: npc 자리 변경
    /// 시작: 피자 장착
    /// </summary>
    public void Func0100(string isStart)
    {
        // 시작
        if (bool.Parse(isStart))
        {
            // 드론 착륙 상태일 땐 퀘스트 진행 불가
            if (!PlayerController.instance.dm.IsControlable())
            {
                PlayerController.instance.SetViewPoint(1, true, true);
                GameObject.Find("Deliver").GetComponent<NPCController>().NextStep(true);
                IngameCanvasManager.instance.OpenNoticeMessage("드론이 착륙상태일 땐 진행할 수 없습니다.", 3);
                return;
            }
            
            // equip pizza box
            GameObject obj = Instantiate(Resources.Load<GameObject>("Prefabs/Quest/Obj/PIzzaBox"),
                PlayerController.instance.dm.transform.Find("EquipArea")
                );

            obj.transform.localPosition = Vector3.zero;

            // start hp check coroutine
            //StartCoroutine(HpChecker());
            coroutine = HpChecker();
            StartCoroutine(coroutine);
        }

        // 취소
        else
        {
            ChangeDeliverPosition();
            MoveAllower("TRUE");
        }
    }

    /// <summary>
    /// NPC의 위치를 변경.
    /// 퀘스트 거절 / 완수시 Deliver NPC는 위치를 변경한다.
    /// </summary>
    public void ChangeDeliverPosition()
    {
        GameObject.Find("Deliver").GetComponent<DeliverMover>().ChangePosition();
    }

    /// <summary>
    /// 피자 삭제
    /// 배달 성공 or 실패
    /// </summary>
    public void DestroyPizza()
    {
        if(PlayerController.instance.dm.transform.Find("EquipArea").GetChild(0) != null)
        {
            //StopCoroutine(coroutine);
            coroutine = null;

            Destroy(PlayerController.instance.dm.transform.Find("EquipArea").GetChild(0).gameObject);
        }
    }

    /// <summary>
    /// 피자 배달 퀘스트 진행 중 충돌 정도를 모니터링하는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator HpChecker()
    {
        float hp = PlayerController.instance.dm.droneHp;
        float limitHp = hp - 1000 < 0 ? 0 : hp - 1000;

        
        // 퀘스트 시작까지 잠시 대기
        while(QuestManager.instance.GetCurQuestNpc() == null)
        { yield return null; }

        // 방해 비둘기 배치
        Vector3 obstaclePos = QuestManager.instance.GetCurQuestInfo().GetWayPoints()[0].position;
        Vector3 obstacleRot = QuestManager.instance.GetCurQuestInfo().GetWayPoints()[0].rotation;

        //print($"[TEST] waypoint pos : {obstaclePos}");
        //obstaclePos.y *= 0.5f;
        GameObject obj = Resources.Load<GameObject>("Prefabs/Quest/Obj/ChickenRoot");
        List<GameObject> obstacleList = new List<GameObject>();

        for(int i=10;i<obstaclePos.y; i+=10)
        {
            obj = Instantiate(obj,
                new Vector3(obstaclePos.x, i, obstaclePos.z),
                Quaternion.Euler(obstacleRot)
                );
            obstacleList.Add(obj);
        }

        PlayerController.instance.dm.SetWindy(new Vector3(Random.Range(-50, 50), 0, Random.Range(-50, 50)));

        while (coroutine != null)
        {
            if(PlayerController.instance.dm.droneHp <= limitHp)
            {
                // fail
                GameObject.FindObjectOfType<WayPointController>().Fail();
                break;
            }



            yield return null;
        }

        PlayerController.instance.dm.ResetWindy();

        for(int i = 0; i < obstacleList.Count; i++)
        {
            Destroy(obstacleList[i]);
        }
        obstacleList.Clear();

    }

    /// <summary>
    /// 퀘스트 실패.
    /// 퀘스트 취소 및 npc 위치 변경 필요
    /// </summary>
    public void Func0199()
    {
        print("Func 0199 call");

        // 퀘스트 취소
        QuestManager.instance.QuestCancel();

        // npc 이동
        ChangeDeliverPosition();
    }

    #endregion

    #region Photographer[02]
    
    void Func0200()
    {
        // 드론 착륙 상태일 땐 퀘스트 진행 불가
        if (!PlayerController.instance.dm.IsControlable())
        {
            PlayerController.instance.SetViewPoint(1, true, true);
            GameObject.Find("Photographer").GetComponent<NPCController>().NextStep(true);
            IngameCanvasManager.instance.OpenNoticeMessage("드론이 착륙상태일 땐 진행할 수 없습니다.", 3);
            return;
        }

        Instantiate(Resources.Load<GameObject>("Prefabs/Quest/Obj/PhotographQuest"));
    }

    void Func0202()
    {
        Destroy(GameObject.Find("PhotographQuest(Clone)"));
    }

    #endregion
    
    #endregion

}
