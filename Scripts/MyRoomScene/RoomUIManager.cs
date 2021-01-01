using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/************************************
* 
* 설명:
*   마이룸의 UI 조작을 담당하는 스크립트
* 
************************************/

// # Need SoundEffect.cs #

public class RoomUIManager : MonoBehaviour
{
    public static RoomUIManager instance;

    public GameObject keyboard_eventSystem;
    public GameObject joystick_eventSystem;
    public EventSystem eventSystem;
    [Space]
    public GameObject timeReputeUIGroup;
    public GameObject normalMenuUIGroup;
    public GameObject interiorMenuUIGroup;
    public GameObject furnitureMenuUIGroup;

    [Header("Control Guide UI")]
    public GameObject leftVerticalKeyGuide;
    public GameObject leftHorizonKeyGuide;
    public GameObject rightVerticalKeyGuide;
    public GameObject rightHorizonKeyGuide;
    public GameObject interactKeyGuide;
    public Text interactKeyText;

    [Header("Sound")]
    public AudioClip menuOpen;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        UpdateEventSystem();
    }

    private void UpdateEventSystem()
    {
        if (InputDeviceChecker.instance.GetCurDevice() == InputDeviceChecker.INPUT_DEVICE.Keyboard)
            eventSystem = IngameCanvasManager.instance.eventSystem = keyboard_eventSystem.GetComponent<EventSystem>();
        else if (InputDeviceChecker.instance.GetCurDevice() == InputDeviceChecker.INPUT_DEVICE.Joystick)
            eventSystem = IngameCanvasManager.instance.eventSystem = joystick_eventSystem.GetComponent<EventSystem>();
    }

    public void OpenUI(GameObject UI) // UI 활성화 함수
    {
        UI.SetActive(true);
    }

    public void CloseUI(GameObject UI) // UI 비활성화 함수
    {
        UI.SetActive(false);
    }

    public void SelectChildUI(GameObject UI) // 자식 UI 하이라이트 함수
    {
        StartCoroutine(SelectChildUI2(UI)); // 하단의 IEnumerator 참조
    }

    private IEnumerator SelectChildUI2(GameObject UI)
    {
        yield return null; // 아래 문장들은 다음 프레임에 실행
        eventSystem.SetSelectedGameObject(UI.GetComponentInChildren<Button>().gameObject); // 최상단 버튼 하이라이트
    }

    public void NoSelectedUI()
    {
        eventSystem.SetSelectedGameObject(null);
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

    public void UpdateControlGuide() // 컨트롤 가이드 업데이트 - 메뉴를 열거나 닫을 때, 가구를 잡거나 놓았을 때 작동
    {
        if (MyRoomManager.instance.modifyMode)
        {
            if (MyRoomManager.instance.menuMode == false)
            {
                if (MyRoomManager.instance.PickedObject)
                {
                    RevealGuideUI(leftVerticalKeyGuide);
                    RevealGuideUI(leftHorizonKeyGuide);
                    RevealGuideUI(rightVerticalKeyGuide);
                    RevealGuideUI(rightHorizonKeyGuide);
                    RevealGuideUI(interactKeyGuide, interactKeyText, "가구 놓기");
                }
                else
                {
                    RevealGuideUI(leftVerticalKeyGuide);
                    HideGuideUI(leftHorizonKeyGuide);
                    RevealGuideUI(rightVerticalKeyGuide);
                    RevealGuideUI(rightHorizonKeyGuide);
                    RevealGuideUI(interactKeyGuide, interactKeyText, "가구 선택\nor 메뉴 열기");
                }
            }
            else
            {
                HideGuideUI(leftVerticalKeyGuide);
                HideGuideUI(leftHorizonKeyGuide);
                HideGuideUI(rightVerticalKeyGuide);
                HideGuideUI(rightHorizonKeyGuide);
                HideGuideUI(interactKeyGuide);
            }
        }
        else
        {
            HideGuideUI(leftVerticalKeyGuide);
            HideGuideUI(leftHorizonKeyGuide);
            HideGuideUI(rightVerticalKeyGuide);
            HideGuideUI(rightHorizonKeyGuide);
            HideGuideUI(interactKeyGuide);
        }
    }

    public IEnumerator OpenNormalMenu()
    {
        MyRoomManager.instance.EnableMenuMode(); // 메뉴 선택 모드 활성화
        yield return null; // 아래 문장들은 다음 프레임에 실행
        timeReputeUIGroup.SetActive(true); // 시간 & 평판 표시 활성화
        normalMenuUIGroup.SetActive(true); // 일반 메뉴 활성화
        eventSystem.SetSelectedGameObject(normalMenuUIGroup.GetComponentInChildren<Button>().gameObject); // 최상단 버튼 하이라이트

        SoundEffect.instance.audioSource.PlayOneShot(menuOpen);
    }

    public IEnumerator OpenInteriorMenu()
    {
        MyRoomManager.instance.EnableMenuMode(); // 메뉴 선택 모드 활성화
        yield return null; // 아래 문장들은 다음 프레임에 실행
        timeReputeUIGroup.SetActive(true); // 시간 & 평판 표시 활성화
        interiorMenuUIGroup.SetActive(true); // 가구 편집 모드 메뉴 활성화 (가구 불러오기, 편집 종료)
        eventSystem.SetSelectedGameObject(interiorMenuUIGroup.GetComponentInChildren<Button>().gameObject); // 최상단 버튼 하이라이트
        UpdateControlGuide();

        SoundEffect.instance.audioSource.PlayOneShot(menuOpen);
    }

    public IEnumerator OpenFurnitureMenu()
    {
        MyRoomManager.instance.EnableMenuMode(); // 메뉴 선택 모드 활성화
        yield return null; // 아래 문장들은 다음 프레임에 실행
        furnitureMenuUIGroup.SetActive(true); // 가구 선택했을 때 전용 메뉴 활성화 (편집, 삭제, 돌아가기)
        eventSystem.SetSelectedGameObject(furnitureMenuUIGroup.GetComponentInChildren<Button>().gameObject); // 최상단 버튼 하이라이트
        UpdateControlGuide();

        SoundEffect.instance.audioSource.PlayOneShot(menuOpen);
    }
}
