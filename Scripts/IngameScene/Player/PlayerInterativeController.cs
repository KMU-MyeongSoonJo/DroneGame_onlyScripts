using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * 
 * 플레이어가 NPC 등에 상호작용하기위한 스크립트
 * 
 * */

public class PlayerInterativeController : MonoBehaviour
{
    /// <summary>
    /// 상호작용 대상
    /// </summary>
    [SerializeField] GameObject InteractiveTarget;

    // Interact btn 입력이 연속으로 발생됨을 방지하는 플래그
    bool isInteractBtnDown;

    private void Update()
    {
        TalkSystem();  
    }
    
    /// <summary>
    /// 대화 시작 여부를 판단 및 진행
    /// </summary>
    void TalkSystem()
    {
        // 대화 중에는 X
        // UI 상호작용 중이라면 X
        // MyRoom 씬이라면 X - Axis reset 방지
        // 월드맵 중에는 X
        if (TalkManager.instance.talking ||
            IngameCanvasManager.instance.isCanvasOn ||
            MinimapCameraController.instance.isWorldmap
            ) return;

        // 플레이어 조작 중 상호작용을 시도했을 경우에만 진행
        // 상호작용 버튼 입력
        if (InputDeviceChecker.instance.Interact() && !isInteractBtnDown)
        {
            isInteractBtnDown = true;

            #region MyRoomScene only
            if (SceneManager.GetActiveScene().name == "My Room") // MyRoom 씬에서만 작동
            {
                if (MyRoomManager.instance.menuMode) // 메뉴 선택 모드
                {
                    // 메뉴 선택 중일 때는 버튼이 작동
                }
                else if (MyRoomManager.instance.modifyMode) // 가구 & 트랙 수정 모드
                {
                    if (MyRoomManager.instance.PickedObject) // 오브젝트를 선택중인 경우
                        MyRoomManager.instance.TryPutObject(); // 오브젝트 놓기 시도
                    else // 오브젝트를 선택중이 아닌 경우
                        MyRoomManager.instance.TrySelectObject(); // 오브젝트 선택 시도
                                                                  // OpenModifyMenu(); // 선택할 오브젝트가 없을 때, 가구 수정할 때 전용 메뉴 활성화 (TryPickCube() 함수 내에서 작동)
                }
                else if (InteractiveTarget != null && InteractiveTarget.tag == "Portal") // 포탈이 있을 때
                {
                    // 포탈 사용
                }
                else // 위 조건 모두 불만족시
                    StartCoroutine(RoomUIManager.instance.OpenNormalMenu()); // 메뉴 활성화

                return;
            }
            #endregion

           

            // 대화 상대가 있다면 대화 시작
            if (InteractiveTarget != null)
            {
                if (!InteractiveTarget.CompareTag("Portal") &&
                PlayerController.instance.GetStatus() == PlayerStatus.Control)
                {
                    // 상태 대화중으로 변경
                    PlayerController.instance.setStatus(PlayerStatus.Talk);

                    // 대상 NPC에 상호작용 시작 시그널 전달
                    InteractiveTarget.GetComponent<NPCInteractiveController>().InteractiveStart(gameObject);

                    // 애니메이션 종료
                    PlayerController.instance.ResetAnim();
                }

                // 대화 종료
                else if (PlayerController.instance.GetStatus() == PlayerStatus.Talk)
                {
                    // 상태 조종중으로 변경
                    PlayerController.instance.setStatus(PlayerStatus.Control);

                    // 대상 NPC에 상호작용 종료 시그널 전달
                    InteractiveTarget.gameObject.GetComponent<NPCInteractiveController>().InteractiveStop();
                }
            }

            // 상호작용 상대가 없다면. Ingame UI 메뉴 탭 활성화
            else if (PlayerController.instance.GetStatus() == PlayerStatus.Control)
            {

                // 예기치 못한 동작이 불가하도록 일괄 상태 변경
                // 탭을 닫을 때 최신 시점 정보를 토대로 시점 셋팅
                PlayerController.instance.setStatus(PlayerStatus.Talk);
                PlayerController.instance.dm.SetStatus(DroneStatus.Talk);

                // 플레이어 애니메이션 초기화
                PlayerController.instance.ResetAnim();
                
                Input.ResetInputAxes(); // 버튼 입력 초기화
                IngameCanvasManager.instance.MenuTab(true);

                print($"Player Menu tab open request");
            }
        }

        //else if (Input.GetButtonUp("Interact"))
        else if(InputDeviceChecker.instance.InteractUp())
        {
            isInteractBtnDown = false;
        }
    }

    /// <summary>
    /// 대화 시작/종료를 임의로 호출할 수 있는 함수
    /// </summary>
    /// <param name="isStop"></param>
    public void TalkSystem(bool isStop)
    {
        // 플레이어 조작 중 상호작용을 시도했을 경우에만 진행
        //if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button0)) && InteractiveTarget != null)
        if (!isStop && InteractiveTarget != null)
        {
            // 대화 시작
            if (PlayerController.instance.GetStatus() == PlayerStatus.Control)
            {
                // 상태 대화중으로 변경
                PlayerController.instance.setStatus(PlayerStatus.Talk);

                // 대상 NPC에 상호작용 시작 시그널 전달
                InteractiveTarget.GetComponent<NPCInteractiveController>().InteractiveStart(gameObject);

                // 애니메이션 종료
                PlayerController.instance.ResetAnim();
            }
        }

        // 대화 종료
        else 
        {
            // 플레이어 상태 조종중으로 변경
            PlayerController.instance.setStatus(PlayerStatus.Control);


            // 대상 NPC에 상호작용 종료 시그널 전달
            InteractiveTarget.gameObject.GetComponent<NPCInteractiveController>().InteractiveStop();
        }
    }


    private void OnTriggerStay(Collider other)
    {
        // 상호작용 대상이 아니라면 종료
        // 드론-NPC 충돌 여부 확인을 위한 영역까지 침범했다면 인식X
        // 이미 대화중이라면 종료
        if (!(other.gameObject.layer == LayerMask.NameToLayer("InteractiveArea"))
            || other.CompareTag("InnerInteractiveArea")
            || PlayerController.instance.GetStatus() == PlayerStatus.Talk)
            return;

        // 조작중이라면 상호작용 준비를 위해 대상을 저장
        if (PlayerController.instance.GetStatus() == PlayerStatus.Control)
        {
            if (other.CompareTag("Portal"))
            {
                InteractiveTarget = other.gameObject;
                IngameCanvasManager.instance.OpenNoticeMessage("포탈을 사용하세요.");
            }


            else // NPC
            {
                if (InteractiveTarget == null)
                {
                    InteractiveTarget = other.gameObject;
                    other.GetComponent<NPCInteractiveController>().QuestionMarkOn();
                }

            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 상호작용 대상이 아니라면 동작 X
        if (other.gameObject.layer != LayerMask.NameToLayer("InteractiveArea"))
            return;

        // 대화 중이 아니었다면 상호작용 대상 초기화
        if (PlayerController.instance.GetStatus() != PlayerStatus.Talk)
        {
            if (other.CompareTag("Portal"))
            {
                InteractiveTarget = null;
            }

            else // NPC
            {
                if (other.gameObject == InteractiveTarget)
                {
                    InteractiveTarget = null;
                    other.GetComponent<NPCInteractiveController>().QuestionMarkOff();
                }


            }
            IngameCanvasManager.instance.OffNoticeMessage();
        }

    }
    
}
