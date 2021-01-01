using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * 
 * 퀘스트 진행에 필요한 모든 정보 관리 및 제어
 * 
 * */

// 퀘스트 진행 상태
public enum QuestStatus { empty, doneYet, done }

// public enum TalkStatus { request = 1, agree, disagree, doneYet, done }

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;

    public Dictionary<string, QuestData> questList;


    [Header("Quest elements")]
    public GameObject questTypePassPref;
    public GameObject questTypePassWithRotPref;
    public GameObject questTypeCameraPref;
    public GameObject questTypeActPref;
    public GameObject questTypeWait;
    [Header("Quest supporters")]
    public QuestPointer questPointer;
    public Transform barricatesRoot; // 경로 제한을 위한 오브젝트들 보관소
    public Transform environmentsRoot; // 퀘스트를 위한 특수 오브젝트들 보관소
    [Tooltip("_IngameCanvas/questUI")] public Transform questUIRoot;
    

    [Header("Quest Monitor")]
    // 현재 진행중인 퀘스트 정보
    // 중복 퀘스트 수행은 불가하다고 가정
    [SerializeField] string curQuestId;
    [SerializeField] GameObject curQuestNpc; // 의뢰를 맡긴 npc

    [Header("Tutorial System")]
    public Animator TutorialUIAnimator;
    [SerializeField] int curTutorialProcess;
    [SerializeField] GameObject tutorialYawTargetLeft, tutorialYawTargetRight;

    
    QuestStatus curQuestStatus;
    List<GameObject> curWayPointsList = new List<GameObject>();



    private void Awake()
    {
        instance = this;

        questList = new Dictionary<string, QuestData>();

        QuestGenerater();

        curQuestStatus = QuestStatus.empty;

        NPCManager.instance.cityNpcs.gameObject.SetActive(true);
    }

    private void Start()
    {

        GameObject.Find("Deliver").GetComponent<DeliverMover>().ChangePosition();
    }

    /// <summary>
    /// 현재 진행중인 퀘스트 정보 반환
    /// </summary>
    /// <returns>부제, 보상 등 포함</returns>
    public QuestData GetCurQuestInfo() => questList[curQuestId];
    public GameObject GetCurQuestNpc() => curQuestNpc;
    

    /// <summary>
    /// 퀘스트 수락시 호출
    /// </summary>
    /// <param name="questId">시작할 퀘스트의 id 전달</param>
    /// <param name="timeLimit">제한 시간(단위: 초)</param>
    public void QuestStart(GameObject npc)
    {
        // 드론 착륙 상태일 땐 퀘스트 진행 불가
        if(!PlayerController.instance.dm.IsControlable())
        {
            IngameCanvasManager.instance.OpenNoticeMessage("드론이 착륙상태일 땐 진행할 수 없습니다.", 3);
            
            return;
        }


        #region reset
        QuestCancel();
        #endregion

        #region init data
        //curQuestId = questId;
        curQuestId = npc.GetComponent<NPCController>().NPCId + npc.GetComponent<NPCController>().questStep;

        if (!questList.ContainsKey(curQuestId)) // 해당 step에 퀘스트가 존재하지 않는다면 종료.
        {
            print("Quest " + curQuestId + "는 존재하지 않습니다.");
            curQuestId = null;
            return;
        }
        else if(curTutorialProcess == 4)
        {
            tutorialYawTargetLeft.GetComponent<Renderer>().enabled = true;
            tutorialYawTargetRight.GetComponent<Renderer>().enabled = true;

            tutorialYawTargetLeft.GetComponent<Collider>().enabled = true;
            tutorialYawTargetRight.GetComponent<Collider>().enabled = true;
        }
        else if(curTutorialProcess== 8)
        {
            tutorialYawTargetLeft.GetComponent<Renderer>().enabled = false;
            tutorialYawTargetRight.GetComponent<Renderer>().enabled = false;

            tutorialYawTargetLeft.GetComponent<Collider>().enabled = false;
            tutorialYawTargetRight.GetComponent<Collider>().enabled = false;
        }
        
        curQuestNpc = npc; // 퀘스트 맡긴 npc 정보
        curQuestStatus = QuestStatus.doneYet;

        print("QuestStart [ "+questList[curQuestId].GetSubtitle() + " ]");
        #endregion

        #region set wayPoints

        // 현 퀘스트에 드론의 이동 경로를 제한하는 Barricate가 존재한다면 활성화
        Transform curBarricate = barricatesRoot.Find(curQuestId);
        if (curBarricate != null)
            curBarricate.gameObject.SetActive(true);

        // 현 퀘스트에 활성화되는 특수한 환경 요소가 존재한다면 활성화
        Transform curEnvironment = environmentsRoot.Find(curQuestId);
        if (curEnvironment!= null)
            curEnvironment.gameObject.SetActive(true);

        // wayPoint 설치
        int idx = 0;
        foreach(Location loc in questList[curQuestId].GetWayPoints())
        {
            GameObject obj;
            switch (loc.questType)
            {
                case QuestType.Pass:
                    obj = Instantiate(questTypePassPref, loc.position, Quaternion.Euler(loc.rotation));
                    obj.GetComponent<WayPointController>().setIdx(idx);
                    curWayPointsList.Add(obj);
                    break;

                case QuestType.PassWithRot:
                    obj = Instantiate(questTypePassWithRotPref, loc.position, Quaternion.Euler(loc.rotation));
                    obj.GetComponent<WayPointController>().setIdx(idx);
                    obj.GetComponent<QuestTypePassWithRot>().limitAngleRange = loc.limitAngleRange;
                    curWayPointsList.Add(obj);
                    break;

                case QuestType.Camera:
                    print($"loc.target : {loc.target}");
                    
                    obj = Instantiate(questTypeCameraPref, loc.target.transform);
                    obj.name = loc.target.name;
                    obj.GetComponent<WayPointController>().setIdx(idx);
                    curWayPointsList.Add(obj);
                    break;

                case QuestType.Act:                    
                    obj = Instantiate(questTypeActPref, loc.position, Quaternion.identity);
                    if (loc.isEquipPoint) obj.transform.parent = loc.target.transform;
                    obj.GetComponent<WayPointController>().setIdx(idx);
                    curWayPointsList.Add(obj);
                    break;

                case QuestType.Wait:
                    obj = Instantiate(questTypeWait, loc.position, Quaternion.Euler(loc.rotation));

                    obj.GetComponent<QuestTypeWait>().isFailPossible = loc.isFailPossible;
                    obj.GetComponent<QuestTypeWait>().waitTime = loc.waitTime; // 대기 시간 전달
                    obj.GetComponent<WayPointController>().setIdx(idx);
                    curWayPointsList.Add(obj);
                    break;
            }
            idx++;
        }
        
        // 첫 번째 wayPoint 활성화
        curWayPointsList[0].GetComponent<WayPointController>().Ready();
        TargetFinder.instance.StartFindTarget(curWayPointsList[0]);
        #endregion

        // 퀘스트 시작 안내 문구 출력
        IngameCanvasManager.instance.OpenNoticeMessage("Quest Start !!", 3);

        // 퀘스트 제한 시간
        float timeLimit = questList[curQuestId].GetTimeLimit();
        if (timeLimit != -1)
        {
            Transform t = questUIRoot.Find("TimeLimit");
            t.gameObject.SetActive(true);
            t.GetComponent<QuestTimeLimiter>().TimeLimit = timeLimit;
        }

    }
    /// <summary>
    /// 퀘스트 정상 종료시 자동호출
    /// </summary>
    void QuestDone()
    {
        if (!(curQuestStatus == QuestStatus.done)) return;

        // type 1: 퀘스트 성공 문구 출력 및 NPC에게 직접 돌아가서 대화를 통해 임무 완수
        // type 2: 퀘스트 성공 문구 없이 자동으로 대화가 진행되고 임무가 완수
        //      autoPass가 true일 때 type 2로 진행.

        bool autoPass = false;

        // 퀘스트 성공 문구 출력
        // 튜토리얼의 연속적인 진행 중에는 문구 출력 X`
        if ((PlayerPrefsData.instance.isTutorialFinish == 0 && curTutorialProcess <= 10)
            || curQuestNpc.name == "Deliver" )
        {
            autoPass = true;
        }


        if (!autoPass)
        {
            IngameCanvasManager.instance.OpenNoticeMessage("퀘스트 클리어 !!" +
                "NPC에게 돌아가세요.", 3);

            TargetMarker.instance.SetNewTarget(GameObject.Find(curQuestNpc.name).transform, "퀘스트 클리어 !!");
        }


        #region 보상
        print("Quest Clear !! You get " + questList[curQuestId].GetReward() + " points !!");

        PlayerPrefsData.instance.repute += questList[curQuestId].GetReward();

        #endregion

        curQuestStatus = QuestStatus.empty;

        // 퀘스트 클리어 대화를 진행시키기 위한 구문
        GameObject.Find(curQuestNpc.name).GetComponent<NPCController>().NextStep();

        // 퀘스트 종료시 플레이어 시점으로 전환
        // 튜토리얼 중에는 전환 X
        if (PlayerPrefsData.instance.isTutorialFinish == 1)
            PlayerController.instance.SetViewPoint(1);
        
        //if (PlayerPrefsData.instance.isTutorialFinish == 0 && curTutorialProcess <= 10)
        if(autoPass)
        {
            TalkManager.instance.TalkStart("Data/Talk/" + curQuestNpc.name + "/" + curQuestNpc.name + "_quest_" + curQuestNpc.GetComponent<NPCController>().questStep);
            PlayerController.instance.dm.SetStatus(DroneStatus.Talk);
        }

        // 현 퀘스트에서 드론의 이동 경로를 제한하던 Barricate가 있다면 비활성화
        Transform curBarricate = barricatesRoot.Find(curQuestId);
        if (curBarricate == barricatesRoot) // 이미 비활성화 되어있다면
            return;
        if (curBarricate != null)
            curBarricate.gameObject.SetActive(false);

        // 현 퀘스트에서 특수하게 활성화되던 환경 요소가 있다면 비활성화
        Transform curEnvironment = environmentsRoot.Find(curQuestId);
        if (curEnvironment == environmentsRoot) // 이미 비활성화 되어있다면
            return;
        if (curEnvironment != null)
            curEnvironment.gameObject.SetActive(false);
            

        // 퀘스트 제한 시간 타이머 비활성화
        float timeLimit = questList[curQuestId].GetTimeLimit();
        if (timeLimit != -1)
        {
            Transform t = questUIRoot.Find("TimeLimit");
            t.gameObject.SetActive(false);
        }

        // 생성했던 wayPoints 전부 Destroy
        for (int i = 0; i < curWayPointsList.Count; i++) Destroy(curWayPointsList[i]);
        curWayPointsList.Clear();
        curQuestId = null;
        curQuestNpc = null;
    }

    /// <summary>
    /// 퀘스트 진행 도중 취소시 호출
    /// </summary>
    public void QuestCancel()
    {
        if (curQuestId == null)
            return;

        // TargetFinder.instance.StopFindTarget();

        curQuestStatus = QuestStatus.empty;

        // 드론의 이동 경로를 제한하던 Barricate가 있다면 비활성화
        Transform curBarricate = barricatesRoot.Find(curQuestId);
        if (curBarricate == barricatesRoot) // 이미 비활성화 되어있다면
            return;
        if (curBarricate != null)
            curBarricate.gameObject.SetActive(false);

        // 현 퀘스트에서 특수하게 활성화되던 환경 요소가 있다면 비활성화
        Transform curEnvironment = environmentsRoot.Find(curQuestId);
        if (curEnvironment == environmentsRoot) // 이미 비활성화 되어있다면
            return;
        if (curEnvironment != null)
            curEnvironment.gameObject.SetActive(false);

        // 퀘스트 제한 시간 타이머 비활성화
        float timeLimit = questList[curQuestId].GetTimeLimit();
        if (timeLimit != -1)
        {
            Transform t = questUIRoot.Find("TimeLimit");

            t.gameObject.SetActive(false);
        }

        // 생성했던 wayPoints 전부 Destroy
        for (int i = 0; i < curWayPointsList.Count; i++) Destroy(curWayPointsList[i]);
        curWayPointsList.Clear();
        curQuestId = null;
        curQuestNpc = null;
    }

    /// <summary>
    /// 퀘스트 실패 이벤트 호출
    /// </summary>
    public void QuestFail()
    {
        switch (curQuestId)
        {
            case "0010":
            case "0013":
            case "0016":


                // Fail시 step을 -1 하는 연산이 수행됨
                // 목표 step이 09 이므로 -1 될 것까지 감안해 10으로 설정
                curQuestNpc.GetComponent<NPCController>().SetStep("10");
                break;
        }

        for(int i = 0; i < curWayPointsList.Count; i++)
        {
            if(curWayPointsList[i].activeSelf == true)
            {
                curWayPointsList[i].GetComponent<WayPointController>().Fail();
                return;
            }
        }
    }

    /// <summary>
    /// WayPoint 통과시 호출
    /// </summary>
    /// <param name="idx"></param>
    public void PassWayPoint(int idx)
    {
        // idx wayPoint를 통과했다는 의미.
        // 다음 wayPoint가 있다면 활성화
        // 다음 wayPoint가 없다면 퀘스트 클리어(done)

        TargetFinder.instance.StopFindTarget();

        // 계속 진행
        if (idx < curWayPointsList.Count - 1)
        {
            print("[System] Next Step");

            idx++;
            curWayPointsList[idx].GetComponent<WayPointController>().Ready();
            TargetFinder.instance.StartFindTarget(curWayPointsList[idx]);
        }
        // 클리어. 퀘스트 종료
        else
        {
            print("[System] Quest Clear");

            if (PlayerPrefsData.instance.isTutorialFinish == 0)
            {
                NextTutorialStep();
            }

            curQuestStatus = QuestStatus.done;
            QuestDone();
        }
        
    }

    public void NextTutorialStep()
    {
        switch (curTutorialProcess)
        {
            case 0: // forward
                TutorialUIAnimator.SetBool("RStickUp", true);
                break;
            case 1: // backward
                TutorialUIAnimator.SetBool("RStickUp", false);
                TutorialUIAnimator.SetBool("RStickDown", true);
                break;
            case 2: // left
                //Input.ResetInputAxes();
                PlayerController.instance.dm.DirectDroneRecoverMoment(PlayerController.instance.dm.transform.eulerAngles.y);

                StartCoroutine(DroneFixAxis("z"));

                TutorialUIAnimator.SetBool("RStickDown", false);
                TutorialUIAnimator.SetBool("RStickLeft", true);
                break;

            case 3: // right
                TutorialUIAnimator.SetBool("RStickLeft", false);
                TutorialUIAnimator.SetBool("RStickRight", true);
                break;

            case 4: // up
                //Input.ResetInputAxes();
                PlayerController.instance.dm.DirectDroneRecoverMoment(PlayerController.instance.dm.transform.eulerAngles.y);
                
                TutorialUIAnimator.SetBool("RStickRight", false);
                break;

            case 5:
                TutorialUIAnimator.SetBool("LStickUp", true);
                break;

            case 6: // down
                TutorialUIAnimator.SetBool("LStickUp", false);
                TutorialUIAnimator.SetBool("LStickDown", true);
                break;

            case 7: // turn left
                Input.ResetInputAxes();
                PlayerController.instance.dm.DirectDroneRecoverMoment(PlayerController.instance.dm.transform.eulerAngles.y);

                StartCoroutine(DroneFixAxis("y"));

                TutorialUIAnimator.SetBool("LStickDown", false);
                TutorialUIAnimator.SetBool("LStickLeft", true);
                break;

            case 8: // turn right
                TutorialUIAnimator.SetBool("LStickLeft", false);
                TutorialUIAnimator.SetBool("LStickRight", true);
                break;

            case 9:
                Input.ResetInputAxes();
                PlayerController.instance.dm.DirectDroneRecoverMoment(PlayerController.instance.dm.transform.eulerAngles.y);

                TutorialUIAnimator.SetBool("LStickRight", false);
                break;
        }
        curTutorialProcess++;   
    }

    /// <summary>
    /// 튜토리얼 도중 드론이 정상 정지하지 못하는 경우 발생
    /// 이를 '강제로' 억제하기위한 코루틴
    /// </summary>
    /// <param name="axis">x, y, z</param>
    /// <returns></returns>
    IEnumerator DroneFixAxis(string axis)
    {
        float t = 0;
        float baseAxisValue; // 해당 축의 기준이 되는 값 (수렴할 값)

        if (axis == "x") baseAxisValue = PlayerController.instance.drone.transform.position.x;
        else if (axis == "y") baseAxisValue = PlayerController.instance.drone.transform.position.y;
        else if (axis == "z") baseAxisValue = PlayerController.instance.drone.transform.position.z;
        else baseAxisValue = 0;



        while (t < 3.0f)
        {
            t += Time.deltaTime;

            switch (axis)
            {
                case "x":
                    //baseAxisValue = PlayerController.instance.drone.transform.position.x;

                    PlayerController.instance.drone.transform.position =
                        Vector3.Lerp(
                            PlayerController.instance.drone.transform.position,
                            new Vector3(baseAxisValue, PlayerController.instance.drone.transform.position.y, PlayerController.instance.drone.transform.position.z),
                            0.3f
                            );
                    break;

                case "y":
                    //baseAxisValue = PlayerController.instance.drone.transform.position.y;

                    PlayerController.instance.drone.transform.position =
                        Vector3.Lerp(
                            PlayerController.instance.drone.transform.position,
                            new Vector3(PlayerController.instance.drone.transform.position.x, baseAxisValue, PlayerController.instance.drone.transform.position.z),
                            0.3f
                            );
                    break;

                case "z":
                    //baseAxisValue = PlayerController.instance.drone.transform.position.z;

                    PlayerController.instance.drone.transform.position =
                        Vector3.Lerp(
                            PlayerController.instance.drone.transform.position,
                            new Vector3(PlayerController.instance.drone.transform.position.x, PlayerController.instance.drone.transform.position.y, baseAxisValue),
                            0.3f
                            );

                    break;
            }

            yield return null;
        }
    }


    public int GetCurTutorialProcess() => curTutorialProcess;
    

    /// <summary>
    /// 모든 퀘스트를 저장, 관리 및 생성
    /// 추후 관리의 용이성을 위해 외부 csv 등으로 빼는 것도 고려
    /// </summary>
    private void QuestGenerater()
    {
        questList.Add("0002", new QuestData(
            "Tutorial - throttle and roll",
            new Location[]
            {
                //전
                new Location(new Vector3(0, 2, -108), new Vector3(0, 90, 0)),
                //후
                new Location(new Vector3(0, 2f, -118), new Vector3(0, 90, 0)),
                //좌
                new Location(new Vector3(-6, 2f, -118), new Vector3(0, 0, 0)),
                //우
                new Location(new Vector3(6, 2f, -118), new Vector3(0, 0, 0))
            },
            0)
            );

        questList.Add("0004", new QuestData(
            "Tutorial - yaw and pitch",
            new Location[]
            {
                //위로
                new Location(new Vector3(0, 7f, -118), new Vector3(0, 0, 90)),
                //아래로
                new Location(new Vector3(0, 2f, -118), new Vector3(0, 0, 90)),
                //왼쪽 보기
                new Location(tutorialYawTargetLeft),
                //오른쪽 보기
                new Location(tutorialYawTargetRight),
            },
            0)
            );

        questList.Add("0006", new QuestData(
            "Tutorial - free move",
            new Location[]
            {
                new Location(new Vector3(-10, 5f, -110), new Vector3(0, 90, 0)),
                new Location(new Vector3(0, 10f, -100), new Vector3(0, 0, 0)),
                new Location(new Vector3(10, 5f, -110), new Vector3(0, 90, 0)),
                new Location(new Vector3(0, 2f, -115), new Vector3(0, 0, 0))
            },
            0)
            );

        questList.Add("0010", new QuestData(
            "Hovering flight - 호버링 및 이동",
            new Location[]
            {
                new Location(new Vector3(0, 3, -107.5f), new Vector3(0, -90, 0), 90),
                new Location(new Vector3(-7, 3, -107.5f), new Vector3(0, -90, 0), 90)
            },
            0, 30)
            );

        questList.Add("0013", new QuestData(
            "Triangular flight - 삼각 비행",
            new Location[]
            {
                new Location(new Vector3(0, 1.5f, -115), new Vector3(0, 90, 0)), // 준비
                new Location(new Vector3(0, 1.5f, -110), Vector3.zero, 2, true),
                new Location(new Vector3(0, 6.5f, -110),Vector3.zero, 2, true), // 이륙
                new Location(new Vector3(5, 1.5f, -110),Vector3.zero, 2, true),
                new Location(new Vector3(-5, 1.5f, -110), Vector3.zero,2, true),
                new Location(new Vector3(0, 6.5f, -110), Vector3.zero,2, true),
                //new Location(new Vector3(-5, 1.0f, -115), Vector3.zero,2, true) // 착지
            },
            400, 40)
            );

        questList.Add("0016", new QuestData(
            "Circular flight - 원주 비행",
            new Location[]
            {
                new Location(new Vector3(5.35f, 1.5f, -112.85f), new Vector3(0, 45, 0), 90),
                new Location(new Vector3(7.5f, 1.5f, -107.5f), new Vector3(0, 0, 0), 90),
                new Location(new Vector3(5.35f, 1.5f, -102.15f), new Vector3(0, -45, 0), 90),
                new Location(new Vector3(0, 1.5f, -100), new Vector3(0, -90, 0), 90),
                new Location(new Vector3(-5.35f, 1.5f, -102.15f), new Vector3(0, -135, 0), 90),
                new Location(new Vector3(-7.5f, 1.5f, -107.5f), new Vector3(0, -180, 0), 90),
                new Location(new Vector3(-5.35f, 1.5f, -112.85f), new Vector3(0, -225, 0), 90),
                new Location(new Vector3(0, 1.5f, -115), new Vector3(0, 90, 0), 90),
            },
            400, 40)
            );

        //questList.Add("0009", new QuestData(
        //    "License Test",
        //    new Location[]
        //    {
        //        new Location(new Vector3(0, 1.5f, -115), new Vector3(0, 90, 0)), // 준비
        //        new Location(new Vector3(0, 1.5f, -110), Vector3.zero, 2, true),
        //        new Location(new Vector3(0, 6.5f, -110),Vector3.zero, 2, true), // 이륙
        //        new Location(new Vector3(5, 1.5f, -110),Vector3.zero, 2, true),
        //        new Location(new Vector3(-5, 1.5f, -110), Vector3.zero,2, true),
        //        new Location(new Vector3(0, 6.5f, -110), Vector3.zero,2, true),
        //        new Location(new Vector3(-5, 1.0f, -115), Vector3.zero,2, true) // 착지
        //    },
        //    400)
        //    );

        questList.Add("0100", new QuestData(
            "Delivery",
            new Location[]
            {
                new Location( new Vector3(-66, 18, -157f), new Vector3(0, 90, 0), 2, false)
            },
            500)
            );

        questList.Add("0200", new QuestData(
            "Photograph",
            new Location[]
            {
                new Location(GameObject.Find("[0200]ACat").transform.Find("CameraTarget").gameObject),
                new Location(GameObject.Find("[0200]BCat").transform.Find("CameraTarget").gameObject),
                new Location(GameObject.Find("CatStatue").transform.GetChild(1).GetChild(2).gameObject)
            }
            ,
            100)
            );
        
        
    }
}
