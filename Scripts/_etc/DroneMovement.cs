using DroneController.Physics;
using UnityEngine;

// 드론의 조작 상태
public enum DroneStatus {  Control, Recall, Talk, Trouble, Landing, Rest }

public class DroneMovement : DroneMovementScript
{
    public DroneFollowController DFC;
    public DronePropelers DP;

    // 현재 드론의 상태(enum)
    public DroneStatus status;

    public float __droneInitHp__ = 5000;
    public float _droneHp = 5000;
    public float droneHp
    {
        set {
            _droneHp = value < 0 ? 0 : value;

            if(_droneHp <= 0) //&& _droneHp != -1000)
            {
                TalkManager.instance.TalkStart("Data/Talk/Drone/Drone_notice_02");
            }
        }
        get { return _droneHp; }
    }

    public override void Awake()
    {
        base.Awake(); //I would suggest you to put code below this line or in a Start() method

        Debug.Assert(DFC != null);
        Debug.Assert(DP != null);

        status = DroneStatus.Trouble;
    }

    void FixedUpdate()
    {

        GetVelocity();
        ClampingSpeedValues();
        SettingControllerToInputSettings(); //sensitivity settings for joystick,keyboard,mobile (depending on which is turned on)

        if (FlightRecorderOverride == false)
        {
            //  메뉴 UI 작동중에는 조작 불가능
            if (IngameCanvasManager.instance.isCanvasOn)
            {
                BasicDroneHoverAndRotation();
                return;
            }

            // '컨트롤' 상태일 때에만 조작.
            // 아닐 경우에는 호버링
            switch (status)
            {
                case DroneStatus.Control:

                    #region tutorial movement
                    if (PlayerPrefsData.instance.isTutorialFinish == 0)
                    {
                        int tutorialProcess = QuestManager.instance.GetCurTutorialProcess();

                        // 조작 제한 튜토리얼
                        if (tutorialProcess == 0) { }
                        else if (tutorialProcess <= 2)
                        {
                            MovementForward();
                        }
                        else if (tutorialProcess <= 4)
                        {
                            MovementForward(true);
                            MovementLeftRight();
                        }
                        else if (tutorialProcess <= 7)
                        {
                            MovementUpDown();
                        }
                        else if (tutorialProcess <= 9)
                        {
                            MovementUpDown(true);
                            Rotation();
                        }

                        // 자유 이동 튜토리얼
                        else
                        {
                            MovementUpDown();
                            MovementLeftRight();
                            Rotation();
                            MovementForward();
                        }
                    }
                    #endregion

                    else
                    {
                        MovementUpDown();
                        MovementLeftRight();
                        Rotation();
                        MovementForward();
                    }

                    // 바람 영향.
                    // 조종 중에만 영향을 받음.
                    AddWindyForce();

                    if (LandingCheck())
                    {
                        BeginLanding();
                        status = DroneStatus.Landing;
                    }

                    break;

                case DroneStatus.Recall:
                    ResetDroneVelocity();

                    break;

                case DroneStatus.Talk:
                    MovementUpDown(true);
                    ResetDroneVelocity();

                    break;

                case DroneStatus.Trouble:

                    MovementUpDown(true);
                    DirectDroneRecoverMoment(transform.eulerAngles.y);

                    if (PlayerPrefsData.instance.isTutorialFinish == 1)
                        RecoverTrouble();

                    break;
                case DroneStatus.Landing:

                    if (PlayerController.instance.GetStatus() != PlayerStatus.Stay)
                        return;

                    MovementLeftRight(true);
                    MovementForward(true);
                    MovementUpDown();

                    if (TurnOffCheck())
                    {
                        DP.TurnOffTrigger();
                        ourDrone.velocity = Vector3.zero;
                        ourDrone.angularVelocity = Vector3.zero;
                        status = DroneStatus.Rest;
                    }

                    else if (DepartureCheck())
                    {
                        EndLanding();
                        status = DroneStatus.Control;
                    }

                    break;
                case DroneStatus.Rest:

                    if (PlayerController.instance.GetStatus() != PlayerStatus.Stay)
                        return;

                    if (TurnOnCheck())
                    {
                        DP.TurnOnTrigger();
                    }
                    break;
            }



            // Hovering
            BasicDroneHoverAndRotation(); //this method applies all the forces and rotations to the drone.

        }
    }

    void Update() {
        RotationUpdateLoop_TrickRotation(); //applies rotation to the drone it self when doing the barrel roll trick, does NOT trigger the animation
        //Animations(); //part where animations are triggered
        DroneSound(); //sound producing stuff
        CameraCorrectPickAndTranslatingInputToWSAD(); //setting input for keys, translating joystick, mobile inputs as WSAD (depending on which is turned on)

    }

    /// <summary>
    /// 기체 기울어짐, 이동 등을 초기화
    /// </summary>
    /// <param name="fixYPos"> 단, 특정 방향으로 바라보아야 하는 경우 y rotate 값을 전달 </param>
    public void DirectDroneRecoverMoment(float fixYPos = 0f)
    {
        ResetDroneObjectRotation(fixYPos);
        ResetDroneVelocity();

    }

    public void SetStatus(DroneStatus _status)
    {
        if (status == DroneStatus.Landing || status == DroneStatus.Rest)
            return;

        status = _status;
    }
    public DroneStatus GetStatus() => status;

    /// <summary>
    /// 드론 조종 가능 여부 판단
    /// </summary>
    public bool IsControlable()
    {
        if (status == DroneStatus.Landing || status == DroneStatus.Rest)
            return false;

        else
            return true;
    }

    /// <summary>
    /// 드론 Follow 시스템의 활성화/비활성화를 제어하는 함수
    /// </summary>
    /// <param name="isOn">true는 활성화, false는 비활성화</param>
    public void SetDroneFollowActive(bool isOn)
    {
        GetComponent<DroneFollowController>().enabled = isOn;
    }
}
