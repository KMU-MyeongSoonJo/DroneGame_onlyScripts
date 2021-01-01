using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * 
 * 드론이 NPC 등에 상호작용하기위한 스크립트
 * 
 * */

public class DroneInteractiveController : MonoBehaviour
{
    public DroneMovement dm;

    /// <summary>
    /// 상호작용 대상
    /// </summary>
    [SerializeField] GameObject InteractiveTarget;

    // Interact btn 입력이 연속으로 발생됨을 방지하는 플래그
    bool isInteractBtnDown;

    private void Update()
    {
        TalkSystem();

        if (InteractiveTarget)
        { }

    }

    /// <summary>
    /// 대화 시작 여부를 판단 및 진행
    /// </summary>
    public void TalkSystem()
    {
        // 대화 중에는 x, UI 상호작용 중이라면 X
        if (TalkManager.instance.talking || IngameCanvasManager.instance.isCanvasOn)
        {
            return;
        }

        //if (InputDeviceChecker.instance.Interact() && InteractiveTarget != null)
        if(InputDeviceChecker.instance.Interact() && !isInteractBtnDown)
        {
            isInteractBtnDown = true;

            // 대화 상대가 있다면 대화 시작
            if (InteractiveTarget != null)
            {
                if (!InteractiveTarget.CompareTag("Portal")&&
                    dm.GetStatus() == DroneStatus.Control)
                {
                    // 상태 대화중으로 변경
                    dm.SetStatus(DroneStatus.Talk);

                    // 대상 NPC에 상호작용 시작 시그널 전달
                    InteractiveTarget.GetComponent<NPCInteractiveController>().InteractiveStart(gameObject);

                    // 기체 기울어짐 등 회복
                    dm.DirectDroneRecoverMoment(transform.eulerAngles.y);
                    // 상하 움직임 초기화
                    dm.MovementUpDown(true);
                }

                // 대화 종료
                else if (dm.GetStatus() == DroneStatus.Talk)
                {
                    // 상태 조종중으로 변경
                    dm.SetStatus(DroneStatus.Control);

                    // 대상 NPC에 상호작용 종료 시그널 전달
                    InteractiveTarget.gameObject.GetComponent<NPCInteractiveController>().InteractiveStop();
                }
            }

            //  상호작용 상대가 없다면. Ingame UI 메뉴 탭 활성화
            else if(dm.GetStatus() == DroneStatus.Control
                || dm.GetStatus() == DroneStatus.Landing
                || dm.GetStatus() == DroneStatus.Rest)
            {
                // 예기치 못한 동작이 불가하도록 일괄 상태 변경
                // 탭을 닫을 때 최신 시점 정보를 토대로 시점 셋팅
                PlayerController.instance.setStatus(PlayerStatus.Talk);
                PlayerController.instance.dm.SetStatus(DroneStatus.Talk);

                // 기체 기울어짐 등 회복
                dm.DirectDroneRecoverMoment(transform.eulerAngles.y);
                // 상하 움직임 초기화
                dm.MovementUpDown(true);


                Input.ResetInputAxes(); // 버튼 입력 초기화
                IngameCanvasManager.instance.MenuTab(true);

                print("Drone Menu tab open request");
            }
        }
        //else if(Input.GetButtonUp("Interact"))
        else if(InputDeviceChecker.instance.InteractUp())
        {
            isInteractBtnDown = false;
        }
    }

    public void TalkSystem(bool isStop)
    {
        print($"y : {PlayerController.instance.drone.transform.eulerAngles.y}");

        if (!isStop)
        {
            // 대화 시작
            if (dm.GetStatus() == DroneStatus.Control)
            {
                // 상태 대화중으로 변경
                dm.SetStatus(DroneStatus.Talk);

                // 대상 NPC에 상호작용 시작 시그널 전달
                InteractiveTarget.GetComponent<NPCInteractiveController>().InteractiveStart(gameObject);
                
                // 기체 기울어짐 등 회복
                dm.DirectDroneRecoverMoment(transform.eulerAngles.y);
                // 상하 움직임 초기화
                dm.MovementUpDown(true);
            }
        }
    
        else
        {
            // 상태 조종중으로 변경
            dm.SetStatus(DroneStatus.Control);

            // 대상 NPC에 상호작용 종료 시그널 전달
            if (InteractiveTarget != null)
            {
                print($"TALK STOP with [{InteractiveTarget.transform.parent.parent.name}]");
                InteractiveTarget.gameObject.GetComponent<NPCInteractiveController>().InteractiveStop();
            }
        }

        print($"y : {PlayerController.instance.drone.transform.eulerAngles.y}");
    }


    private void OnTriggerEnter(Collider other)
    {
        // NPC와 직접 충돌

        // 대화 중에는 발생 X
        //if (dm.getStatus() != DroneStatus.Talk && other.CompareTag("InnerInteractiveArea"))
        if (dm.GetStatus() == DroneStatus.Talk)
            return;

        if(other.CompareTag("InnerInteractiveArea"))
        {
            // 대상 NPC Interactive target으로 지정
            InteractiveTarget = other.transform.parent.GetComponentInChildren<NPCInteractiveController>().gameObject;

            // 주의 문구 출력
            TalkManager.instance.TalkStart("Data/Talk/Drone/Drone_notice_00");
            PlayerController.instance.dm.SetStatus(DroneStatus.Talk);

            //// NPC 정지
            //// 대상 NPC에 상호작용 시작 시그널 전달
            //other.transform.parent.GetComponentInParent<NPCController>().WalkStop(gameObject, true);

            // 플레이어 시점으로 전환
            PlayerController.instance.SetViewPoint(1, true);

            return;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // 상호작용 대상이 아니라면 종료
        // 이미 대화중이라면 종료
        // 상호작용 대상이 근처에 있다면 대상 중복 탐색 방지
        if (!(other.gameObject.layer == LayerMask.NameToLayer("InteractiveArea"))
            || dm.GetStatus() == DroneStatus.Talk
            || InteractiveTarget != null )
            return;



        // 조작중이라면 상호작용 준비를 위해 대상을 저장
        if (dm.GetStatus() == DroneStatus.Control)
        {
            InteractiveTarget = other.gameObject;
            other.GetComponent<NPCInteractiveController>().QuestionMarkOn();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 상호작용 대상이 아니라면 동작 X
        // NPC 내 드론 충돌 체크 트리거라면 동작 X - '대화'와 관련 X
        if (other.gameObject.layer != LayerMask.NameToLayer("InteractiveArea")
            || other.CompareTag("InnerInteractiveArea"))
            return;

        // 대화 중이 아니었다면 상호작용 대상 초기화
        if (dm.GetStatus() != DroneStatus.Talk)
        {
            InteractiveTarget = null;
            other.GetComponent<NPCInteractiveController>().QuestionMarkOff();
        }

    }
}
