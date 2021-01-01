using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/**
 * 
 * 입력 장치를 체크하는 스크립트
 * 
 * SPECTRUM RECEIVER - 17 Length
 * 
 * InterLink-X - 11 Length
 * 
 * 여러 입력 장치에 대해 대응하고자 한다면 보완 작업 필요
 * 
 * */

public class InputDeviceChecker : MonoBehaviour
{
    public static InputDeviceChecker instance;
    public static bool isStart = true; // 최초 시작 스폰 지점 선택은 키보드로 하도록 유도키 위한 플래그

    // DroneMovementScript.inputEditorSelection
    // custom editor의 Device select btn과 그 순서를 맞춤
    public enum INPUT_DEVICE { Keyboard = 1, Joystick, Mobile, }
    

    // 조이스틱 입력 장치가 연결되었다면
    // 키보드 입력 장치를 비활성화 한다.
    public GameObject keyboard_eventSystem;
    public GameObject joystick_eventSystem;

    [SerializeField] INPUT_DEVICE inputDevice;

    // 실행 중 입력 장치 변경 버튼
    //[SerializeField] bool deviceRefresh;

    string[] inputDeviceName;
    [HideInInspector] public string pitchInputAxis, yawInputAxis, throttleInputAxis, rollInputAxis, interactInputAxis;
    [HideInInspector] public bool isPitchAxisReverse, isYawAxisReverse, isThrottleAxisReverse, isRollAxisReverse;

    // 영점 조절용 변수
    [HideInInspector] public bool isInterLinkX;


    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        //print($"{gameObject.GetInstanceID()} is Start()");
        //print($"cur active scene is {SceneManager.GetActiveScene().name}");

        //print($"input device checker sceneName : {SceneManager.GetActiveScene().name}");
        //if (SceneManager.GetActiveScene() != SceneManager.GetSceneByName("LoadingScene") && PlayerPrefs.HasKey("deviceName") )
        //{
        //    SetInputDevice(PlayerPrefs.GetString("deviceName"));
        //    print($"[TEST] MyRoom device is set to {PlayerPrefs.GetString("deviceName")}");
        //}

        //else
        //{
        //    //if (PlayerPrefs.HasKey("deviceName")){
        //    //    PlayerPrefs.SetString("deviceName", "Keyboard");
        //    //}
        //    keyboard_eventSystem.SetActive(true);
        //    joystick_eventSystem.SetActive(false);

        //    inputDevice = INPUT_DEVICE.Keyboard;
        //}

        if (isStart)
        {
            isStart = false;

            keyboard_eventSystem.SetActive(true);
            joystick_eventSystem.SetActive(false);

            inputDevice = INPUT_DEVICE.Keyboard;
        }
        else
        {
            SetInputDevice(PlayerPrefs.GetString("deviceName"));
            print($"[TEST] MyRoom device is set to {PlayerPrefs.GetString("deviceName")}");
        }

    }
    

    // 2020-08-03 이승주 작업
    /***********************
     * 컨트롤러에서 받아들이는 값 소수점 2째자리 까지로 제한. (3째이하는 버림)
     ***********************/
    public float ReduceDicimal(float num)
    {
        //int temp = (int)(num * 100.0f);

        //float temp2 = temp * 0.01f;


        // 소수점 1째자리로 제한
        int temp = (int)(num * 10.0f);

        float temp2 = temp * 0.1f;

        return temp2;
    }

    /// <summary>
    /// 입력 장치 연결을 확인하는 함수
    /// 현재, 키보드 - 컨트롤러(DXe) 지원
    /// </summary>
    public bool ConnectedDeviceRefresher(string deviceName)
    {
        if (deviceName == "Keyboard")
        {
            // setting Input manager
            keyboard_eventSystem.SetActive(true);
            joystick_eventSystem.SetActive(false);

            IngameCanvasManager.instance.eventSystem = keyboard_eventSystem.GetComponent<EventSystem>();

            inputDevice = INPUT_DEVICE.Keyboard;
            PlayerPrefs.SetString("deviceName", deviceName);

            print("Joystick device is not found");

            return true;
        }


        inputDeviceName = Input.GetJoystickNames();

        bool isError = true;
        foreach (string s in inputDeviceName)
        {
            if (s == deviceName)
            {
                print($"[{deviceName}] 연결 확인");
                isError = false;
                break;
            }
        }
        
        // 해당 디바이스가 연결되어 있지 않다면 키보드로 설정.
        if (isError)
        {
            // setting Input manager
            keyboard_eventSystem.SetActive(true);
            joystick_eventSystem.SetActive(false);

            IngameCanvasManager.instance.eventSystem = keyboard_eventSystem.GetComponent<EventSystem>();

            inputDevice = INPUT_DEVICE.Keyboard;
            PlayerPrefs.SetString("deviceName", "Keyboard");

            return false;
        }

        switch (deviceName)
        {
            case "SPEKTRUM RECEIVER":
                keyboard_eventSystem.SetActive(false);
                joystick_eventSystem.SetActive(true);

                IngameCanvasManager.instance.eventSystem = joystick_eventSystem.GetComponent<EventSystem>();

                inputDevice = INPUT_DEVICE.Joystick;
                isInterLinkX = false;

                print(inputDeviceName[0] + " is connected");

                break;

            case "InterLinkDX":
                keyboard_eventSystem.SetActive(false);
                joystick_eventSystem.SetActive(true);

                IngameCanvasManager.instance.eventSystem = joystick_eventSystem.GetComponent<EventSystem>();

                inputDevice = INPUT_DEVICE.Joystick;
                isInterLinkX = false;

                print(inputDeviceName[0] + " is connected");
                break;

            case "InterLink-X":
                keyboard_eventSystem.SetActive(false);
                joystick_eventSystem.SetActive(true);

                IngameCanvasManager.instance.eventSystem = joystick_eventSystem.GetComponent<EventSystem>();

                inputDevice = INPUT_DEVICE.Joystick;
                isInterLinkX = true;

                print(inputDeviceName[0] + " is connected");
                break;
        }

        PlayerPrefs.SetString("deviceName", deviceName);
        return true;
    }

    #region set input device

    /// <summary>
    /// 입력 장치 변경 가능 여부 확인 및 실행 함수
    /// </summary>
    /// <param name="deviceName">입력 장치 이름</param>
    /// <returns>가능 여부</returns>
    public bool SetInputDevice(string deviceName = "Keyboard")
    {

        if (!ConnectedDeviceRefresher(deviceName))
        {
            Debug.LogError($"[BUG] 해당 {deviceName} 디바이스가 존재하지 않습니다.");


            return false;
        }

        switch (deviceName)
        {
            case "Keyboard":
                print("== Device is [Keyboard] now ==");
                PlayerController.instance.dm.joystick_turned_on = false;

                break;

            case "SPEKTRUM RECEIVER":
                print("== Device is [SPEKTRUM RECEIVER] now ==");
                PlayerController.instance.dm.joystick_turned_on = true;

                throttleInputAxis = "4th axis";
                yawInputAxis = "X axis";
                pitchInputAxis = "Y axis";
                rollInputAxis = "3rd axis";
                interactInputAxis = "DXeInteract";

                isThrottleAxisReverse = false;
                isYawAxisReverse = false;
                isPitchAxisReverse = false;
                isRollAxisReverse = false;

                joystick_eventSystem.GetComponent<StandaloneInputModule>().horizontalAxis = "DXeHorizontal";
                joystick_eventSystem.GetComponent<StandaloneInputModule>().verticalAxis = "DXeVertical";

                joystick_eventSystem.GetComponent<StandaloneInputModule>().submitButton = "DXeInteract";

                break;

            case "InterLinkDX":
                print("== Device is [InterLinkDX] now ==");
                PlayerController.instance.dm.joystick_turned_on = true;

                throttleInputAxis = "Y axis";
                yawInputAxis = "X axis";
                pitchInputAxis = "5th axis";
                rollInputAxis = "4th axis";
                interactInputAxis = "LinkDXInteract";

                isThrottleAxisReverse = false;
                isYawAxisReverse = true;
                isPitchAxisReverse = false;
                isRollAxisReverse = true;

                joystick_eventSystem.GetComponent<StandaloneInputModule>().verticalAxis = "LinkDXVertical";
                joystick_eventSystem.GetComponent<StandaloneInputModule>().horizontalAxis = "LinkDXHorizontal";

                joystick_eventSystem.GetComponent<StandaloneInputModule>().submitButton = "LinkDXInteract";

                break;

            case "InterLink-X":
                print("== Device is [InterLink-X] now ==");
                PlayerController.instance.dm.joystick_turned_on = true;

                throttleInputAxis = "3rd axis";
                yawInputAxis = "6th axis";
                pitchInputAxis = "Y axis";
                rollInputAxis = "X axis";
                interactInputAxis = "Link-XInteract";

                isThrottleAxisReverse = true;
                isYawAxisReverse = true;
                isPitchAxisReverse = true;
                isRollAxisReverse = true;

                joystick_eventSystem.GetComponent<StandaloneInputModule>().verticalAxis = "Link-XVertical";
                joystick_eventSystem.GetComponent<StandaloneInputModule>().horizontalAxis = "Link-XHorizontal";

                joystick_eventSystem.GetComponent<StandaloneInputModule>().submitButton = "Link-XInteract";

                break;
        }

        return true;
    }

    #endregion



    /// <summary>
    /// 현재 키보드, 조이스틱 등 어떤 디바이스가 연결되어 있는지를 반환
    /// </summary>
    /// <returns>
    /// DroneMovementScript의 Custom Editor 선택지와 동일한 값 반환.
    /// 1: Keyboard.  2: Joystick.  3: Mobile.
    /// </returns>
    public INPUT_DEVICE GetCurDevice()
    {
        return inputDevice;
    }


    /// <summary>
    /// 현재 연결된 디바이스에 의해 사용될 이벤트 시스템
    /// IngameCanvasManager.cs, TalkManager.cs 등에서 본 함수 호출 후
    /// </summary>
    /// <returns></returns>
    public EventSystem GetCurEventSystem()
    {
        switch (inputDevice)
        {
            case INPUT_DEVICE.Joystick:
                return joystick_eventSystem.GetComponent<EventSystem>();
            case INPUT_DEVICE.Keyboard:
                return keyboard_eventSystem.GetComponent<EventSystem>();
            default:
                print("연결된 이벤트 시스템을 찾을 수 없습니다 !!");
                return null;
        }
    }

    /// <summary>
    /// 키 입력이 이루어지고 있는지를 확인
    /// </summary>
    /// <returns>어떠한 키가 입력중이라면 true 반환</returns>
    public bool GetAnyAxis()
    {
        if (Pitch() != 0) return true;
        if (Yaw() != 0) return true;
        if (Throttle() != 0) return true;
        if (Roll() != 0) return true;
        if (Input.GetAxis("Interact") != 0) return true;
        return false;
    }

    // 한 Input으로 조이스틱과 키보드를 동시에 제어하는 것이 불가능
    // 따라서 각 디바이스 타입에 따라 Input 정보들을 전부 따로 설정
    // 이후 현재 연결된 디바이스에 알맞는 input 정보를 전달
    #region INPUT

    public float Pitch()
    {
        switch (inputDevice)
        {
            case INPUT_DEVICE.Keyboard:

                return Input.GetAxis("Vertical_r");
            case INPUT_DEVICE.Joystick:

                float pitch = Input.GetAxis(pitchInputAxis) * (isPitchAxisReverse == true ? 1 : -1);

                //2020-08-03 이승주
                /* 영점 조절 편하게 하기 위해서 씀 */
                pitch = ReduceDicimal(pitch);

                if (pitch >= 0) return pitch * pitch; // y = x^2
                else return -(pitch * pitch) ; // y = -(x^2)
        }
        return 0.0f;
    }

    public float Roll()
    {
        switch (inputDevice)
        {
            case INPUT_DEVICE.Keyboard:
                return Input.GetAxis("Horizontal_r");
            case INPUT_DEVICE.Joystick:

                float roll = Input.GetAxis(rollInputAxis) * (isRollAxisReverse == true ? 1 : -1);

                //2020-08-03 이승주
                /* 영점 조절 편하게 하기 위해서 씀 */
                roll = ReduceDicimal(roll);

                if (roll >= 0) return roll * roll;
                else return -(roll * roll);
        }
        return 0.0f;
    }

    public float Throttle()
    {
        switch (inputDevice)
        {
            case INPUT_DEVICE.Keyboard:
                
                return Input.GetAxis("Vertical");
            case INPUT_DEVICE.Joystick:

                float throttle = Input.GetAxis(throttleInputAxis) * (isThrottleAxisReverse == true ? -1 : 1);

                //2020-08-03 이승주
                /* 영점 조절 편하게 하기 위해서 씀 */
                throttle = ReduceDicimal(throttle);

                if (throttle >= 0) return throttle * throttle;
                else return -(throttle * throttle);
        }
        return 0.0f;
    }

    public float Yaw()
    {
        switch (inputDevice)
        {
            case INPUT_DEVICE.Keyboard:
                return Input.GetAxis("Horizontal");
            case INPUT_DEVICE.Joystick:

                float yaw = Input.GetAxis(yawInputAxis) * (isYawAxisReverse == true ? 1 : -1);

                //2020-08-03 이승주
                /* 영점 조절 편하게 하기 위해서 씀 */
                yaw = ReduceDicimal(yaw);

                // InterLink-X이 연결되면 추가연산으로 멈추게함.
                if (isInterLinkX.Equals(true))
                    return yaw + 0.18f;
                else
                    return yaw;
        }
        return 0.0f;
    }

    public bool Interact()
    {
        // 지도 활성화중에는 작동 X
        //if (MinimapCameraController.instance != null && MinimapCameraController.instance.isWorldmap)
        //    return false;

        switch (inputDevice)
        {
            case INPUT_DEVICE.Keyboard:
                return Input.GetButtonDown("Interact");
            case INPUT_DEVICE.Joystick:
                return Input.GetButtonDown(interactInputAxis);
        }
        return false;
    }

    public bool InteractUp()
    {
        switch (inputDevice)
        {
            case INPUT_DEVICE.Keyboard:
                return Input.GetButtonUp("Interact");
            case INPUT_DEVICE.Joystick:
                return Input.GetButtonUp(interactInputAxis);
        }
        return false;
    }



    #endregion

    #region INPUT RAW

    public float Pitch_RAW()
    {
        switch (inputDevice)
        {
            case INPUT_DEVICE.Keyboard:

                return Input.GetAxisRaw("Vertical_r");
            case INPUT_DEVICE.Joystick:

                float pitch = Input.GetAxis(pitchInputAxis) * (isPitchAxisReverse == true ? 1 : -1);

                if (pitch > 0.5f) return 1;
                else if (pitch < -0.5f) return -1;
                else return 0;
        }
        return 0.0f;
    }

    public float Roll_RAW()
    {
        switch (inputDevice)
        {
            case INPUT_DEVICE.Keyboard:
                return Input.GetAxisRaw("Horizontal_r");
            case INPUT_DEVICE.Joystick:

                float roll = Input.GetAxis(rollInputAxis) * (isRollAxisReverse == true ? 1 : -1);

                if (roll > 0.5f) return 1;
                else if (roll < -0.5f) return -1;
                else return 0;
        }
        return 0.0f;
    }

    public float Throttle_RAW()
    {
        switch (inputDevice)
        {
            case INPUT_DEVICE.Keyboard:

                return Input.GetAxisRaw("Vertical");
            case INPUT_DEVICE.Joystick:

                float throttle = Input.GetAxis(throttleInputAxis) * (isThrottleAxisReverse == true ? -1 : 1);

                if (throttle > 0.5f) return 1;
                else if (throttle < -0.5f) return -1;
                else return 0f;
        }
        return 0.0f;
    }

    public float Yaw_Raw()
    {
        switch (inputDevice)
        {
            case INPUT_DEVICE.Keyboard:
                return Input.GetAxisRaw("Horizontal");
            case INPUT_DEVICE.Joystick:

                float yaw = Input.GetAxis(yawInputAxis) * (isYawAxisReverse == true ? 1 : -1);

                if (yaw > 0.5f) return 1;
                else if (yaw < -0.5f) return -1;
                else return 0;
        }
        return 0.0f;
    }

    #endregion



}
