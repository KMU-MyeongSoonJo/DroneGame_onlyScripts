using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/**
 * 
 * Ingame Scene에서 Canvas를 관리하는 스크립트
 * 각 상황에 맞는 UI를 Canvas에 On/Off 하는 기능을 담당한다.
 * 
 * */

public class IngameCanvasManager : MonoBehaviour
{
    public static IngameCanvasManager instance;

    public EventSystem eventSystem;

    [Header("_Ingame Canvas Elements")]
    // 기본 UI
    public GameObject basicUI;
    public bool isBasicUILock; // true이면 조작시 본 ui가 닫히지 않는다
    public GameObject leftTopPanel; // 좌상단 상태 바
    //public GameObject tabButton; // 우하단 기능 버튼
    public GameObject rightBottomTab; // 우하단 기능 탭
    public GameObject minimap; // 미니맵
    public GameObject minimapCam; // 미니맵 카메라
    public RectTransform compassNeedle; // 나침반 바늘
    public GameObject effUI;

    [Space]
    public GameObject noticeUI;
    public GameObject topPos; // 알림창이 표시될 상단 위치
    public Text noticeMessage; // 메시지
    public NoticeMessageController nmc;
    public HighlightSound highlightSound;

    [Header("MenuTab")]
    public GameObject selector;

    [Header("Drone Control Mode UI")]
    // 드론 조작 모드일 때 나타나는 UI
    public GameObject droneControlUI;
    public Transform fpvUI; // 드론 1인칭 시점일 때 표시되는 ui

    [Header("Player Control Mode UI")]
    // 플레이어 조작 모드일 때 나타나는 UI
    public GameObject playerControlUI;

    [Header("Setting UI")]
    // 설정 창 UI
    public GameObject settingUI;
    public RectTransform content;
    public JoyKeyCustomizer joyKeyCustomizer;

    [Header("Monitor")]
    public bool isIngameScene;// 인게임 씬인지 확인하는 플래그. 본 스크립트는 인게임 씬에서만 동작한다.
    public bool isCanvasOn; // 캔버스 조작 중인지 확인하는 플래그
    

    float timer = 0f;
    IEnumerator coroutine;

    private void Awake()
    {
        instance = this;

    }
    private void Start()
    {
        eventSystem = InputDeviceChecker.instance.GetCurEventSystem();

    }

    private void OnEnable()
    {
        isIngameScene = true;
    }

    private void OnDisable()
    {
        isIngameScene = false;
    }

    public void Update()
    {
        if (!isIngameScene) return;

        BasicUIChecker();
    }

    /// <summary>
    /// 드론 상태 UI
    /// </summary>
    /// <param name="curView">2: 플레이어 시점,  3: 드론 3인칭,  4: 드론 1인칭 </param>
    public void DroneMode(int curView)
    {
        switch (curView)
        {
            case 2:
                droneControlUI.SetActive(true);
                fpvUI.gameObject.SetActive(false);
                break;

            case 3:
                droneControlUI.SetActive(true);
                fpvUI.gameObject.SetActive(false);
                break;

            case 4:
                droneControlUI.SetActive(true);
                fpvUI.gameObject.SetActive(true);
                break;
        }
    }

    /// <summary>
    /// 플레이어 상태 UI
    /// </summary>
    public void PlayerMode()
    {
        droneControlUI.SetActive(false);
    }


#region BasicUI


    /// <summary>
    /// 인게임 3초간 대기시 (비)활성화할 UI를 제어하는 함수
    /// </summary>
    public void BasicUIChecker()
    {
        // 나침반 동작 제어
        if (minimap.activeSelf)
            CompassUpdate();

        // ui 잠금
        // 기본 ui들이 꺼지지 않는다
        if (isBasicUILock)
        {
            leftTopPanel.SetActive(true);
            //tabButton.SetActive(true);
            minimap.SetActive(true);
            
            return;
        }
        

        // Basic ui가 비활성화되는 조건 정의
        // 좌상단 상태바, 우하단 메뉴 탭 버튼, 미니맵 ...
        // ==== conditions ====
        // 키 입력중이거나 대화나 퀘스트 진행중에는 표시X 
        // 퀘스트 진행 중?
        // 지도가 켜져 있는 경우
        if (!isCanvasOn && (
            InputDeviceChecker.instance.GetAnyAxis() ||
            TalkManager.instance.talking ||
            QuestManager.instance.GetCurQuestNpc() != null) ||
            MinimapCameraController.instance.isWorldmap
            )
        {
            timer = 0f;
            leftTopPanel.SetActive(false);
            //tabButton.SetActive(false);
            minimap.SetActive(false);
        }


        else
        {
            timer += Time.deltaTime;

            // n초 이상 대기시 UI Hide off
            if (timer > 3f)
            {
                leftTopPanel.SetActive(true);
                //tabButton.SetActive(true);
                minimap.SetActive(true);
            }
        }


        
    }

    /// <summary>
    /// 나침반 동작 제어
    /// </summary>
    void CompassUpdate()
    {
        Vector3 dRot = compassNeedle.eulerAngles;

        dRot.z = minimapCam.transform.eulerAngles.y;

        compassNeedle.eulerAngles = dRot;
    }

    /// <summary>
    /// 인게임 UI 중 우하단 메뉴탭의 (비)활성화 여부를 제어하는 함수
    /// </summary>
    /// <param name="onlyOpen">GetButton() 에 의해서는 true가 전달되어 반드시 열리기만 한다</param>
    public void MenuTab(bool onlyOpen = false)
    {

        Input.ResetInputAxes();

        print("[test] menu 1");

        // 인게임 씬이 아니라면 진행 X
        if (!isIngameScene) return;

        print("[test] menu 2");

        // 탭을 활성화하면 버튼은 비활성화.
        if (!rightBottomTab.activeSelf || onlyOpen)
        {
            rightBottomTab.SetActive(true);
            //tabButton.SetActive(false);
            minimap.SetActive(true);
            leftTopPanel.SetActive(true);

            // 탭이 활성화되면 상호작용 중으로 상태 변경
            isCanvasOn = true;

            eventSystem.SetSelectedGameObject(rightBottomTab.transform.GetChild(0).GetChild(0).gameObject);

            // 메뉴 탭 선택 시작
            StartCoroutine(TabSelector());
        }

        // 탭을 비활성화하면 버튼은 활성화
        else if (rightBottomTab.activeSelf)
        {
            if(!settingUI.activeSelf && PlayerController.instance.viewChangeEffCoroutine != null)
            {
                return;
            }            

            rightBottomTab.SetActive(false);
            //tabButton.SetActive(true);
            minimap.SetActive(false);
            
            // 시점을 되돌리기 전 호출되어야 한다.
            // ui에서의 작업을 종료한다는 플래그 전달
            isCanvasOn = false;

            // 시점 되돌리기
            // isCanvasOn 을 false로 돌린 다음 호출되어야 한다.
            // 드론의 follow를?

            PlayerController.instance.SetViewPoint(PlayerController.instance.curViewPoint, true, true);

            InputDeviceChecker.instance.GetCurEventSystem().SetSelectedGameObject(null);
        }

        print("[test] menu 3");
    }

    /// <summary>
    /// 메뉴 탭을 닫는다(열려있건 닫혀있건 어쨌든 닫는다).
    /// </summary>
    public void MenuTapClose()
    {
        rightBottomTab.SetActive(false);
       // tabButton.SetActive(true);
        minimap.SetActive(false);

        // 시점을 되돌리기 전 호출되어야 한다.
        // ui에서의 작업을 종료한다는 플래그 전달
        isCanvasOn = false;
    }

    /// <summary>
    /// 메뉴 탭 선택 코루틴
    /// </summary>
    IEnumerator TabSelector()
    {
        float tabSpeed = 10f;
        Text SetViewTabText = rightBottomTab.transform.GetChild(0).Find("SetView").GetComponentInChildren<Text>();

        // 메뉴 탭이 닫힐 때까지 반복 체크
        while(rightBottomTab.activeSelf == true)
        {
            string selectTabName = eventSystem.currentSelectedGameObject.name;

            if(selectTabName == "Tab Button")
            {
                eventSystem.SetSelectedGameObject(rightBottomTab.transform.GetChild(0).GetChild(3).gameObject); // Exit Btn
                selectTabName = "Exit";
            }

            if(selectTabName == "Close Tab" || selectTabName == "SetView" || selectTabName == "Setting" || selectTabName == "Exit")
            {

                // 시점 변경
                if (selectTabName == "SetView")
                {
                    // 튜토리얼이 완료되지 않았다면 시점 전환 불가.
                    // 코루틴을 한번만 호출하기 위한 플래그
                    if (PlayerPrefsData.instance.isTutorialFinish != 0 && coroutine == null)
                    {
                        coroutine = SetViewSupporter();
                        StartCoroutine(coroutine);
                    }
                }
                else
                {
                    SetViewTabText.text = GetCurViewPointName(PlayerController.instance.curViewPoint);
                    coroutine = null;
                }
                
                // 선택 박스 이동
                selector.transform.position = Vector3.Lerp(
                    selector.transform.position,
                    eventSystem.currentSelectedGameObject.transform.position,
                    tabSpeed * Time.deltaTime
                    );
            }
            else
            {
                print($"[TEST] cur select obj name : {selectTabName}");
            }
            
            yield return null;
        }
    }

    /// <summary>
    /// 메뉴 탭 내 시점 전환 버튼 제어 코루틴
    /// </summary>
    IEnumerator SetViewSupporter()
    {
        // 조이스틱 시점 전환시 axis가 0이 아닐 때 한 번만 동작되도록 막기 위한 플레그
        // -1: -1 axis 방향으로 동작된 상태
        // 0: 대기 상태
        // 1: 1 axis 방향으로 동작된 상태
        int axisOnlyOnceWorkFlag = 0;

        StandaloneInputModule module = eventSystem.GetComponent<StandaloneInputModule>();

        List<int> viewKey = new List<int>();
        //viewKey.Add(1); // 플레이어 조종, 플레이어 시점, Player
        //viewKey.Add(3); // 드론 조종, 드론 3인칭 시점, TPV
        //viewKey.Add(4); // 드론 조종, 드론 1인칭 시점, FPV
        //viewKey.Add(2); // 드론 조종, 플레이어 시점, PPV

        Text text = eventSystem.currentSelectedGameObject.GetComponentInChildren<Text>();

        #region init

        switch (PlayerController.instance.curViewPoint)
        {
            case 1:
                viewKey.Add(1);
                viewKey.Add(3);
                viewKey.Add(4);
                viewKey.Add(2);

                text.text = "Player";
                break;
            case 3:
                viewKey.Add(3);
                viewKey.Add(4);
                viewKey.Add(2);
                viewKey.Add(1);

                text.text = "TPV";
                break;

            case 4:
                viewKey.Add(4);
                viewKey.Add(2);
                viewKey.Add(1);
                viewKey.Add(3);

                text.text = "FPV";
                break;

            case 2:
                viewKey.Add(2);
                viewKey.Add(1);
                viewKey.Add(3);
                viewKey.Add(4);

                text.text = "PDV";
                break;
        }

        #endregion

        while (eventSystem.currentSelectedGameObject.name == "SetView")
        {


            if (Input.GetButtonDown(module.submitButton))
            {
                PlayerController.instance.SetViewPoint(viewKey[0]);
            }

            if (Input.GetAxis(module.horizontalAxis) == 0)
            {
                axisOnlyOnceWorkFlag = 0;
            }

            else if(Input.GetAxis(module.horizontalAxis) > 0 && axisOnlyOnceWorkFlag != 1)
            {
                axisOnlyOnceWorkFlag = 1; // 연속 동작 방지 플래그

                // (1)-2-3-4   ->   (2)-3-4-1.
                // 제일 앞 값을 뒤로 돌리고
                // (2) 를 매개변수로 시점 전환.
                int key = viewKey[0];
                viewKey.RemoveAt(0);
                viewKey.Insert(viewKey.Count, key);

                highlightSound.PlayHighlightSound();

                Input.ResetInputAxes();
            }
            else if(Input.GetAxis(module.horizontalAxis) < 0 && axisOnlyOnceWorkFlag != -1)
            {
                axisOnlyOnceWorkFlag = -1; // 연속 동작 방지 플래그

                // (1)-2-3-4   ->   (4)-1-2-3
                // 제일 뒤 값을 앞으로 돌리고
                // (4)를 매개변수로 시점 전환
                int key = viewKey[viewKey.Count - 1];
                viewKey.RemoveAt(viewKey.Count - 1);
                viewKey.Insert(0, key);

                highlightSound.PlayHighlightSound();

                Input.ResetInputAxes();
            }
                
            text.text = GetCurViewPointName(viewKey[0]);
            
            yield return null;
        }
    }

    /// <summary>
    /// 시점 정보 값(int)에 따른 이름(string)을 반환하는 함수
    /// </summary>
    string GetCurViewPointName(int curViewPoint)
    {
        switch (curViewPoint)
        {
            case 1: return "Player";
            case 2: return "PDV";
            case 3: return "TPV";
            case 4: return "FPV";
        }
        return null;
    }

    
    /// <summary>
    /// 
    ///                 [no use]
    /// 
    /// 플레이어-드론 간 조종 주체를 변경하는 함수
    /// </summary>
    public void ChangeControlPlayerAndDrone()
    {
        // 튜토리얼 중 시점 전환 불가
        if (PlayerPrefsData.instance.isTutorialFinish == 0) return;

        int key = PlayerController.instance.curViewPoint;

        // 드론 조종중이었다면 플레이어 조종으로 변경
        // 플레이어 조종중이었다면 드론 조종으로 변경
        //    시점은 Flap 레버에따라 전환
        if (key != 1) key = 1;
        else
        {
            float mode = Input.GetAxis("FlightMode");

            if (mode > 0.9f)
            { // 상(default)
                key = 2;
            }
            else if (mode < -0.9f) // 하
            {
                key = 4;
            }
            else // 중
            {
                key = 3;
            }
        }
        
        PlayerController.instance.SetViewPoint(key);
    }

    /// <summary>
    /// 
    ///                 [no use]
    /// 
    /// 드론 조종 시점 전환 함수
    /// DXe 전면 상단 flap 레버의 3단계 입력을 감지해 동작한다
    /// </summary>
    /// <returns>시점 key value(2~4) </returns>
    public void ChangeDroneView()
    {
        // 튜토리얼 중 시점 전환 불가
        // 조이스틱 입력이 아닌 경우에는 함수 진행 X
        if (PlayerPrefsData.instance.isTutorialFinish == 0
            || InputDeviceChecker.instance.GetCurDevice() != InputDeviceChecker.INPUT_DEVICE.Joystick
            ) return;

        // 플레이어 조종 중 전환 억제
        if(PlayerController.instance.curViewPoint == 1) return;

        float mode = Input.GetAxis("FlightMode");

        if (mode > 0.9f) { // 상(default)
            PlayerController.instance.SetViewPoint(3);
        }
        else if (mode< -0.9f) // 하
        {
            PlayerController.instance.SetViewPoint(2);
        }
        else // 중
        {
            PlayerController.instance.SetViewPoint(4);
        }
    }

    /// <summary>
    /// 
    ///               [no use] 
    /// 
    /// 버튼 입력에 의해 시점을 순차 변환하는 함수
    /// </summary>
    public void ChangeViewPoint()
    {
        // 튜토리얼 중 시점 전환 불가
        if (PlayerPrefsData.instance.isTutorialFinish == 0) return;

        int key = PlayerController.instance.curViewPoint;
        if (key < 4) key++;
        else key = 1;


        PlayerController.instance.SetViewPoint(key);
    }

    /// <summary>
    /// Setting 탭 활성화/비활성화 함수
    /// </summary>
    public void SettingTap()
    {
        if(settingUI.activeSelf == true)
        {
            // 활성화된 설정 탭 초기화
            joyKeyCustomizer.TabSwitch((int)SettingTab.SelectDevice);

            settingUI.SetActive(false);
            isCanvasOn = false;

            StopCoroutine(BtnSelectChecker());

            PlayerController.instance.SetViewPoint(PlayerController.instance.curViewPoint);
        }
        else
        {
            settingUI.SetActive(true);
            MenuTab();
            isCanvasOn = true;
            
            StartCoroutine(BtnSelectChecker());
        }
    }

    public GameObject ButtonHighlightStop()
    {
        GameObject obj = eventSystem.currentSelectedGameObject;
        eventSystem.SetSelectedGameObject(null);
        return obj;
    }

    public void ButtonHighlightStart()
    {
        eventSystem.SetSelectedGameObject(settingUI.transform.GetComponentInChildren<Button>().gameObject);
    }

    /// <summary>
    /// 셋팅 탭 버튼 선택 하이라이팅 재개
    /// </summary>
    public void StartCoBtnSelectChecker()
    {
        StartCoroutine(BtnSelectChecker());
    }

    IEnumerator BtnSelectChecker()
    {
        ButtonHighlightStart();

        content.localPosition = new Vector3(0, -285 -150 +150 * 3, 0);
        float timer = 0;
        GameObject beforeSelect = null;
        int n = 3;

        while (settingUI.activeSelf && eventSystem.currentSelectedGameObject != null)
        {
            timer += Time.deltaTime;

            if (beforeSelect != eventSystem.currentSelectedGameObject)
            {
                timer = 0;
                beforeSelect = eventSystem.currentSelectedGameObject;
            }

            
            string numb = eventSystem.currentSelectedGameObject.transform.parent.name;

            numb = numb.Substring(numb.Length - 1, 1);

            int saveN = n;
            if (!int.TryParse(numb, out n)) { n = saveN; }

            content.localPosition = new Vector3(0,
                Mathf.Lerp(
                    content.localPosition.y,
                    -285 - 150 + 150 * n,
                    timer
                    ),
                0);
            
            //tabButton.SetActive(false);

            yield return null;
        }
    }

    /// <summary>
    /// 게임 종료. PlayerPrefs 데이터 저장 과정 필요
    /// </summary>
    public void GameExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif

    }
#endregion
    
#region NoticeUI

    /// <summary>
    /// 알림 메시지를 전달하는 함수
    /// </summary>
    /// <param name="msg">전달할 메시지</param>
    /// <param name="_timer">해당 메시지를 출력할 시간. default로 전달시 Off 전까지 계속 떠있게 됨</param>
    public void OpenNoticeMessage(string msg, float _timer = 0f)
    {
        if (SceneManager.GetActiveScene().name != "CityScene") return;
        topPos.SetActive(true);
        noticeMessage.text = msg;
        nmc.timer = _timer;
        
    }

    /// <summary>
    /// 알림 메시지 창을 종료하는 함수
    /// </summary>
    public void OffNoticeMessage()
    {
        if(topPos != null)
        topPos.SetActive(false);
    }


#endregion

#region EffectUI

    public void CallPhotographEff()
    {
        StartCoroutine(PhotographEff());
    }

    IEnumerator PhotographEff()
    {
        Image targetImg = effUI.transform.Find("PhotographEff").GetComponent<Image>();

        targetImg.gameObject.SetActive(true);

        float a = 1;
        targetImg.color = new Color(1, 1, 1, a);
        
        while(targetImg.color.a > 0.1f)
        {
            a = Mathf.Lerp(a, 0, Time.deltaTime * 2);
            targetImg.color = new Color(1, 1, 1, a);

            yield return null;
        }

        targetImg.gameObject.SetActive(false);
    }


    public Image GetFadeEffImg()
    {
        return effUI.transform.Find("FadeEff").GetComponent<Image>();
    }

#endregion

}
