using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum SettingTab
{
    SelectDevice, // 입력 장치 선택
    KeyCustom, // 입력 키 커스터마이징
    GameSetting, // 게임 설정 탭 - 볼륨 등.. 추후 추가 예정


}

public class JoyKeyCustomizer : MonoBehaviour { 

    [Header("Components")]
    public GameObject settingUI;

    [Header("Tabs")]
    public GameObject SelectDeviceTab; // select input device
    public GameObject KeyCustomTab; // customize input system
    public GameObject GameSettingTab; // 게임 설정 탭

    [Header("Buttons")]
    public GameObject ThrottleUp; public GameObject ThrottleDown; public GameObject ThrottleAxis;
    public GameObject YawUp, YawDown, YawAxis;
    public GameObject PitchUp, PitchDown, PitchAxis;
    public GameObject RollUp, RollDown, RollAxis;

    [Header("Notices")]
    public Text connectedDevice;
    public Text searchedDeviceList;
    [Space]
    public GameObject noticeLoading; // 키 입력 직전 안내 문구 알림창
    public GameObject noticeReset; // 초기화 여부 확인 알림창
    public GameObject noticeClose; // 종료시 저장되지 않은 데이터 알림창

    [Header("Axis Monitor")]
    public GameObject axisMonitor;
    public Text[] initValue;
    public Text[] curValue;


    // == Variables ==
    SettingTab curTab = SettingTab.SelectDevice;

    // 키 편집 과정 저장
    // ex) <Throttle, 4th axis>
    Dictionary<string, string> result = new Dictionary<string, string>();

    // 축 방향 정/역방향 저장
    // ex) <Throttle, false> : 역방향. 4th axis를 아래로 내려야 axis 값 증가.
    Dictionary<string, bool> isResultAxisMinus = new Dictionary<string, bool>();


    private void OnEnable()
    {
        // 드론 조종 모드에서 키 설정시 버그 야기.
        // 방지를 위해 플레이어 시점으로 변경
        if (PlayerController.instance.curViewPoint != 1) PlayerController.instance.SetViewPoint(1, true, true);

        // 현재 연결된 디바이스 정보 초기화
        connectedDevice.text = PlayerPrefs.HasKey("deviceName") ? PlayerPrefs.GetString("deviceName") : "Keyboard";


        StartCoroutine(SearchConnectedDeviceList());

        UpdateBtnText();
    }
    
    #region TabSwitch

    public void TabSwitch(int tabIdx)
    {
        switch (tabIdx)
        {
            case (int)SettingTab.SelectDevice: // Open Select Device Tab

                StopCoroutine(SearchConnectedDeviceList());
                StartCoroutine(SearchConnectedDeviceList());

                if (curTab == SettingTab.SelectDevice) break; // 이미 열려 있는 탭이라면
                curTab = SettingTab.SelectDevice;

                SelectDeviceTab.SetActive(true);
                KeyCustomTab.SetActive(false);
                GameSettingTab.SetActive(false);

                break;
            case (int)SettingTab.KeyCustom: // Open Key Custom Tab
                
                if (curTab == SettingTab.KeyCustom) break;
                curTab = SettingTab.KeyCustom;

                SelectDeviceTab.SetActive(false);
                KeyCustomTab.SetActive(true);
                GameSettingTab.SetActive(false);

                break;
            case (int)SettingTab.GameSetting:

                if (curTab == SettingTab.GameSetting) break;
                curTab = SettingTab.GameSetting;

                SelectDeviceTab.SetActive(false);
                KeyCustomTab.SetActive(false);
                GameSettingTab.SetActive(true);

                break;
        }
    }

    #endregion

    #region Select Input Device

    IEnumerator SearchConnectedDeviceList()
    {
        while (curTab == SettingTab.SelectDevice)
        {


            string[] list = Input.GetJoystickNames();

            searchedDeviceList.text = "";

            foreach (string s in list)
                searchedDeviceList.text += s + "\n";


            yield return new WaitForSeconds(0.5f);
        }
    }

    public void SetInputDevice(string deviceName)
    {
        print($"SetInputDevice func is called to {deviceName}");

        if (InputDeviceChecker.instance.SetInputDevice(deviceName))
        {
            // [device name] is connected. 
            connectedDevice.text = deviceName;

            // 드론 조종 디바이스 변경
            if (deviceName == "Keyboard")
                PlayerController.instance.dm.joystick_turned_on = false;
            else
                PlayerController.instance.dm.joystick_turned_on = true;
        }

        TalkManager.instance.eventSystem = InputDeviceChecker.instance.GetCurEventSystem();
        IngameCanvasManager.instance.eventSystem.SetSelectedGameObject(GetComponentInChildren<Button>().gameObject);
    }
    
    public void SelectDeviceTabClose()
    {
        IngameCanvasManager.instance.SettingTap();   
    }
    
    #endregion

    #region KeyCustom

    /// <summary>
    /// 키 편집 시작
    /// </summary>
    public void KeyCustomStart()
    {
        Input.ResetInputAxes();
        StartCoroutine(KeyCustom());
    }

    /// <summary>
    /// 키 편집 진행
    /// </summary>
    IEnumerator KeyCustom()
    {
        GameObject btn = IngameCanvasManager.instance.ButtonHighlightStop();

        noticeLoading.GetComponentInChildren<Text>().text = "잠시 후 인식이 시작됩니다.";
        noticeLoading.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        axisMonitor.SetActive(true);
        noticeLoading.SetActive(false);

        float[,] axis = new float[2,8];


        #region init

        axis[0, 0] = Input.GetAxis("X axis");
        axis[0, 1] = Input.GetAxis("Y axis");
        axis[0, 2] = Input.GetAxis("3rd axis");
        axis[0, 3] = Input.GetAxis("4th axis");
        axis[0, 4] = Input.GetAxis("5th axis");
        axis[0, 5] = Input.GetAxis("6th axis");
        axis[0, 6] = Input.GetAxis("7th axis");
        axis[0, 7] = Input.GetAxis("8th axis");

        initValue[0].text = axis[0, 0].ToString();
        initValue[1].text = axis[0, 1].ToString();
        initValue[2].text = axis[0, 2].ToString();
        initValue[3].text = axis[0, 3].ToString();
        initValue[4].text = axis[0, 4].ToString();
        initValue[5].text = axis[0, 5].ToString();
        initValue[6].text = axis[0, 6].ToString();
        initValue[7].text = axis[0, 7].ToString();

        #endregion

        // Space 를 누를 때까지 키 입력
        while (!Input.GetKeyDown(KeyCode.Space))
        {
            axis[1,0] = Input.GetAxis("X axis");
            axis[1,1] = Input.GetAxis("Y axis");
            axis[1,2] = Input.GetAxis("3rd axis");
            axis[1,3] = Input.GetAxis("4th axis");
            axis[1,4] = Input.GetAxis("5th axis");
            axis[1,5] = Input.GetAxis("6th axis");
            axis[1,6] = Input.GetAxis("7th axis");
            axis[1, 7] = Input.GetAxis("8th axis");

            curValue[0].text = axis[1, 0].ToString();
            curValue[1].text = axis[1, 1].ToString();
            curValue[2].text = axis[1, 2].ToString();
            curValue[3].text = axis[1, 3].ToString();
            curValue[4].text = axis[1, 4].ToString();
            curValue[5].text = axis[1, 5].ToString();
            curValue[6].text = axis[1, 6].ToString();
            curValue[7].text = axis[1, 7].ToString();

            Input.ResetInputAxes();

            yield return null;
        }


        // 가장 편차가 컸던 입력(Axis) 추적
        int targetIdx = 0;
        float maxGap = 0;

        for (int i = 0; i < 8; i++)
        {
            if (Mathf.Abs(axis[1,i] - axis[0,i]) > maxGap)
            {
                maxGap = Mathf.Abs(axis[1, i] - axis[0, i]);
                targetIdx = i;
            }
        }

        string axisName;
        switch (targetIdx)
        {
            case 0: axisName = "X axis"; break;
            case 1: axisName = "Y axis"; break;
            case 2: axisName = "3rd axis"; break;
            default: axisName = (targetIdx + 1).ToString() + "th axis"; break;
        }

        btn.GetComponentInChildren<Text>().text = axisName;
        btn.GetComponent<Image>().color = Color.red;

        string btnName = btn.transform.parent.name;
        btnName = btnName.Substring(0, btnName.Length - 2); // 이름 끝 공백+숫자 제외
        print("substring btnName : " + btnName);

        // 이미 변경 이력이 있는 key라면 다시 값 설정
        if (result.ContainsKey(btnName))
        { 
            result[btnName] = axisName;
            isResultAxisMinus[btnName] = axis[1, targetIdx] < 0 ? true : false;
        }

        // 처음 변경되는 key라면 변경 이력 추가
        else
        { 
            result.Add(btnName, axisName);
            isResultAxisMinus.Add(btnName, axis[1, targetIdx] < 0 ? true : false);
        }
        
        axisMonitor.SetActive(false);

        // Setting ui 버튼 하이라이팅 재개
        IngameCanvasManager.instance.StartCoBtnSelectChecker();
    }

    #region key custom save

    /// <summary>
    /// 키 편집 저장
    /// </summary>
    public void SettingSave()
    {

        // 중복 변경 여부 체크 및 저장

        List<string> values = new List<string>();

        foreach (var v in result.Keys)
        {
            if (values.Contains(result[v]))
            {
                // 중복 ! 저장 불가능. 초기화 후 세팅 창 닫기

                // 초기화
                result.Clear();
                isResultAxisMinus.Clear();

                return;
            }

            else
            {
                values.Add(result[v]);
            }
        }

        // 중복 X. 저장 프로세스 진행
        string Throttle = null, Yaw = null, Pitch = null, Roll = null;
        bool isThrottleReverse = false, isYawReverse = false, isPitchReverse = false, isRollReverse= false;
        for (int i = 0; i < result.Count; i++)
        {
            if (result.ContainsKey("Throttle"))
            {
                Throttle = result["Throttle"];
                isThrottleReverse = isResultAxisMinus["Throttle"];
            }
            if (result.ContainsKey("Yaw"))
            {
                Yaw = result["Yaw"];
                isYawReverse = isResultAxisMinus["Yaw"];
            }
            if (result.ContainsKey("Pitch"))
            {
                Pitch = result["Pitch"];
                isPitchReverse = isResultAxisMinus["Pitch"];
            }
            if (result.ContainsKey("Roll"))
            {
                Roll = result["Roll"];
                isRollReverse = isResultAxisMinus["Roll"];
            }
        }

        JoyKeyInfoContainer.instance.Write(Throttle, Yaw, Pitch, Roll,
            isThrottleReverse, isYawReverse, isPitchReverse, isRollReverse
            );

        // Dictionary 초기화
        result.Clear();
        isResultAxisMinus.Clear();

        print("저장 완료 ! ");

        JoyKeyInfoContainer.instance.Read();
        UpdateBtnText();
    }
    #endregion

    #region key custom reset

    /// <summary>
    /// 모든 키 설정 초기화
    /// </summary>
    public void SettingReset()
    {
        // == 정말 리셋하시겠습니까? == //
        IngameCanvasManager.instance.ButtonHighlightStop();

        settingUI.SetActive(false);
        noticeReset.SetActive(true);

        IngameCanvasManager.instance.eventSystem.SetSelectedGameObject(noticeReset.GetComponentInChildren<Button>().gameObject);
    }
    
    public void SettingResetYes()
    {
        noticeReset.SetActive(false);
        settingUI.SetActive(true);

        result.Clear();
        isResultAxisMinus.Clear();

        IngameCanvasManager.instance.StartCoBtnSelectChecker();

        JoyKeyInfoContainer.instance.WriteDefault();
        JoyKeyInfoContainer.instance.Read();
        UpdateBtnText();
    }

    public void SettingResetNo()
    {
        noticeReset.SetActive(false);
        settingUI.SetActive(true);

        IngameCanvasManager.instance.StartCoBtnSelectChecker();
    }

    #endregion

    #region key custom close

    public void KeyCustomTabClose()
    {

        // == 편집한게 남아있다면? ==
        if (result.Count > 0)
        {
            // == 정말 닫으시겠습니까? ==
            print($"저장되지 않은 정보가 {result.Count}개 리셋됩니다.");

            settingUI.SetActive(false);
            noticeClose.SetActive(true);

            IngameCanvasManager.instance.ButtonHighlightStop();
        }

        else
        {
            KeyCustomTabCloseYes();
        }

        IngameCanvasManager.instance.eventSystem.SetSelectedGameObject(noticeClose.GetComponentInChildren<Button>().gameObject);
    }

    public void KeyCustomTabCloseYes()
    {
        noticeClose.SetActive(false);
        settingUI.SetActive(true);

        // 데이터 삭제
        result.Clear();
        isResultAxisMinus.Clear();
        

        IngameCanvasManager.instance.SettingTap();
    }

    public void KeyCustomTabCloseNo()
    {
        noticeClose.SetActive(false);
        settingUI.SetActive(true);

        IngameCanvasManager.instance.StartCoBtnSelectChecker();
    }

    #endregion


    /// <summary>
    /// 각 버튼에 표시된 키 할당 정보를 업데이트
    /// </summary>
    void UpdateBtnText()
    {
        //ThrottleUp.GetComponent<Text>().text = "t";
        //ThrottleDown.GetComponent<Text>().text = "t";
        ThrottleAxis.GetComponentInChildren<Text>().text = JoyKeyInfoContainer.instance.GetAxisFromName("Throttle");
        ThrottleAxis.GetComponent<Image>().color = Color.white;
        

        //YawUp.GetComponent<Text>().text = "t";
        //YawDown.GetComponent<Text>().text = "t";
        YawAxis.GetComponentInChildren<Text>().text = JoyKeyInfoContainer.instance.GetAxisFromName("Yaw");
        YawAxis.GetComponent<Image>().color = Color.white;

        //PitchUp.GetComponent<Text>().text = "t";
        //PitchDown.GetComponent<Text>().text = "t";
        PitchAxis.GetComponentInChildren<Text>().text = JoyKeyInfoContainer.instance.GetAxisFromName("Pitch");
        PitchAxis.GetComponent<Image>().color = Color.white;

        //RollUp.GetComponent<Text>().text = "t";
        //RollDown.GetComponent<Text>().text = "t";
        RollAxis.GetComponentInChildren<Text>().text = JoyKeyInfoContainer.instance.GetAxisFromName("Roll");
        RollAxis.GetComponent<Image>().color = Color.white;
    }


    #endregion

    #region GameSetting

    public void GameSettingChangeGuiFixInfo()
    {
        // Gui Setting 오브젝트 찾기
        Transform tr = transform.GetChild(0).Find("GameSettingTab").GetChild(0).GetChild(0);
        
        // Unfix가 활성화 상태라면 GUI가 고정되어 있지 않은 상태 -> 고정
        if (tr.GetChild(0).gameObject.activeSelf)
        {
            tr.GetChild(0).gameObject.SetActive(false);
            tr.GetChild(1).gameObject.SetActive(true);

            IngameCanvasManager.instance.isBasicUILock = true;
        }

        else
        {
            tr.GetChild(0).gameObject.SetActive(true);
            tr.GetChild(1).gameObject.SetActive(false);

            IngameCanvasManager.instance.isBasicUILock = false;
        }

    }

    public void GameSettingChangePlaymode()
    {
        // Playmode 오브젝트 찾기
        Transform tr = transform.GetChild(0).Find("GameSettingTab").GetChild(0).GetChild(1);

        // 현재 RealMode 비행이라면 -> FreeMode 비행으로 변경
        if (tr.GetChild(0).gameObject.activeSelf)
        {
            tr.GetChild(0).gameObject.SetActive(false);
            tr.GetChild(1).gameObject.SetActive(true);

            // 조종 가능 거리
            PlayerController.instance.currentDroneControlMaxDistance = int.MaxValue;
        }
        else
        {
            tr.GetChild(0).gameObject.SetActive(true);
            tr.GetChild(1).gameObject.SetActive(false);

            // 조종 가능 거리 초기화
            PlayerController.instance.currentDroneControlMaxDistance = PlayerController.MAX_DRONE_CONTROL_DISTANCE;
        }

    }

    public void GameSettingTabClose()
    {        
        IngameCanvasManager.instance.SettingTap();
    }

    #endregion
}
