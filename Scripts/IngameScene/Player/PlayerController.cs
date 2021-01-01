using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

/************************************
* 
* 버전: 
* 작성자: 진민준     최초 작성날짜:             최근 수정날짜:       
* 설명:
*   플레이어의 움직임을 제어하는 스크립트
* 
************************************/

public enum PlayerStatus { Control, Stay, Talk, Trouble, }

public class PlayerController : MonoBehaviour
{
    #region variable
    // singleton
    public static PlayerController instance;

    // 드론
    [Header("Drone Elements")]
    public GameObject drone;
    public DroneMovement dm;
    public GameObject dronePos;// 드론이 위치할 위치

    // 드론 조종 가능 최대 거리
    [Header("Drone Control Info")]
    static public int MAX_DRONE_CONTROL_DISTANCE = 100;
    public int currentDroneControlMaxDistance = MAX_DRONE_CONTROL_DISTANCE;
    public Image controlDistanceGauge;
    public Text distText;
    
    [Header("My Elements")]
    public Transform cameraPos; // 플레이어 yaw 회전시 플레이어 이전에 카메라가 먼저 회전
    public SkinnedMeshRenderer m_skinnedMeshRenderer;
    Rigidbody rb;
    Animator animator;


    [Header("Sensibilities")]
    float speed = 5f;
    float turnSpeed = 100f;
    float angleSpeed = 100f;

    [Header("Monitor")]
    /// <summary>
    /// [현재 시점 상태]
    /// 1: 플레이어
    /// 2: 플레이어 시점 드론 조종
    /// 3: 드론 3인칭
    /// 4: 드론 1인칭
    /// </summary>
    public int curViewPoint;

    [SerializeField] PlayerStatus status;    // 플레이어 조작 상태

    public IEnumerator viewChangeEffCoroutine; // 시점 전환 효과 코루틴

    #endregion


    private void Awake()
    {
        instance = this;

        status = PlayerStatus.Control;
        
        dm = drone.GetComponent<DroneMovement>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        #region init Variable
        
        dm.joystick_turned_on = InputDeviceChecker.instance.GetCurDevice() == InputDeviceChecker.INPUT_DEVICE.Joystick ? true : false;
        curViewPoint = 1;

        #endregion
    }

    private void Start()
    {
        SpawnPosContainer.instance.SpawnPlayer();

        if (PlayerPrefs.GetInt("isTutorialFinish") == 1)
            TutorialSkip();
    }

    private void FixedUpdate()
    {
        // ###### 버그를 야기할 수 있음 ###### //
        //
        // Alpha5 로 튜토리얼을 스킵.
        // 
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            TutorialSkip();
            TargetMarker.instance.SetNoTarget();
        }

        // 다음의 경우 이동 불가
        // 대화 중
        // UI 동작 중
        if (!TalkManager.instance.talking && !IngameCanvasManager.instance.isCanvasOn)
            movement();
        

        // 시점 전환
        SetViewPoint();
    }



    void movement()
    {
        // 드론과 조종자 간 거리 계산
        CalcDistance();
        
        // 조작
        switch (status)
        {
            case PlayerStatus.Control:

                #region move
        
                if (InputDeviceChecker.instance.Pitch() != 0)
                {
                    //animator.SetBool("runFront", true);
                    float pitch = InputDeviceChecker.instance.Pitch() * -1;
                    if (pitch < 0.1f && pitch > -0.1f)
                        pitch = 0f;
                    
                    // 해당 방향으로 바라보게 회전
                    TurnPlayer(cameraPos.transform.localEulerAngles);

                    // Joystick control                    
                    animator.SetFloat("runY", pitch);
                    transform.Translate(Vector3.forward * speed * pitch * Time.deltaTime);
                    //animator.speed = pitch * -1;   
                }
                else
                {
                    animator.SetFloat("runY", 0);
                }

                // 좌우 걷기
                if (InputDeviceChecker.instance.Roll() != 0)
                {
                    //animator.SetBool("runFront", true);
                    float roll = InputDeviceChecker.instance.Roll();
                    if (roll < 0.1f && roll > -0.1f)
                        roll = 0f;

                    animator.SetFloat("runX", roll);

                    transform.Translate(Vector3.left * speed * -roll * Time.deltaTime);
                }
                else
                {
                    animator.SetFloat("runX", 0);
                }

                #endregion


                #region rotate
                // 상하 시선 이동
                if(InputDeviceChecker.instance.Throttle() != 0)
                {
                    float throttle = InputDeviceChecker.instance.Throttle();


                    cameraPos.eulerAngles += new Vector3(-angleSpeed * throttle * Time.deltaTime, 0, 0);
                
                    // 범위를 벗어났다면 보정
                    if( cameraPos.localEulerAngles.x > 70 && cameraPos.localEulerAngles.x < 180)
                    {
                        cameraPos.localEulerAngles =
                            new Vector3(70, cameraPos.localEulerAngles.y, cameraPos.localEulerAngles.z);
                    }
                    else if(cameraPos.localEulerAngles.x < 300 && cameraPos.localEulerAngles.x > 180)
                    {
                        cameraPos.localEulerAngles =
                            new Vector3(300, cameraPos.localEulerAngles.y, cameraPos.localEulerAngles.z);
                    }

                }


                // 좌우 회전
                if(InputDeviceChecker.instance.Yaw() != 0)
                {
                    float yaw = InputDeviceChecker.instance.Yaw();
                    cameraPos.eulerAngles += new Vector3(0, -angleSpeed * (-yaw) * Time.deltaTime, 0);
                }
                break;
                #endregion

            // 문제 상황. 거리, 배터리 문제 등으로 드론 Recall이 불가피한 상황
            case PlayerStatus.Trouble:

                ResetAnim();

                
                break;
        }
    }


    /// <summary>
    /// 플레이어를 주어진 방향으로 바라보게 하는 함수
    /// </summary>
    /// <param name="lookWay">나아가는 방향</param>
    void TurnPlayer(Vector3 lookWay) {

        transform.InverseTransformPoint(lookWay);

        // turn left
        if (transform.InverseTransformPoint(lookWay).y > 180)
        {
            transform.eulerAngles += new Vector3(0, -1, 0) * Time.deltaTime * turnSpeed;
            cameraPos.eulerAngles -= new Vector3(0, -1, 0) * Time.deltaTime * turnSpeed;
        }

        // turn right
        else if (transform.InverseTransformPoint(lookWay).y < 180)
        {
            transform.eulerAngles += new Vector3(0, 1, 0) * Time.deltaTime * turnSpeed;
            cameraPos.eulerAngles -= new Vector3(0, 1, 0) * Time.deltaTime * turnSpeed;
        }
    }


    /// <summary>
    /// 플레이어와 드론간 거리 계산
    /// </summary>
    void CalcDistance()
    {
        if (!IngameCanvasManager.instance.isIngameScene) return;

        // 거리 계산
        controlDistanceGauge.fillAmount = Vector3.Distance(transform.position, drone.transform.position) / currentDroneControlMaxDistance;
        distText.text = (controlDistanceGauge.fillAmount * currentDroneControlMaxDistance).ToString("000") + " M";

        // 문자 색상
        if (controlDistanceGauge.fillAmount >= 1.0f) distText.color = Color.red;
        else if (controlDistanceGauge.fillAmount > 0.7) distText.color = new Color(1f, 0.5f, 0f);
        else distText.color = Color.white;

        if ((dm.GetStatus() != DroneStatus.Recall && dm.GetStatus() != DroneStatus.Landing && dm.GetStatus() != DroneStatus.Rest)
            && PlayerPrefsData.instance.isTutorialFinish == 1
            && Vector3.Distance(transform.position, drone.transform.position) > currentDroneControlMaxDistance
            )
        {
            status = PlayerStatus.Trouble;
            dm.SetStatus(DroneStatus.Trouble);

            // 메시지
            IngameCanvasManager.instance.OpenNoticeMessage("플레이어와 거리가 너무 멉니다.", 3.0f);
        }
    }


    /// <summary>
    /// 플레이어 애니메이션을 초기화 (Idle 상태로 전환)
    /// </summary>
    public void ResetAnim()
    {
        animator.SetFloat("runX", 0);
        animator.SetFloat("runY", 0);
    }

    /// <summary>
    /// 키 입력을 통해 시점을 변환하는 함수
    /// </summary>
    void SetViewPoint()
    {
        // 시점 변환 불가한 경우
        // 대화 중
        // 튜토리얼 중
        // 키보드 입력이 아닌 경우
        // 인게임 씬이 아닌 경우
        // 지도(World map)가 활성화되어 있는 경우
        // 시점 전환 이펙트가 수행중인 경우
        if (TalkManager.instance.talking
            || PlayerPrefsData.instance.isTutorialFinish == 0
            || InputDeviceChecker.instance.GetCurDevice() != InputDeviceChecker.INPUT_DEVICE.Keyboard
            || !IngameCanvasManager.instance.isIngameScene
            || MinimapCameraController.instance.isWorldmap
            || viewChangeEffCoroutine != null
            )
            return;
        
        // 캐릭터 시점, 캐릭터 조종
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // 시점 전환 이펙트 수행
            //viewChangeEffCoroutine = ViewChangeEff(1, true);
            //StartCoroutine(viewChangeEffCoroutine);

             SetViewPoint(1);
        }

        // 캐릭터 시점, 드론 조종
        else if (Input.GetKeyDown(KeyCode.Alpha2) )
        {
            // 시점 전환 이펙트 수행
            //viewChangeEffCoroutine = ViewChangeEff(2, true);
            //StartCoroutine(viewChangeEffCoroutine);

            SetViewPoint(2);
        }

        // TPP 드론 시점, 드론 조종
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            // 시점 전환 이펙트 수행
            //viewChangeEffCoroutine = ViewChangeEff(3, true);
            //StartCoroutine(viewChangeEffCoroutine);

            SetViewPoint(3);
        }

        // FPP 드론 시점, 드론 조종
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            // 시점 전환 이펙트 수행
            //viewChangeEffCoroutine = ViewChangeEff(4, true);
            //StartCoroutine(viewChangeEffCoroutine);

            SetViewPoint(4);
        }
    }
    
    /// <summary>
    /// 임의로 시점을 변환할 때 호출되는 함수
    /// </summary>
    /// <param name="key">키보드 입력을 통해 시점을 변환할 때의 값과 매칭</param>
    /// <param name="passFlag">대화, 튜토리얼 등 진행 불가한 상황에도 예외를 두어 진행 가능케 하는 플레그</param>
    /// <param name="effCancelFlag">시점 전환 이펙트를 수행하지 않는 플래그</param>
    public void SetViewPoint(int key, bool passFlag = false, bool effCancelFlag = false)
    {
        print($"[TEST] SetViewPoint({key}, {passFlag}, {effCancelFlag}) is called");

        if ((!passFlag && TalkManager.instance.talking)
            || viewChangeEffCoroutine != null
            )
            return;

        if (effCancelFlag
            || key == curViewPoint)
        {
            SetViewPointCore(key, false);
        }
        else
        {
            viewChangeEffCoroutine = ViewChangeEff(key);
            StartCoroutine(viewChangeEffCoroutine);
        }
    }

    void SetViewPointCore(int alpha, bool isKeyboardInput)
    {

        if (isKeyboardInput)
        {
            switch (alpha)
            {
                case 1:
                    PostProcessController.instance.EnablePostProcessEff(PostProcessType.playerViewEff);
                    curViewPoint = 1;

                    // 드론 및 플레이어 상태 조정
                    if (PlayerPrefsData.instance.isTutorialFinish == 1)
                    {
                        drone.GetComponent<DroneFollowController>().StartRecall();
                        //dm.setStatus(DroneStatus.Recall);
                    }
                    status = PlayerStatus.Control;

                    // UI 셋팅
                    IngameCanvasManager.instance.PlayerMode();

                    // 카메라 셋팅
                    //InRoomCamera.instance.cameraMode = InRoomCamera.CameraMode.CharacterView;
                    InRoomCamera.instance.ChangeCameraMode(InRoomCamera.CameraMode.CharacterView);
                    MinimapCameraController.instance.SetTarget(gameObject);

                    // 애니메이션 초기화
                    animator.SetBool("isControl", false);
                    ResetAnim();
                    break;

                case 2:
                    PostProcessController.instance.EnablePostProcessEff(PostProcessType.playerDroneViewEff);
                    curViewPoint = 2;

                    // 드론 및 플레이어 상태 조정
                    drone.GetComponent<DroneFollowController>().StopRecall();
                    status = PlayerStatus.Stay;
                    dm.SetStatus(DroneStatus.Control);

                    // 카메라 셋팅
                    //InRoomCamera.instance.cameraMode = InRoomCamera.CameraMode.PlayerDroneView;
                    InRoomCamera.instance.ChangeCameraMode(InRoomCamera.CameraMode.PlayerDroneView);

                    MinimapCameraController.instance.SetTarget(gameObject);

                    // UI 셋팅
                    IngameCanvasManager.instance.DroneMode(2);

                    // 애니메이션 초기화
                    animator.SetBool("isControl", true);
                    ResetAnim();
                    break;

                case 3:
                    PostProcessController.instance.EnablePostProcessEff(PostProcessType.droneTppEff);
                    curViewPoint = 3;

                    // 드론 및 플레이어 상태 조정
                    drone.GetComponent<DroneFollowController>().StopRecall();
                    status = PlayerStatus.Stay;
                    dm.SetStatus(DroneStatus.Control);

                    // 카메라 셋팅
                    //InRoomCamera.instance.cameraMode = InRoomCamera.CameraMode.TPDroneView;
                    InRoomCamera.instance.ChangeCameraMode(InRoomCamera.CameraMode.TPDroneView);
                    MinimapCameraController.instance.SetTarget(dm.gameObject);

                    // UI 셋팅
                    IngameCanvasManager.instance.DroneMode(3);

                    // 애니메이션 초기화
                    animator.SetBool("isControl", true);
                    ResetAnim();
                    break;

                case 4:
                    PostProcessController.instance.EnablePostProcessEff(PostProcessType.droneFppEff);
                    curViewPoint = 4;

                    // 드론 및 플레이어 상태 조정
                    drone.GetComponent<DroneFollowController>().StopRecall();
                    status = PlayerStatus.Stay;
                    dm.SetStatus(DroneStatus.Control);

                    // 카메라 셋팅
                    //InRoomCamera.instance.cameraMode = InRoomCamera.CameraMode.FPDroneView;
                    InRoomCamera.instance.ChangeCameraMode(InRoomCamera.CameraMode.FPDroneView);
                    MinimapCameraController.instance.SetTarget(dm.gameObject);

                    // UI 셋팅
                    IngameCanvasManager.instance.DroneMode(4);

                    // 애니메이션 초기화
                    animator.SetBool("isControl", true);
                    ResetAnim();
                    break;
            }
        }
        else
        {
            switch (alpha)
            {
                case 1:

                    PostProcessController.instance.EnablePostProcessEff(PostProcessType.playerViewEff);
                    curViewPoint = 1;

                    // UI 셋팅
                    IngameCanvasManager.instance.PlayerMode();

                    // 카메라 셋팅
                    //InRoomCamera.instance.cameraMode = InRoomCamera.CameraMode.CharacterView;
                    InRoomCamera.instance.ChangeCameraMode(InRoomCamera.CameraMode.CharacterView);
                    MinimapCameraController.instance.SetTarget(gameObject);

                    //// 인게임 ui 조작중이라면 follow 동작 x
                    //// 혹은 튜토리얼이 끝나지 않았다면 follow 동작 x
                    //// 해당 탭을 닫는다면 follow 동작 o
                    //if (IngameCanvasManager.instance.isCanvasOn)
                    //{
                    //    break;
                    //}

                    // 드론 및 플레이어 상태 조정
                    if (PlayerPrefsData.instance.isTutorialFinish == 1)
                    {
                        drone.GetComponent<DroneFollowController>().StartRecall();
                        dm.SetStatus(DroneStatus.Recall);
                    }
                    status = PlayerStatus.Control;

                    // 애니메이션 초기화
                    animator.SetBool("isControl", false);
                    ResetAnim();
                    break;

                case 2:
                    PostProcessController.instance.EnablePostProcessEff(PostProcessType.playerDroneViewEff);
                    curViewPoint = 2;

                    // 드론 및 플레이어 상태 조정
                    drone.GetComponent<DroneFollowController>().StopRecall();
                    status = PlayerStatus.Stay;
                    dm.SetStatus(DroneStatus.Control);

                    // 카메라 셋팅
                    //InRoomCamera.instance.cameraMode = InRoomCamera.CameraMode.PlayerDroneView;
                    InRoomCamera.instance.ChangeCameraMode(InRoomCamera.CameraMode.PlayerDroneView);
                    MinimapCameraController.instance.SetTarget(gameObject);

                    // UI 셋팅
                    IngameCanvasManager.instance.DroneMode(2);

                    // 애니메이션 초기화
                    animator.SetBool("isControl", true);
                    ResetAnim();
                    break;

                case 3:
                    PostProcessController.instance.EnablePostProcessEff(PostProcessType.droneTppEff);
                    curViewPoint = 3;

                    // 드론 및 플레이어 상태 조정
                    drone.GetComponent<DroneFollowController>().StopRecall();
                    status = PlayerStatus.Stay;
                    dm.SetStatus(DroneStatus.Control);

                    // 카메라 셋팅
                    //InRoomCamera.instance.cameraMode = InRoomCamera.CameraMode.TPDroneView;
                    InRoomCamera.instance.ChangeCameraMode(InRoomCamera.CameraMode.TPDroneView);
                    MinimapCameraController.instance.SetTarget(dm.gameObject);

                    // UI 셋팅
                    IngameCanvasManager.instance.DroneMode(3);

                    // 애니메이션 초기화
                    animator.SetBool("isControl", true);
                    ResetAnim();
                    break;

                case 4:
                    PostProcessController.instance.EnablePostProcessEff(PostProcessType.droneFppEff);
                    curViewPoint = 4;

                    // 드론 및 플레이어 상태 조정
                    drone.GetComponent<DroneFollowController>().StopRecall();
                    status = PlayerStatus.Stay;
                    dm.SetStatus(DroneStatus.Control);

                    // 카메라 셋팅
                    //InRoomCamera.instance.cameraMode = InRoomCamera.CameraMode.FPDroneView;
                    InRoomCamera.instance.ChangeCameraMode(InRoomCamera.CameraMode.FPDroneView);
                    MinimapCameraController.instance.SetTarget(dm.gameObject);

                    // UI 셋팅
                    IngameCanvasManager.instance.DroneMode(4);

                    // 애니메이션 초기화
                    animator.SetBool("isControl", true);
                    ResetAnim();
                    break;
            }
        }
    }

    /// <summary>
    /// 시점 전환 이펙트
    /// </summary>
    /// <param name="alpha">시점 전환 넘버 - 키보드 1, 2, 3... 에 대응</param>
    /// <param name="isKeyboardInput">키보드 1, 2... 의 입력에 의한 호출일 경우</param>
    IEnumerator ViewChangeEff(int alpha, bool isKeyboardInput = false)
    {
        Image fadeImg = IngameCanvasManager.instance.GetFadeEffImg();
        fadeImg.gameObject.SetActive(true);
        Transform controllerTr = null;

        Color c = fadeImg.color;
        c.a = 0;
        fadeImg.color = c;

        // 플레이어 -> 드론 조종시 카메라가 컨트롤러로 들어가는 연출
        if (curViewPoint == 1)// && alpha != 1)
        {
            animator.SetBool("isControl", true);
            //InRoomCamera.instance.cameraMode = InRoomCamera.CameraMode.Free;
            InRoomCamera.instance.ChangeCameraMode(InRoomCamera.CameraMode.Free);
            controllerTr = transform.Find("Equipments").Find("DroneController");

            float timer = 0f;
            Vector3 targetPos = Vector3.zero;
            Vector3 startPos = InRoomCamera.instance.transform.position;

            

            while (timer < 0.5f)
            {
                timer += Time.deltaTime;

                targetPos = controllerTr.position + controllerTr.TransformVector(new Vector3(0, 0, 0.8f));
                InRoomCamera.instance.transform.position = Vector3.Lerp(startPos, targetPos, timer);
                InRoomCamera.instance.transform.LookAt(targetPos);

                yield return null;
            }

            RaycastHit hit;
            if(Physics.Raycast(InRoomCamera.instance.transform.position, InRoomCamera.instance.transform.forward,
                out hit,
                Vector3.Distance(InRoomCamera.instance.transform.position, targetPos)))
            {
                // 컨트롤러가 플레이어에 가려 검출되지 않았다면
                // 플레이어 렌더러를 false
                if(!hit.collider.CompareTag("EquipController")){
                    m_skinnedMeshRenderer.enabled = false;
                }
            }


            while(timer < 1f)
            {
                timer += Time.deltaTime;

                c.a = (timer - 0.5f) * 2;
                fadeImg.color = c;

                targetPos = controllerTr.position + controllerTr.TransformVector(new Vector3(0, 0, 0.8f));
                InRoomCamera.instance.transform.position = Vector3.Lerp(startPos, targetPos, timer);
                InRoomCamera.instance.transform.LookAt(targetPos);

                yield return null;
            }

            m_skinnedMeshRenderer.enabled = true;
        }
        else
        {
            


            while (c.a < 0.95f)
            {
                c.a += Time.deltaTime * 2;
                fadeImg.color = c;

                yield return null;
            }
        }

        c.a = 1f;
        fadeImg.color = c;

        // 시점 전환 수행
        SetViewPointCore(alpha, isKeyboardInput);

        while (c.a > 0.05f)
        {
            c.a -= Time.deltaTime * 2;
            fadeImg.color = c;

            yield return null;
        }

        c.a = 0f;
        fadeImg.color = c;
        fadeImg.gameObject.SetActive(false);

        // 화면 밝아짐
        // 조작 허용 및 게임 진행
        viewChangeEffCoroutine = null;
    }


    public void setStatus(PlayerStatus _status) => status = _status;
    public PlayerStatus GetStatus() => status;

    public Vector3 getDronePos()
    {
        return dronePos.transform.position;
    }
    
    /// <summary>
    /// 타겟을 향해 돌아보는 코루틴
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public IEnumerator Turn(GameObject target)
    {
        Vector3 targetEulerAngles = Vector3.zero;
        targetEulerAngles.y = target.transform.eulerAngles.y + 180;

        while (transform.eulerAngles.y < targetEulerAngles.y - 3.0f ||
            transform.eulerAngles.y > targetEulerAngles.y + 3.0f)
        {
            transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, targetEulerAngles, 0.03f);
            yield return null;
        }

        //transform.eulerAngles = targetEulerAngles;
    }
    

    void TutorialSkip()
    {
        DroneFollowController DFC = PlayerController.instance.drone.GetComponent<DroneFollowController>();

        // 드론 Recall 활성화.
        DFC.enabled = true;

        // 드론이 플레이어를 따라다니도록 설정
        DFC.dronePos
        = PlayerController.instance.dronePos;

        DFC.StartRecall();

        // 튜토리얼 종료 시그널 전달
        // 드론으로 시점 변환이 가능
        PlayerPrefsData.instance.isTutorialFinish = 1;
        PlayerPrefs.SetInt("isTutorialFinish", 1);

        // 드론 위치 조정
        dm.transform.position = PlayerController.instance.transform.position + new Vector3(0, 10, 0);


        // 튜토리얼 스킵
        if (GameObject.Find("Examiner"))
            GameObject.Find("Examiner").GetComponent<NPCController>().SetStep("09");

    }
}
