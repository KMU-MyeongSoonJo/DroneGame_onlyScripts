using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/************************************
* 
* 설명:
*   꾸미기 씬에서 사용하는 변수들을 관리하는 스크립트
* 
************************************/

public class DecorationManager : MonoBehaviour
{
    public static DecorationManager instance;

    public List<DecorationItem> droneDecorations;
    public List<DecorationItem> operatorDecorations;
    [Space]
    public GameObject droneDecortationUI;
    public GameObject droneShop;
    public GameObject operatorShop;
    
    [Header("Control Guide UI")]
    public GameObject upKeyGuide;
    public Text upKeyText;
    public GameObject downKeyGuide;
    public Text downKeyText;
    public GameObject leftKeyGuide;
    public Text leftKeyText;
    public GameObject rightKeyGuide;
    public Text rightKeyText;
    public GameObject interactKeyGuide;
    public Text interactKeyText;

    [Header("Sound Effect")]
    public AudioClip openMenu;
    public AudioClip closeMenu;
    public AudioClip highlight;
    public AudioClip click;

    private enum Control1 { Camera, UI }
    private Control1 control1 = Control1.Camera;
    private enum Control2 { Drone, Operator }
    private Control2 control2;

    private int droneIndex; // 현재 n번째 드론 꾸미기 아이템 선택중
    private int operatorIndex; // 현재 n번째 오퍼레이터 꾸미기 아이템 선택중

    private float inputDelay; // > 0 일 때, UI을 컨트롤할 때, 입력을 받지 않습니다.
    private bool throttleNeutral;
    private bool yawNeutral;
    private bool pitchNeutral;
    private bool rollNeutral;

    private void Awake()
    {
        instance = this;

        foreach(DecorationItem item in droneShop.GetComponentsInChildren<DecorationItem>()) // 상점에 있는 드론 꾸미기 아이템 리스트 만들기
            droneDecorations.Add(item);

        foreach (DecorationItem item in operatorShop.GetComponentsInChildren<DecorationItem>()) // 상점에 있는 오퍼레이터 꾸미기 아이템 리스트 만들기
            operatorDecorations.Add(item);
    }

    private void OnEnable()
    {
        PlayerController.instance.dm.droneHp = PlayerController.instance.dm.__droneInitHp__;
    }

    private void Start()
    {
        UpdateControlGuide(); // 컨트롤 가이드 초기화
    }

    private void Update()
    {
        UpdateDelay(); // InputDelay > 0 -> InputDelay가 0이하가 될 때까지 감소

        #region GET INPUT
        if (Input.GetButtonDown("Vertical"))
        {
            if (Input.GetAxis("Vertical") > 0)
                W();
            else
                S();
        }

        if (throttleNeutral == true && Input.GetAxis("Throttle") != 0) // 조이스틱 GetButtonDown 수동 인식
        {
            if (Input.GetAxis("Throttle") > 0.5f)
                W();
            if (Input.GetAxis("Throttle") < -0.5f)
                S();
            throttleNeutral = false;
        }
        else if (Mathf.Abs(Input.GetAxis("Throttle")) <= 0.5f)
            throttleNeutral = true;

        if (Input.GetButtonDown("Horizontal"))
        {
            if (Input.GetAxis("Horizontal") > 0)
                D();
            else
                A();
        }

        if (yawNeutral == true && Input.GetAxis("Yaw") != 0) // 조이스틱 GetButtonDown 수동 인식
        {
            if (Input.GetAxis("Yaw") > 0.5f)
                D();
            if (Input.GetAxis("Yaw") < -0.5f)
                A();
            yawNeutral = false;
        }
        else if (Mathf.Abs(Input.GetAxis("Yaw")) <= 0.5f)
            yawNeutral = true;

        if (Input.GetButtonDown("Vertical_r"))
        {
            if (Input.GetAxis("Vertical_r") > 0)
                K();
            else
                I();
        }

        if (pitchNeutral == true && Input.GetAxis("Pitch") != 0) // 조이스틱 GetButtonDown 수동 인식
        {
            if (Input.GetAxis("Pitch") > 0.5f)
                K();
            if (Input.GetAxis("Pitch") < -0.5f)
                I();
            pitchNeutral = false;
        }
        else if (Mathf.Abs(Input.GetAxis("Pitch")) <= 0.5f)
            pitchNeutral = true;

        if (Input.GetButtonDown("Horizontal_r"))
        {
            if (Input.GetAxis("Horizontal_r") > 0)
                L();
            else
                J();
        }

        if (rollNeutral == true && Input.GetAxis("Roll") != 0) // 조이스틱 GetButtonDown 수동 인식
        {
            if (Input.GetAxis("Roll") > 0.5f)
                L();
            if (Input.GetAxis("Roll") < -0.5f)
                J();
            rollNeutral = false;
        }
        else if (Mathf.Abs(Input.GetAxis("Roll")) <= 0.5f)
            rollNeutral = true;

        if(InputDeviceChecker.instance.Interact())
            Space();
        #endregion
    }

    private void W()
    {
        switch (control2)
        {
            case Control2.Drone:
                if (inputDelay <= 0)
                {
                    control2 = Control2.Operator;
                    droneShop.GetComponent<Animator>().SetBool("reveal", false);
                    operatorShop.GetComponent<Animator>().SetBool("reveal", true);
                    UpdateScaleOfItem(); // 현재 선택 중인 아이템을 크게 표시해주는 함수
                    SetDelay(0.25f);
                    UpdateControlGuide(); // 조작 가이드 업데이트

                    SoundEffect.instance.audioSource.PlayOneShot(highlight);
                }
                return;
        }
    }

    private void S()
    {
        switch (control1)
        {
            case Control1.Camera:
                switch (DecorationRoomCamera.instance.viewMode)
                {
                    case DecorationRoomCamera.ViewMode.lookDrone:
                    case DecorationRoomCamera.ViewMode.lookCharacter:
                        DecorationRoomCamera.instance.viewMode = DecorationRoomCamera.ViewMode.lookDefault;

                        SoundEffect.instance.audioSource.PlayOneShot(highlight);

                        UpdateControlGuide(); // 조작 가이드 업데이트
                        return;
                    case DecorationRoomCamera.ViewMode.lookDefault:
                        switch (Recorder.instance.previousScene)
                        {
                            case "My Room":
                                LoadingManager.LoadScene("My Room", SceneManager.GetActiveScene().name);
                                break;
                            default:
                                LoadingManager.LoadScene("IngameScene", SceneManager.GetActiveScene().name);
                                break;
                        }
                        
                        SoundEffect.instance.audioSource.PlayOneShot(closeMenu);

                        return;
                }
                return;
            case Control1.UI:
                switch (control2)
                {
                    case Control2.Drone:
                        if (inputDelay <= 0)
                        {
                            CloseUI(droneDecortationUI, Control1.Camera); // 꾸미기 UI를 닫고 카메라를 조종 시작
                            DecorationRoomCamera.instance.viewMode = DecorationRoomCamera.ViewMode.lookDefault;
                            UpdateControlGuide(); // 조작 가이드 업데이트

                            SoundEffect.instance.audioSource.PlayOneShot(closeMenu);
                        }
                        return;
                    case Control2.Operator:
                        if (inputDelay <= 0)
                        {
                            control2 = Control2.Drone;
                            droneShop.GetComponent<Animator>().SetBool("reveal", true);
                            operatorShop.GetComponent<Animator>().SetBool("reveal", false);
                            UpdateScaleOfItem(); // 현재 선택 중인 아이템을 크게 표시해주는 함수
                            SetDelay(0.25f);
                            UpdateControlGuide(); // 조작 가이드 업데이트

                            SoundEffect.instance.audioSource.PlayOneShot(highlight);
                        }
                        return;
                }
                return;
        }
    }

    private void A()
    {
        switch (control1)
        {
            case Control1.Camera:
                DecorationRoomCamera.instance.GoLeft(); // 카메라 왼쪽으로 이동
                UpdateControlGuide(); // 조작 가이드 업데이트

                SoundEffect.instance.audioSource.PlayOneShot(highlight);
                return;
            case Control1.UI:
                switch (control2)
                {
                    case Control2.Drone:
                        if (inputDelay <= 0)
                        {
                            DecreaseDroneIndex(); // 현재 선택 중인 index -1
                            SetDelay(0.25f);
                            UpdateControlGuide(); // 조작 가이드 업데이트

                            SoundEffect.instance.audioSource.PlayOneShot(highlight);
                        }
                        return;
                    case Control2.Operator:
                        if (inputDelay <= 0)
                        {
                            DecreaseOperatorIndex(); // 현재 선택 중인 index -1
                            SetDelay(0.25f);
                            UpdateControlGuide(); // 조작 가이드 업데이트

                            SoundEffect.instance.audioSource.PlayOneShot(highlight);
                        }
                        return;
                }
                return;
        }
    }

    private void D()
    {
        switch (control1)
        {
            case Control1.Camera:
                DecorationRoomCamera.instance.GoRight(); // 카메라 오른쪽으로 이동
                UpdateControlGuide(); // 조작 가이드 업데이트

                SoundEffect.instance.audioSource.PlayOneShot(highlight);
                return;
            case Control1.UI:
                switch (control2)
                {
                    case Control2.Drone:
                        if (inputDelay <= 0)
                        {
                            IncreaseDroneIndex();  // 현재 선택 중인 index +1
                            SetDelay(0.25f);
                            UpdateControlGuide(); // 조작 가이드 업데이트

                            SoundEffect.instance.audioSource.PlayOneShot(highlight);
                        }
                        return;
                    case Control2.Operator:
                        if (inputDelay <= 0)
                        {
                            IncreaseOperatorIndex(); // 현재 선택 중인 index +1
                            SetDelay(0.25f);
                            UpdateControlGuide(); // 조작 가이드 업데이트

                            SoundEffect.instance.audioSource.PlayOneShot(highlight);
                        }
                        return;
                }
                return;
        }
    }

    private void I()
    {
        W();
    }

    private void K()
    {
        S();
    }

    private void J()
    {
        A();
    }

    private void L()
    {
        D();
    }

    private void Space()
    {
        switch (control1)
        {
            case Control1.Camera:
                switch (DecorationRoomCamera.instance.viewMode)
                {
                    case DecorationRoomCamera.ViewMode.lookDrone:
                        OpenUI(droneDecortationUI, Control1.UI, Control2.Drone); // 꾸미기 UI를 열고 카메라를 조정하는 대신 UI를 조정하기 시작
                        UpdatePositionOfItem(); // 모든 꾸미기 아이템 UI의 위치 업데이트
                        UpdateScaleOfItem(); // 현재 선택 중인 아이템을 크게 표시해주는 함수
                        UpdateControlGuide(); // 조작 가이드 업데이트

                        SoundEffect.instance.audioSource.PlayOneShot(openMenu);

                        return;
                }
                return;
            case Control1.UI:
                TryBuy();
                return;
        }
    }

    private void IncreaseDroneIndex()
    {
        if (droneIndex < droneDecorations.Count - 1)
            droneIndex += 1;

        UpdateDestinationOfItem(); // 모든 꾸미기 아이템 UI의 위치 업데이트
        UpdateScaleOfItem(); // 현재 선택 중인 아이템을 크게 표시해주는 함수
    }

    private void DecreaseDroneIndex()
    {
        if (droneIndex > 0)
            droneIndex -= 1;

        UpdateDestinationOfItem();
        UpdateScaleOfItem();
    }

    private void IncreaseOperatorIndex()
    {
        if (operatorIndex < operatorDecorations.Count - 1)
            operatorIndex += 1;

        UpdateDestinationOfItem();
        UpdateScaleOfItem();
    }

    private void DecreaseOperatorIndex()
    {
        if (operatorIndex > 0)
            operatorIndex -= 1;

        UpdateDestinationOfItem();
        UpdateScaleOfItem();
    }

    private void UpdateDestinationOfItem() // 모든 꾸미기 아이템 UI의 목적지 업데이트 (Set Destination 사용, 서서히 이동)
    {
        switch (control2)
        {
            case Control2.Drone:
                for (int i = 0; i < droneIndex - 1; i++)
                    droneDecorations[i].GetComponent<DecorationItem>().SetDestination(new Vector2(-1400, 0)); // 상점 슬롯이 2칸 이상 차이나는 이전 아이템은 왼쪽에 보이지 않게 표시

                if (droneIndex >= 1)
                    droneDecorations[droneIndex - 1].GetComponent<DecorationItem>().SetDestination(new Vector2(-700, 0)); // 바로 이전 아이템은 왼쪽에 표시

                droneDecorations[droneIndex].GetComponent<DecorationItem>().SetDestination(new Vector2(0, 0)); // 선택중인 아이템은 중앙에 표시

                if (droneIndex <= droneDecorations.Count - 2)
                    droneDecorations[droneIndex + 1].GetComponent<DecorationItem>().SetDestination(new Vector2(700, 0)); // 바로 다음 아이템은 오른쪽에 표시

                for (int i = droneIndex + 2; i < droneDecorations.Count; i++)
                    droneDecorations[i].GetComponent<DecorationItem>().SetDestination(new Vector2(1400, 0));  // 상점 슬롯이 2칸 이상 차이나는 다음 아이템은 오른쪽에 보이지 않게 표시

                return;
            case Control2.Operator:
                for (int i = 0; i < operatorIndex - 1; i++)
                    operatorDecorations[i].GetComponent<DecorationItem>().SetDestination(new Vector2(-1400, 0)); // 상점 슬롯이 2칸 이상 차이나는 이전 아이템은 왼쪽에 보이지 않게 표시

                if (operatorIndex >= 1)
                    operatorDecorations[operatorIndex - 1].GetComponent<DecorationItem>().SetDestination(new Vector2(-700, 0)); // 바로 이전 아이템은 왼쪽에 표시

                operatorDecorations[operatorIndex].GetComponent<DecorationItem>().SetDestination(new Vector2(0, 0)); // 선택중인 아이템은 중앙에 표시

                if (operatorIndex <= operatorDecorations.Count - 2)
                    operatorDecorations[operatorIndex + 1].GetComponent<DecorationItem>().SetDestination(new Vector2(700, 0)); // 바로 다음 아이템은 오른쪽에 표시

                for (int i = operatorIndex + 2; i < operatorDecorations.Count; i++)
                    operatorDecorations[i].GetComponent<DecorationItem>().SetDestination(new Vector2(1400, 0));  // 상점 슬롯이 2칸 이상 차이나는 다음 아이템은 오른쪽에 보이지 않게 표시

                return;
        }
    }

    private void UpdatePositionOfItem() // 모든 꾸미기 아이템 UI의 위치 업데이트 (Set Position 사용, 즉시 이동)
    {
        for (int i = 0; i < droneIndex - 1; i++)
            droneDecorations[i].GetComponent<DecorationItem>().SetPosition(new Vector2(-1400, 0)); // 상점 슬롯이 2칸 이상 차이나는 이전 아이템은 왼쪽에 보이지 않게 표시

        if (droneIndex >= 1)
            droneDecorations[droneIndex - 1].GetComponent<DecorationItem>().SetPosition(new Vector2(-700, 0)); // 바로 이전 아이템은 왼쪽에 표시

        droneDecorations[droneIndex].GetComponent<DecorationItem>().SetPosition(new Vector2(0, 0)); // 선택중인 아이템은 중앙에 표시

        if (droneIndex <= droneDecorations.Count - 2)
            droneDecorations[droneIndex + 1].GetComponent<DecorationItem>().SetPosition(new Vector2(700, 0)); // 바로 다음 아이템은 오른쪽에 표시

        for (int i = droneIndex + 2; i < droneDecorations.Count; i++)
            droneDecorations[i].GetComponent<DecorationItem>().SetPosition(new Vector2(1400, 0));  // 상점 슬롯이 2칸 이상 차이나는 다음 아이템은 오른쪽에 보이지 않게 표시


        for (int i = 0; i < operatorIndex - 1; i++)
            operatorDecorations[i].GetComponent<DecorationItem>().SetPosition(new Vector2(-1400, 0)); // 상점 슬롯이 2칸 이상 차이나는 이전 아이템은 왼쪽에 보이지 않게 표시

        if (operatorIndex >= 1)
            operatorDecorations[operatorIndex - 1].GetComponent<DecorationItem>().SetPosition(new Vector2(-700, 0)); // 바로 이전 아이템은 왼쪽에 표시

        operatorDecorations[operatorIndex].GetComponent<DecorationItem>().SetPosition(new Vector2(0, 0)); // 선택중인 아이템은 중앙에 표시

        if (operatorIndex <= operatorDecorations.Count - 2)
            operatorDecorations[operatorIndex + 1].GetComponent<DecorationItem>().SetPosition(new Vector2(700, 0)); // 바로 다음 아이템은 오른쪽에 표시

        for (int i = operatorIndex + 2; i < operatorDecorations.Count; i++)
            operatorDecorations[i].GetComponent<DecorationItem>().SetPosition(new Vector2(1400, 0));  // 상점 슬롯이 2칸 이상 차이나는 다음 아이템은 오른쪽에 보이지 않게 표시
    }

    private void UpdateScaleOfItem() // 현재 선택 중인 아이템을 서서히 크게 표시해주고 함수, 다른 아이템은 원래 크기로 만들어주는 함수
    {
        switch (control2)
        {
            case Control2.Drone:
                foreach(DecorationItem item in droneDecorations) // 현재 카테고리에 있는 모든 아이템 중
                {
                    if (item == droneDecorations[droneIndex])
                        item.SetBoolAnimation("upscaling", true); // 현재 선택중인 아이템은 크게 표시
                    else
                        item.SetBoolAnimation("upscaling", false); // 현재 선택중인 아이템이 아닌 것은 작게 표시
                }
                foreach (DecorationItem item in operatorDecorations) // 다른 카테고리에 있는 모든 아이템을
                {
                    item.SetBoolAnimation("upscaling", false); // 작게 표시
                }
                return;
            case Control2.Operator:
                foreach (DecorationItem item in droneDecorations) // 다른 카테고리에 있는 모든 아이템을
                {
                    item.SetBoolAnimation("upscaling", false); // 작게 표시
                }
                foreach (DecorationItem item in operatorDecorations) // 현재 카테고리에 있는 모든 아이템 중
                {
                    if (item == operatorDecorations[operatorIndex])
                        item.SetBoolAnimation("upscaling", true); // 현재 선택중인 아이템은 크게 표시
                    else
                        item.SetBoolAnimation("upscaling", false); // 현재 선택중인 아이템이 아닌 것은 작게 표시
                }
                return;
        }
    }

    private void UpdateItemStatus() // 현재 선택 중인 카테고리의 꾸미기 아이템들의 상태 확인 (구입 필요, 보유중, 착용중)
    {
        switch (control2)
        {
            case Control2.Drone:
                foreach (DecorationItem item in droneDecorations)
                    item.CheckItemStatus();
                return;
            case Control2.Operator:
                foreach (DecorationItem item in operatorDecorations)
                    item.CheckItemStatus();
                return;
        }
    }

    private void TryBuy() // 아이템 구매 시도
    {
        switch (control2)
        {
            case Control2.Drone:
                if (droneDecorations[droneIndex].locked) // 아이템 구매
                {
                    if (PlayerPrefsData.instance.repute >= droneDecorations[droneIndex].reputeCost)
                    {
                        PlayerPrefs.SetInt(droneDecorations[droneIndex].name, 1); // 구입한 아이템 표시
                        PlayerPrefs.SetString("droneDecoration", droneDecorations[droneIndex].name); // 아이템 착용
                        DroneSkinController.instance.UpdateDroneSkin();
                        PlayerPrefsData.instance.repute -= droneDecorations[droneIndex].reputeCost; // 평판 소모
                        PlayerPrefs.SetInt("repute", PlayerPrefsData.instance.repute);
                        UpdateItemStatus();
                        UpdateControlGuide();

                        SoundEffect.instance.audioSource.PlayOneShot(click);

                        return; // 아이템 구매 성공
                    }
                    else
                        return; // 아이템 구매 실패
                }
                else // 아이템 착용
                {
                    if (PlayerPrefs.GetString("droneDecoration") != droneDecorations[droneIndex].name)
                    {
                        PlayerPrefs.SetString("droneDecoration", droneDecorations[droneIndex].name); // 아이템 착용
                        DroneSkinController.instance.UpdateDroneSkin();
                        UpdateItemStatus();

                        SoundEffect.instance.audioSource.PlayOneShot(click);
                    }
                    return; // 아이템 착용
                }

            case Control2.Operator:
                if (operatorDecorations[operatorIndex].locked) // 아이템 구매
                {
                    if (PlayerPrefsData.instance.repute >= operatorDecorations[operatorIndex].reputeCost)
                    {
                        PlayerPrefs.SetInt(operatorDecorations[operatorIndex].name, 1); // 구입한 아이템 표시
                        PlayerPrefs.SetString("operatorDecoration", operatorDecorations[operatorIndex].name); // 아이템 착용
                        PlayerPrefsData.instance.repute -= operatorDecorations[operatorIndex].reputeCost; // 평판 소모
                        PlayerPrefs.SetInt("repute", PlayerPrefsData.instance.repute);
                        UpdateItemStatus();
                        UpdateControlGuide();

                        SoundEffect.instance.audioSource.PlayOneShot(click);

                        return; // 아이템 구매 성공
                    }
                    else
                        return; // 아이템 구매 실패
                }
                else // 아이템 착용
                {
                    if (PlayerPrefs.GetString("operatorDecoration") != operatorDecorations[operatorIndex].name)
                    {
                        PlayerPrefs.SetString("operatorDecoration", operatorDecorations[operatorIndex].name); // 아이템 착용
                        UpdateItemStatus();

                        SoundEffect.instance.audioSource.PlayOneShot(click);
                    }

                    return; // 아이템 착용
                }
        }
    }

    private void SetDelay(float _delaytime)
    {
        inputDelay = _delaytime;
    }

    private void UpdateDelay()
    {
        if (inputDelay > 0)
            inputDelay -= Time.deltaTime;
    }

    private void OpenUI(GameObject UI)
    {
        UI.SetActive(true);
    }

    private void OpenUI(GameObject UI, Control1 _control1)
    {
        UI.SetActive(true);
        control1 = _control1;
    }

    private void OpenUI(GameObject UI, Control1 _control1, Control2 _control2)
    {
        UI.SetActive(true);
        control1 = _control1;
        control2 = _control2;
    }

    private void CloseUI(GameObject UI)
    {
        UI.SetActive(false);
    }

    private void CloseUI(GameObject UI, Control1 _control)
    {
        UI.SetActive(false);
        control1 = _control;
    }

    private void CloseUI(GameObject UI, Control1 _control1, Control2 _control2)
    {
        UI.SetActive(false);
        control1 = _control1;
        control2 = _control2;
    }

    private void RevealGuideUI(GameObject UI) // 컨트롤 가이드 UI 드러내기
    {
        UI.GetComponent<Animator>().SetBool("reveal", true);
    }

    private void RevealGuideUI(GameObject UI, Text text, string _guidetext) // 컨트롤 가이드 UI 드러내기 (가이드 UI, 가이드 UI의 Text, 가이드 UI의 Text에 적용할 string)
    {
        UI.GetComponent<Animator>().SetBool("reveal", true);
        text.text = _guidetext;
    }

    private void HideGuideUI(GameObject UI) // 컨트롤 가이드 UI 숨기기
    {
        UI.GetComponent<Animator>().SetBool("reveal", false);
    }

    private void UpdateControlGuide() // 컨트롤 가이드 업데이트
    {
        switch (control1)
        {
            case Control1.Camera:
                switch (DecorationRoomCamera.instance.viewMode)
                {
                    case DecorationRoomCamera.ViewMode.lookDrone:
                        HideGuideUI(upKeyGuide);
                        RevealGuideUI(downKeyGuide, downKeyText, "돌아가기");
                        HideGuideUI(leftKeyGuide);
                        RevealGuideUI(rightKeyGuide, rightKeyText, "선택");
                        RevealGuideUI(interactKeyGuide, interactKeyText, "드론 꾸미기");
                        return;
                    case DecorationRoomCamera.ViewMode.lookCharacter:
                        HideGuideUI(upKeyGuide);
                        RevealGuideUI(downKeyGuide, downKeyText, "돌아가기");
                        RevealGuideUI(leftKeyGuide, leftKeyText, "선택");
                        HideGuideUI(rightKeyGuide);
                        HideGuideUI(interactKeyGuide);
                        return;
                    case DecorationRoomCamera.ViewMode.lookDefault:
                        HideGuideUI(upKeyGuide);
                        RevealGuideUI(downKeyGuide, downKeyText, "나가기");
                        RevealGuideUI(leftKeyGuide, leftKeyText, "선택");
                        RevealGuideUI(rightKeyGuide, rightKeyText, "선택");
                        HideGuideUI(interactKeyGuide);
                        return;
                }
                return;
            case Control1.UI:
                switch (control2)
                {
                    case Control2.Drone:
                        RevealGuideUI(upKeyGuide, upKeyText, "오퍼레이터\n꾸미기");
                        RevealGuideUI(downKeyGuide, downKeyText, "돌아가기");

                        if (droneIndex >= 1)
                            RevealGuideUI(leftKeyGuide, leftKeyText, "선택");
                        else
                            HideGuideUI(leftKeyGuide);

                        if (droneIndex <= droneDecorations.Count - 2)
                            RevealGuideUI(rightKeyGuide, rightKeyText, "선택");
                        else
                            HideGuideUI(rightKeyGuide);

                        if (droneDecorations[droneIndex].locked)
                            RevealGuideUI(interactKeyGuide, interactKeyText, "구매");
                        else
                            RevealGuideUI(interactKeyGuide, interactKeyText, "착용");
                        return;
                    case Control2.Operator:
                        HideGuideUI(upKeyGuide);
                        RevealGuideUI(downKeyGuide, downKeyText, "드론 외형\n꾸미기");

                        if (operatorIndex >= 1)
                            RevealGuideUI(leftKeyGuide, leftKeyText, "선택");
                        else
                            HideGuideUI(leftKeyGuide);

                        if (operatorIndex <= operatorDecorations.Count - 2)
                            RevealGuideUI(rightKeyGuide, rightKeyText, "선택");
                        else
                            HideGuideUI(rightKeyGuide);

                        if (operatorDecorations[operatorIndex].locked)
                            RevealGuideUI(interactKeyGuide, interactKeyText, "구매");
                        else
                            RevealGuideUI(interactKeyGuide, interactKeyText, "착용");
                        return;
                }
                return;
        }
    }
}