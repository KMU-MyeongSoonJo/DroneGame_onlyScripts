using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR;

/************************************
* 
* 작성자: 송창하
* 설명:
*   엑셀파일(CSV)에서 대화 정보를 불러와서 대화 내용을 UI에 출력해주는 스크립트
* 
************************************/

public class TalkManager : MonoBehaviour
{
    public static TalkManager instance;

    public EventSystem eventSystem;

    [Space]
    public GameObject TalkUIGroup; // 대화 UI 전체

    [Header("캐릭터")]
    public GameObject Character1Group; // 캐릭터1 그룹
    public Animator Charcter1Animator; // 캐릭터1 애니메이터
    public Image Character1Skin; // 캐릭터1
    public Image Character1Face; // 캐릭터1 표정
    public GameObject Character2Group; // 캐릭터2 그룹
    public Animator Charcter2Animator; // 캐릭터2 애니메이터
    public Image Character2Skin; // 캐릭터2
    public Image Character2Face; // 캐릭터2 표정

    [Header("텍스트")]
    public Text TalkPanelName; // 대화창 이름
    public Text TalkPanelText; // 대화창 텍스트
    public Animator TalkPanelArrow; // 대화창 화살표 (대사 출력이 끝났을 때 애니메이션 활성화)
    public float printSpeed; // 대화 글자 출력 속도 (낮을 수록 빠름, 타이핑 이펙트에 사용)

    [Header("선택지")]
    public GameObject SelectButton1; // 1번 선택지 text
    public GameObject SelectButton2; // 2번 선택지 text
    public GameObject SelectButton3; // 3번 선택지 text
    public GameObject SelectButton4; // 4번 선택지 text

    [Header("효과음")]
    public AudioClip talkSound; // 대화 중 효과음

    [Header("기타 UI")]
    public Image SkipDelayProgress; // 대화 넘기기 지연 진행도 이미지

    [HideInInspector] public bool talking; // 대화 중?
    [HideInInspector] public bool effecting; // 타이핑 이펙트 중?
    [HideInInspector] public bool forceSkip; // 대화 출력 완료시 강제로 다음 대화로 넘기기 (On & Off)
    [HideInInspector] public string OP_SPRITE_PATH = "Sprites/Characters/operator/"; // 오퍼레이터 이미지 경로

    private List<Dictionary<string, object>> CSVData; // CSV파일에서 가져온 데이터

    private string targetString; // 현재 대화 문장 전체
    private int CSVIndex; // 현재 CSV Index (CSV 기준으로 행)
    private int charIndex; // 현재 글자 Index (타이핑 이펙트에 사용)
    private int cutsceneStatus = 0; // 컷씬 화면 연출 효과의 상태
                                    // 0 -> 컷씬 효과를 주기 전 or 주지 않음
                                    // 1 -> 컷씬 효과 (서서히 어두워짐)
                                    // 2 -> 컷씬 효과 (서서히 밝아짐)
    private float skipDelayTime = 0; // 대화 넘기기 버튼을 누르는 시간 (뗄 경우 초기화)

    /// <summary>
    /// Input
    /// </summary>
    private bool rollNeutral;

    /// <summary>
    /// usedFuction[A] = B
    /// 이전 대화를 보고 다시 대화를 진행할때, 함수 중복 실행을 방지하기 위한 List<bool>
    /// A는 현재 대사가 CSV파일 내의 몇번째 대사인지 나타내는 수
    /// B는 현재 대화 내에서 현재 대사의 함수가 이미 호출했다면 true, 아니라면 false
    /// </summary>
    private List<bool> usedFunction;

    private void Awake()
    {
        instance = this;

        // 함수 사용 기록 초기화
        usedFunction = new List<bool>();
        for (int i = 0; i < 100; i++)
            usedFunction.Add(false);
    }

    private void Start()
    {
        eventSystem = InputDeviceChecker.instance.GetCurEventSystem();
    }

    private void LateUpdate()
    {
        if (talking) // 대화 매니저는 대화 중에만 작동
        {
            #region backup201229
            //if (Input.GetButtonDown("Horizontal_r"))
            //{
            //    if (Input.GetAxis("Horizontal_r") > 0)
            //        L();
            //    else
            //        J();
            //}

            //if (rollNeutral == true && Input.GetAxis("Roll") != 0) // 조이스틱 GetButtonDown 수동 인식
            //{
            //    if (Input.GetAxis("Roll") > 0.5f)
            //        L();
            //    if (Input.GetAxis("Roll") < -0.5f)
            //        J();
            //    rollNeutral = false;
            //}
            //else if (Mathf.Abs(Input.GetAxis("Roll")) <= 0.5f)
            //    rollNeutral = true;

            //if (Input.GetButton("Horizontal_r"))
            //{
            //    if (Input.GetAxis("Horizontal_r") > 0)
            //        LHold();
            //}

            //if (Input.GetAxis("Roll") != 0)
            //{
            //    if (Input.GetAxis("Roll") > 0.5f)
            //        LHold();
            //}

            //if (Input.GetAxis("Horizontal_r") <= 0 && Input.GetAxis("Roll") <= 0)
            //{
            //    skipDelayTime = 0; // 대화 넘기기 버튼을 누르는 시간 초기화
            //    UpdateSkipDelayUI();
            //}

            //if (InputDeviceChecker.instance.Interact())
            //    Interact();
            #endregion

            if (InputDeviceChecker.instance.Roll_RAW() != 0)
            {
                if (InputDeviceChecker.instance.Roll_RAW() > 0)
                    L();
                else
                    J();
            }

            if (rollNeutral == true && InputDeviceChecker.instance.Roll_RAW() != 0) // 조이스틱 GetButtonDown 수동 인식
            {
                if (InputDeviceChecker.instance.Roll_RAW() > 0.5f)
                    L();
                if (InputDeviceChecker.instance.Roll_RAW() < -0.5f)
                    J();
                rollNeutral = false;
            }
            else if (Mathf.Abs(InputDeviceChecker.instance.Roll_RAW()) <= 0.5f)
                rollNeutral = true;

            if (InputDeviceChecker.instance.Roll_RAW() != 0)
            {
                if (InputDeviceChecker.instance.Roll_RAW() > 0)
                    LHold();
            }

            if (InputDeviceChecker.instance.Roll_RAW() != 0)
            {
                if (InputDeviceChecker.instance.Roll_RAW() > 0.5f)
                    LHold();
            }

            if (InputDeviceChecker.instance.Roll_RAW() <= 0)
            {
                skipDelayTime = 0; // 대화 넘기기 버튼을 누르는 시간 초기화
                UpdateSkipDelayUI();
            }

            if (InputDeviceChecker.instance.Interact())
                Interact();
        }
    }

    /// <summary>
    /// 이전 대화 보기
    /// </summary>
    private void J()
    {
        if (cutsceneStatus >= 1) // 컷씬 연출 중일때는 상호작용 받지 않기
            return;

        if (effecting && CSVIndex >= 1) // 대사 출력 중일 때 대사 자동 스킵
        {
            Interact();
        }

        // <이전 대화 보기>
        // 대사 출력 중이 아닐 때 && 대사가 1줄 이상 진행 완료된 상태 && 이전 대사가 존재할 때 && 이전 대사에 선택지가 없을 때
        if (effecting == false && CSVIndex >= 2)
        {
            if (CSVData[CSVIndex - 2]["text"].ToString().Length >= 1 && CSVData[CSVIndex - 2]["select1"].ToString().Length == 0)
            {
                if (SelectButton1.activeInHierarchy == true) // 선택지가 활성화되어있을 때
                    HideSelectButton(); // 선택지 숨기기
                CSVIndex -= 2;
                Interact();
            }
        }
    }

    private void L()
    {

    }

    /// <summary>
    /// 대화 넘기기
    /// </summary>
    private void LHold()
    {
        if (skipDelayTime <= 1.0f)
        {
            skipDelayTime += Time.deltaTime;
            UpdateSkipDelayUI();
        }
        else
            Interact();
    }

    private void UpdateSkipDelayUI()
    {
        SkipDelayProgress.fillAmount = skipDelayTime / 1.0f;
    }

    private void Interact()
    {
        if (cutsceneStatus >= 1) // 컷씬 연출 중일때는 상호작용 받지 않기
            return;

        if (effecting == true && TalkPanelText.text.Length >= 1) // 대화 출력 도중
        {
            EffectSkip(); // 타이핑 이펙트 스킵
        }
        else if (effecting == false) // 대화 출력 중이 아닐 경우
        {
            if (CSVData[CSVIndex]["function"].ToString().Length >= 1 && usedFunction[CSVIndex] == false) // 현재 CSV파일의 행에 함수 이름이 입력되었을 때 && 이번 대화에서 해당 대사의 함수가 처음 쓰였을 때
            {
                usedFunction[CSVIndex] = true; // 이번 대사의 함수 사용 기록 남기기

                string functionName = CSVData[CSVIndex]["function"].ToString(); // 함수 이름을 가져옴

                if (CSVData[CSVIndex]["value"].ToString().Length >= 1) // 벨류값이 있을 때
                    TalkTriggers.instance.SendMessage(functionName, CSVData[CSVIndex]["value"].ToString()); // TalkTriggers.cs에서 해당 이름을 가진 함수 실행 + 벨류값
                else // 벨류값이 없을 때
                    TalkTriggers.instance.SendMessage(functionName); // TalkTriggers.cs에서 해당 이름을 가진 함수 실행
            }

            if (CSVData[CSVIndex]["cutscene"].ToString().Length >= 1) // 현재 행에 컷씬 연출이 있고, 아직 연출 실행 전일 때
                StartCoroutine(CutSceneEffect()); // 컷씬 연출 시작 (아래에 있는 else if 문장들은 컷씬 연출 끝나고 자동 실행)
            else if (CSVData[CSVIndex]["select1"].ToString().Length >= 1) // 현재 CSV파일의 행에 선택지가 있을 때
                ShowSelectButton();
            else if (CSVData[CSVIndex]["text"].ToString() == "END") // 선택지가 없고, 현재 CSV파일의 행의 text(대화 내용)가 "END"일 때, 대화 종료 (대화 창 닫기)
                StartCoroutine(TalkEnd());
            else // 선택지가 없고, 대화 종료 조건이 아니라면 대화 진행
                EffectStart();
        }
    }

    /// <summary>
    /// 대화 시작(대화 CSV 파일 경로)
    /// </summary>
    // path ex)
    //      1.  "Data/Talk/" + curQuestNpc.name + "/" + curQuestNpc.name + "_quest_" + curQuestNpc.GetComponent<NPCController>().questStep
    //      2.  "Data/Talk/" + curQuestNpc.name + "/" + curQuestNpc.name + "_quest_99

    public void TalkStart(string CSVFilePath) // 대화 시작 (CSVFilePath = 해당 대화 CSV 파일 경로)
    {
        talking = true; // 대화 시작 상태
        CSVData = CSVReader.Read(CSVFilePath); // CSVFIlePath에 적힌 경로대로 CSV 파일 불러오기
        for (int i = 0; i < usedFunction.Count; i++) // 함수 사용 기록 초기화
            usedFunction[i] = false;
        CSVIndex = 0; // CSV 첫 행부터 읽기

        foreach (Image img in Character1Group.GetComponentsInChildren<Image>())
        {
            img.color = new Color(1, 1, 1, 0);
        } // 캐릭터1 이미지 투명도 초기화
        foreach (Image img in Character2Group.GetComponentsInChildren<Image>())
        {
            img.color = new Color(1, 1, 1, 0);
        } // 캐릭터2 이미지 투명도 초기화

        Interact();
    }

    /// <summary>
    /// 대화 시작(대화 CSV 파일 경로, 대화 시작 index 위치)
    /// </summary>
    public void TalkStart(string CSVFilePath, int startIndex) // 대화 시작 (CSVFilePath = 해당 대화 CSV 파일 경로, startIndex = 대화 시작 위치 (CSV파일내의 인덱스))
    {
        talking = true; // 대화 시작 상태
        CSVData = CSVReader.Read(CSVFilePath); // CSVFIlePath에 적힌 경로대로 CSV 파일 불러오기
        for (int i = 0; i < usedFunction.Count; i++) // 함수 사용 기록 초기화
            usedFunction[i] = false;
        CSVIndex = startIndex; // CSV startIndex부터 읽기

        foreach (Image img in Character1Group.GetComponentsInChildren<Image>())
        {
            img.color = new Color(1, 1, 1, 0);
        } // 캐릭터1 이미지 투명도 초기화
        foreach (Image img in Character2Group.GetComponentsInChildren<Image>())
        {
            img.color = new Color(1, 1, 1, 0);
        } // 캐릭터2 이미지 투명도 초기화

        Interact();
    }

    private void EffectStart() // 대화 출력 시작 (타이핑 이펙트 시작)
    {
        effecting = true;

        if (TalkUIGroup.activeInHierarchy == false) // 대화창이 꺼져있다면
            TalkUIGroup.SetActive(true); // 대화창 열기

        TalkPanelArrow.SetBool("shake", false); // 대화 출력 완료를 알리는 화살표 애니메이션 비활성화

        TalkPanelName.text = CSVData[CSVIndex]["name"].ToString(); // CSV파일에 적힌 이름 불러와서 대화창에 적용 (현재 말하고 있는 사람의 이름)
        targetString = CSVData[CSVIndex]["text"].ToString(); // CSV파일에 적힌 현재 대화 문장 불러오기
        TalkPanelText.text = ""; // 대화창 초기화 (공란)
        charIndex = 0; // 현재 글자 index 초기화

        UpdateImage(); // 캐릭터 이미지 업데이트

        Invoke("Effecting", printSpeed); // (printSpeed초 후 다음 글자 출력)
    }

    private void Effecting() // 대화 출력 중 (타이핑 이펙트 중)
    {
        if (TalkPanelText.text == targetString) // 현재 대화창에 적힌 텍스트 == 현재 대화 문장 전체 ( 대화 출력 완료 시 )
        {
            EffectEnd(); // 대화 출력 완료 함수 실행
            return; // 대화 출력 함수 종료
        }

        TalkPanelText.text += targetString[charIndex]; // 대화창 업데이트 (보이는 글자 수 +1)

        // 띄어쓰기가 아닐 때, 효과음을 재생
        if (targetString[charIndex] != ' ')
            SoundEffect.instance.audioSource.PlayOneShot(talkSound);

        charIndex++; // 현재 글자 Index +1
        Invoke("Effecting", printSpeed); // (printSpeed초 후 다음 글자 출력)
    }

    private void EffectEnd() // 대화 출력 완료 (타이핑 이펙트 종료)
    {
        effecting = false;

        TalkPanelArrow.SetBool("shake", true); // 대화 출력 완료를 알리는 화살표 애니메이션 활성화

        CSVIndex++; // 현재 CSV Index +1

        if (forceSkip) // forceSkip이 true일 때
        {
            forceSkip = false;
            Interact();  // 대화 출력 완료하자마자 강제로 다음 대화로 넘기기
        }
    }

    private void EffectSkip()
    {
        TalkPanelText.text = targetString; // 대화창에 바로 문장 전체 입력
    }

    private void UpdateImage() // 캐릭터 이미지 업데이트
    {
        if (CSVData[CSVIndex]["char1Brightness"].ToString().Length >= 1) // 캐릭터1 이미지 밝기 조절
        {
            float brightness = float.Parse(CSVData[CSVIndex]["char1Brightness"].ToString()); // 캐릭터1 밝기 (0.0~1.0)
            foreach (Image img in Character1Group.GetComponentsInChildren<Image>())
            {
                img.color = new Color(brightness, brightness, brightness, img.color.a);
            }

            if (brightness == 1.0f)
                Charcter1Animator.SetTrigger("say"); // 대화 애니메이션 (캐릭터의 밝기를 1.0로 했을 때 작동)
        }

        if (CSVData[CSVIndex]["char2Brightness"].ToString().Length >= 1) // 캐릭터2 이미지 밝기 조절
        {
            float brightness = float.Parse(CSVData[CSVIndex]["char2Brightness"].ToString()); // 캐릭터2 밝기 (0.0~1.0)
            foreach (Image img in Character2Group.GetComponentsInChildren<Image>())
            {
                img.color = new Color(brightness, brightness, brightness, img.color.a);
            }

            if (brightness == 1.0f)
                Charcter2Animator.SetTrigger("say"); // 대화 애니메이션 (캐릭터의 밝기를 1.0로 했을 때 작동)
        }

        if (CSVData[CSVIndex]["char1PositionX"].ToString().Length >= 1) // 캐릭터1 위치 X 조절
        {
            float posX = float.Parse(CSVData[CSVIndex]["char1PositionX"].ToString());
            Character1Group.GetComponent<RectTransform>().anchoredPosition = new Vector2(posX, Character1Group.GetComponent<RectTransform>().anchoredPosition.y);
        }

        if (CSVData[CSVIndex]["char1PositionY"].ToString().Length >= 1) // 캐릭터1 위치 Y 조절
        {
            float posY = float.Parse(CSVData[CSVIndex]["char1PositionY"].ToString());
            Character1Group.GetComponent<RectTransform>().anchoredPosition = new Vector2(Character1Group.GetComponent<RectTransform>().anchoredPosition.x, posY);
        }

        if (CSVData[CSVIndex]["char2PositionX"].ToString().Length >= 1) // 캐릭터2 위치 X 조절
        {
            float posX = float.Parse(CSVData[CSVIndex]["char2PositionX"].ToString());
            Character2Group.GetComponent<RectTransform>().anchoredPosition = new Vector2(posX, Character2Group.GetComponent<RectTransform>().anchoredPosition.y);
        }

        if (CSVData[CSVIndex]["char2PositionY"].ToString().Length >= 1) // 캐릭터2 위치 Y 조절
        {
            float posY = float.Parse(CSVData[CSVIndex]["char2PositionY"].ToString());
            Character2Group.GetComponent<RectTransform>().anchoredPosition = new Vector2(Character2Group.GetComponent<RectTransform>().anchoredPosition.x, posY);
        }

        if (CSVData[CSVIndex]["char1ScaleX"].ToString().Length >= 1) // 캐릭터1 스케일 X 조절
        {
            float scaleX = float.Parse(CSVData[CSVIndex]["char1ScaleX"].ToString());
            Character1Group.GetComponent<RectTransform>().localScale = new Vector2(scaleX, Character1Group.GetComponent<RectTransform>().localScale.y);
        }

        if (CSVData[CSVIndex]["char1ScaleY"].ToString().Length >= 1) // 캐릭터1 스케일 Y 조절
        {
            float scaleY = float.Parse(CSVData[CSVIndex]["char1ScaleY"].ToString());
            Character1Group.GetComponent<RectTransform>().localScale = new Vector2(Character1Group.GetComponent<RectTransform>().localScale.x, scaleY);
        }

        if (CSVData[CSVIndex]["char2ScaleX"].ToString().Length >= 1) // 캐릭터2 스케일 X 조절
        {
            float scaleX = float.Parse(CSVData[CSVIndex]["char2ScaleX"].ToString());
            Character2Group.GetComponent<RectTransform>().localScale = new Vector2(scaleX, Character2Group.GetComponent<RectTransform>().localScale.y);
        }

        if (CSVData[CSVIndex]["char2ScaleY"].ToString().Length >= 1) // 캐릭터2 스케일 Y 조절
        {
            float scaleY = float.Parse(CSVData[CSVIndex]["char2ScaleY"].ToString());
            Character2Group.GetComponent<RectTransform>().localScale = new Vector2(Character2Group.GetComponent<RectTransform>().localScale.x, scaleY);
        }

        if (CSVData[CSVIndex]["char1Skin"].ToString().Length >= 1)
        {
            string CSVchar1Skin = CSVData[CSVIndex]["char1Skin"].ToString(); // 캐릭터1의 이미지 경로 불러오기
                                                                             // 경로가 0으로 적혀있으면 이미지를 숨김
                                                                             // 경로가 Op로 적혀져 있으면 현재 오퍼레이터 이미지를 불러옴
                                                                             // 경로가 Op_anger, Op_glad, Op_smile, Op_talk, Op_thorne로 적혀져 있으면 현재 오퍼레이터의 표정 이미지를 불러옴
                                                                             // 경로가 적혀져 있으면 해당 경로의 이미지로 변경 후 표시
                                                                             // 경로가 적혀져 있지 않으면 변화 없음
            switch (CSVchar1Skin)
            {
                case "0":
                    Character1Skin.color = new Color(Character1Skin.color.r, Character1Skin.color.g, Character1Skin.color.b, 0);
                    break;
                case "Op":
                case "op":
                    Character1Skin.sprite = Resources.Load<Sprite>(OP_SPRITE_PATH + PlayerPrefs.GetString("operatorDecoration") + "/operator");
                    Character1Skin.color = new Color(Character1Skin.color.r, Character1Skin.color.g, Character1Skin.color.b, 1);
                    break;
                case "Op_anger":
                case "op_anger":
                    Character1Skin.sprite = Resources.Load<Sprite>(OP_SPRITE_PATH + PlayerPrefs.GetString("operatorDecoration") + "/operator_anger");
                    Character1Skin.color = new Color(Character1Skin.color.r, Character1Skin.color.g, Character1Skin.color.b, 1);
                    break;
                case "Op_glad":
                case "op_glad":
                    Character1Skin.sprite = Resources.Load<Sprite>(OP_SPRITE_PATH + PlayerPrefs.GetString("operatorDecoration") + "/operator_glad");
                    Character1Skin.color = new Color(Character1Skin.color.r, Character1Skin.color.g, Character1Skin.color.b, 1);
                    break;
                case "Op_smile":
                case "op_smile":
                    Character1Skin.sprite = Resources.Load<Sprite>(OP_SPRITE_PATH + PlayerPrefs.GetString("operatorDecoration") + "/operator_smile");
                    Character1Skin.color = new Color(Character1Skin.color.r, Character1Skin.color.g, Character1Skin.color.b, 1);
                    break;
                case "Op_talk":
                case "op_talk":
                    Character1Skin.sprite = Resources.Load<Sprite>(OP_SPRITE_PATH + PlayerPrefs.GetString("operatorDecoration") + "/operator_talk");
                    Character1Skin.color = new Color(Character1Skin.color.r, Character1Skin.color.g, Character1Skin.color.b, 1);
                    break;
                case "Op_thorne":
                case "op_thorne":
                    Character1Skin.sprite = Resources.Load<Sprite>(OP_SPRITE_PATH + PlayerPrefs.GetString("operatorDecoration") + "/operator_thorne");
                    Character1Skin.color = new Color(Character1Skin.color.r, Character1Skin.color.g, Character1Skin.color.b, 1);
                    break;
                default:
                    Character1Skin.sprite = Resources.Load<Sprite>(CSVchar1Skin);
                    Character1Skin.color = new Color(Character1Skin.color.r, Character1Skin.color.g, Character1Skin.color.b, 1);
                    break;
            }
            Character1Skin.SetNativeSize();
        }

        if (CSVData[CSVIndex]["char1Face"].ToString().Length >= 1)
        {
            string CSVchar1Face = CSVData[CSVIndex]["char1Face"].ToString(); // 캐릭터1의 이미지 경로 불러오기
                                                                             // 경로가 0으로 적혀있으면 이미지를 숨김
                                                                             // 경로가 Op로 적혀져 있으면 현재 오퍼레이터 이미지를 불러옴
                                                                             // 경로가 Op_anger, Op_glad, Op_smile, Op_talk, Op_thorne로 적혀져 있으면 현재 오퍼레이터의 표정 이미지를 불러옴
                                                                             // 경로가 적혀져 있으면 해당 경로의 이미지로 변경 후 표시
                                                                             // 경로가 적혀져 있지 않으면 변화 없음
            switch (CSVchar1Face)
            {
                case "0":
                    Character1Face.color = new Color(Character1Face.color.r, Character1Face.color.g, Character1Face.color.b, 0);
                    break;
                case "Op":
                case "op":
                    Character1Face.sprite = Resources.Load<Sprite>(OP_SPRITE_PATH + PlayerPrefs.GetString("operatorDecoration") + "/operator");
                    Character1Face.color = new Color(Character1Face.color.r, Character1Face.color.g, Character1Face.color.b, 1);
                    break;
                case "Op_anger":
                case "op_anger":
                    Character1Face.sprite = Resources.Load<Sprite>(OP_SPRITE_PATH + PlayerPrefs.GetString("operatorDecoration") + "/operator_anger");
                    Character1Face.color = new Color(Character1Face.color.r, Character1Face.color.g, Character1Face.color.b, 1);
                    break;
                case "Op_glad":
                case "op_glad":
                    Character1Face.sprite = Resources.Load<Sprite>(OP_SPRITE_PATH + PlayerPrefs.GetString("operatorDecoration") + "/operator_glad");
                    Character1Face.color = new Color(Character1Face.color.r, Character1Face.color.g, Character1Face.color.b, 1);
                    break;
                case "Op_smile":
                case "op_smile":
                    Character1Face.sprite = Resources.Load<Sprite>(OP_SPRITE_PATH + PlayerPrefs.GetString("operatorDecoration") + "/operator_smile");
                    Character1Face.color = new Color(Character1Face.color.r, Character1Face.color.g, Character1Face.color.b, 1);
                    break;
                case "Op_talk":
                case "op_talk":
                    Character1Face.sprite = Resources.Load<Sprite>(OP_SPRITE_PATH + PlayerPrefs.GetString("operatorDecoration") + "/operator_talk");
                    Character1Face.color = new Color(Character1Face.color.r, Character1Face.color.g, Character1Face.color.b, 1);
                    break;
                case "Op_thorne":
                case "op_thorne":
                    Character1Face.sprite = Resources.Load<Sprite>(OP_SPRITE_PATH + PlayerPrefs.GetString("operatorDecoration") + "/operator_thorne");
                    Character1Face.color = new Color(Character1Face.color.r, Character1Face.color.g, Character1Face.color.b, 1);
                    break;
                default:
                    Character1Face.sprite = Resources.Load<Sprite>(CSVchar1Face);
                    Character1Face.color = new Color(Character1Face.color.r, Character1Face.color.g, Character1Face.color.b, 1);
                    break;
            }
            Character1Face.SetNativeSize();
        }

        if (CSVData[CSVIndex]["char2Skin"].ToString().Length >= 1)
        {
            string CSVchar2Skin = CSVData[CSVIndex]["char2Skin"].ToString(); // 캐릭터2의 이미지 경로 불러오기
                                                                             // 경로가 0으로 적혀있으면 이미지를 숨김
                                                                             // 경로가 Op로 적혀져 있으면 현재 오퍼레이터 이미지를 불러옴
                                                                             // 경로가 Op_anger, Op_glad, Op_smile, Op_talk, Op_thorne로 적혀져 있으면 현재 오퍼레이터의 표정 이미지를 불러옴
                                                                             // 경로가 적혀져 있으면 해당 경로의 이미지로 변경 후 표시
                                                                             // 경로가 적혀져 있지 않으면 변화 없음
            switch (CSVchar2Skin)
            {
                case "0":
                    Character2Skin.color = new Color(Character2Skin.color.r, Character2Skin.color.g, Character2Skin.color.b, 0);
                    break;
                case "Op":
                case "op":
                    Character2Skin.sprite = Resources.Load<Sprite>(OP_SPRITE_PATH + PlayerPrefs.GetString("operatorDecoration") + "/operator");
                    Character2Skin.color = new Color(Character2Skin.color.r, Character2Skin.color.g, Character2Skin.color.b, 1);
                    break;
                case "Op_anger":
                case "op_anger":
                    Character2Skin.sprite = Resources.Load<Sprite>(OP_SPRITE_PATH + PlayerPrefs.GetString("operatorDecoration") + "/operator_anger");
                    Character2Skin.color = new Color(Character2Skin.color.r, Character2Skin.color.g, Character2Skin.color.b, 1);
                    break;
                case "Op_glad":
                case "op_glad":
                    Character2Skin.sprite = Resources.Load<Sprite>(OP_SPRITE_PATH + PlayerPrefs.GetString("operatorDecoration") + "/operator_glad");
                    Character2Skin.color = new Color(Character2Skin.color.r, Character2Skin.color.g, Character2Skin.color.b, 1);
                    break;
                case "Op_smile":
                case "op_smile":
                    Character2Skin.sprite = Resources.Load<Sprite>(OP_SPRITE_PATH + PlayerPrefs.GetString("operatorDecoration") + "/operator_smile");
                    Character2Skin.color = new Color(Character2Skin.color.r, Character2Skin.color.g, Character2Skin.color.b, 1);
                    break;
                case "Op_talk":
                case "op_talk":
                    Character2Skin.sprite = Resources.Load<Sprite>(OP_SPRITE_PATH + PlayerPrefs.GetString("operatorDecoration") + "/operator_talk");
                    Character2Skin.color = new Color(Character2Skin.color.r, Character2Skin.color.g, Character2Skin.color.b, 1);
                    break;
                case "Op_thorne":
                case "op_thorne":
                    Character2Skin.sprite = Resources.Load<Sprite>(OP_SPRITE_PATH + PlayerPrefs.GetString("operatorDecoration") + "/operator_thorne");
                    Character2Skin.color = new Color(Character2Skin.color.r, Character2Skin.color.g, Character2Skin.color.b, 1);
                    break;
                default:
                    Character2Skin.sprite = Resources.Load<Sprite>(CSVchar2Skin);
                    Character2Skin.color = new Color(Character2Skin.color.r, Character2Skin.color.g, Character2Skin.color.b, 1);
                    break;
            }
            Character2Skin.SetNativeSize();
        }

        if (CSVData[CSVIndex]["char2Face"].ToString().Length >= 1)
        {
            string CSVchar2Face = CSVData[CSVIndex]["char2Face"].ToString(); // 캐릭터2의 이미지 경로 불러오기
                                                                             // 경로가 0으로 적혀있으면 이미지를 숨김
                                                                             // 경로가 Op로 적혀져 있으면 현재 오퍼레이터 이미지를 불러옴
                                                                             // 경로가 Op_anger, Op_glad, Op_smile, Op_talk, Op_thorne로 적혀져 있으면 현재 오퍼레이터의 표정 이미지를 불러옴
                                                                             // 경로가 적혀져 있으면 해당 경로의 이미지로 변경 후 표시
                                                                             // 경로가 적혀져 있지 않으면 변화 없음
            switch (CSVchar2Face)
            {
                case "0":
                    Character2Face.color = new Color(Character2Face.color.r, Character2Face.color.g, Character2Face.color.b, 0);
                    break;
                case "Op":
                case "op":
                    Character2Face.sprite = Resources.Load<Sprite>(OP_SPRITE_PATH + PlayerPrefs.GetString("operatorDecoration") + "/operator");
                    Character2Face.color = new Color(Character2Face.color.r, Character2Face.color.g, Character2Face.color.b, 1);
                    break;
                case "Op_anger":
                case "op_anger":
                    Character2Face.sprite = Resources.Load<Sprite>(OP_SPRITE_PATH + PlayerPrefs.GetString("operatorDecoration") + "/operator_anger");
                    Character2Face.color = new Color(Character2Face.color.r, Character2Face.color.g, Character2Face.color.b, 1);
                    break;
                case "Op_glad":
                case "op_glad":
                    Character2Face.sprite = Resources.Load<Sprite>(OP_SPRITE_PATH + PlayerPrefs.GetString("operatorDecoration") + "/operator_glad");
                    Character2Face.color = new Color(Character2Face.color.r, Character2Face.color.g, Character2Face.color.b, 1);
                    break;
                case "Op_smile":
                case "op_smile":
                    Character2Face.sprite = Resources.Load<Sprite>(OP_SPRITE_PATH + PlayerPrefs.GetString("operatorDecoration") + "/operator_smile");
                    Character2Face.color = new Color(Character2Face.color.r, Character2Face.color.g, Character2Face.color.b, 1);
                    break;
                case "Op_talk":
                case "op_talk":
                    Character2Face.sprite = Resources.Load<Sprite>(OP_SPRITE_PATH + PlayerPrefs.GetString("operatorDecoration") + "/operator_talk");
                    Character2Face.color = new Color(Character2Face.color.r, Character2Face.color.g, Character2Face.color.b, 1);
                    break;
                case "Op_thorne":
                case "op_thorne":
                    Character2Face.sprite = Resources.Load<Sprite>(OP_SPRITE_PATH + PlayerPrefs.GetString("operatorDecoration") + "/operator_thorne");
                    Character2Face.color = new Color(Character2Face.color.r, Character2Face.color.g, Character2Face.color.b, 1);
                    break;
                default:
                    Character2Face.sprite = Resources.Load<Sprite>(CSVchar2Face);
                    Character2Face.color = new Color(Character2Face.color.r, Character2Face.color.g, Character2Face.color.b, 1);
                    break;
            }
            Character2Face.SetNativeSize();
        }
    }

    #region SELECT
    private void ShowSelectButton() // 선택지 보여주기
    {
        if (SelectButton1.activeInHierarchy == true) // 이미 선택지가 활성화되어있다면 실행 X
            return;

        eventSystem.SetSelectedGameObject(null); // 기존 버튼 하이라이트 제거

        if (CSVData[CSVIndex]["select1"].ToString().Length >= 1) // 1번 선택지가 존재할 때
        {
            SelectButton1.GetComponentInChildren<Text>().text = CSVData[CSVIndex]["select1"].ToString(); // 선택지에 적힌 글자 CSV에 있는 대로 바꿔주기
            SelectButton1.SetActive(true); // 1번 선택지 활성화
        }

        if (CSVData[CSVIndex]["select2"].ToString().Length >= 1)
        {
            SelectButton2.GetComponentInChildren<Text>().text = CSVData[CSVIndex]["select2"].ToString();
            SelectButton2.SetActive(true);
        }

        if (CSVData[CSVIndex]["select3"].ToString().Length >= 1)
        {
            SelectButton3.GetComponentInChildren<Text>().text = CSVData[CSVIndex]["select3"].ToString();
            SelectButton3.SetActive(true);
        }

        if (CSVData[CSVIndex]["select4"].ToString().Length >= 1)
        {
            SelectButton4.GetComponentInChildren<Text>().text = CSVData[CSVIndex]["select4"].ToString();
            SelectButton4.SetActive(true);
        }

        eventSystem.SetSelectedGameObject(SelectButton1.GetComponentInChildren<Button>().gameObject); // 1번 선택지 하이라이트
    }

    public void HideSelectButton()
    {
        SelectButton1.SetActive(false); // 선택지 비활성화
        SelectButton2.SetActive(false);
        SelectButton3.SetActive(false);
        SelectButton4.SetActive(false);
    }

    public void Select1() // 1번 선택지 선택
    {
        SelectButton1.SetActive(false); // 선택지 비활성화
        SelectButton2.SetActive(false);
        SelectButton3.SetActive(false);
        SelectButton4.SetActive(false);

        string nextIndex = CSVData[CSVIndex]["indexAfterSelect1"].ToString(); // 다음 인덱스 값을 CSV파일에서 가져옴
        CSVIndex = int.Parse(nextIndex); // 불러온 다음 인덱스 값을 실제로 반영
    }

    public void Select2() // 2번 선택지 선택
    {
        SelectButton1.SetActive(false); // 선택지 비활성화
        SelectButton2.SetActive(false);
        SelectButton3.SetActive(false);
        SelectButton4.SetActive(false);

        string nextIndex = CSVData[CSVIndex]["indexAfterSelect2"].ToString(); // 다음 인덱스 값을 CSV파일에서 가져옴
        CSVIndex = int.Parse(nextIndex); // 불러온 다음 인덱스 값을 실제로 반영
    }

    public void Select3() // 3번 선택지 선택
    {
        SelectButton1.SetActive(false); // 선택지 비활성화
        SelectButton2.SetActive(false);
        SelectButton3.SetActive(false);
        SelectButton4.SetActive(false);

        string nextIndex = CSVData[CSVIndex]["indexAfterSelect3"].ToString(); // 다음 인덱스 값을 CSV파일에서 가져옴
        CSVIndex = int.Parse(nextIndex); // 불러온 다음 인덱스 값을 실제로 반영
    }

    public void Select4() // 4번 선택지 선택
    {
        SelectButton1.SetActive(false); // 선택지 비활성화
        SelectButton2.SetActive(false);
        SelectButton3.SetActive(false);
        SelectButton4.SetActive(false);

        string nextIndex = CSVData[CSVIndex]["indexAfterSelect4"].ToString(); // 다음 인덱스 값을 CSV파일에서 가져옴
        CSVIndex = int.Parse(nextIndex); // 불러온 다음 인덱스 값을 실제로 반영
    }
    #endregion

    IEnumerator TalkEnd() // 대화 끝
    {
        yield return null; // 아래 문장들은 다음 프레임에 실행
        talking = false; // 대화 종료 상태

        TalkUIGroup.SetActive(false); // 대화창 닫기
    }

    IEnumerator CutSceneEffect() // 컷씬 효과 과정
    {
        Image fadeImg = IngameCanvasManager.instance.GetFadeEffImg();
        fadeImg.gameObject.SetActive(true);

        fadeImg.color = new Color(fadeImg.color.r, fadeImg.color.g, fadeImg.color.b, 0);

        // 1. 화면을 완전히 가릴 때까지 화면 서서히 가리기
        cutsceneStatus = 1;
        while (fadeImg.color.a < 1)
        {
            fadeImg.color = new Color(fadeImg.color.r, fadeImg.color.g, fadeImg.color.b, fadeImg.color.a + Time.deltaTime);
            yield return null; // 다음 프레임에 실행
        }

        // 2. 화면이 완전히 보일 때까지 화면 서서히 드러내기
        cutsceneStatus = 2;
        // 대화 또는 선택지 출력 후 연출
        if (CSVData[CSVIndex]["select1"].ToString().Length >= 1) // 현재 CSV파일의 행에 선택지가 있을 때
            ShowSelectButton();
        else if (CSVData[CSVIndex]["text"].ToString() == "END") // 선택지가 없고, 현재 CSV파일의 행의 text(대화 내용)가 "END"일 때, 대화 종료 (대화 창 닫기)
            StartCoroutine(TalkEnd());
        else // 선택지가 없고, 대화 종료 조건이 아니라면 대화 진행
            EffectStart();
        while (fadeImg.color.a > 0)
        {
            fadeImg.color = new Color(fadeImg.color.r, fadeImg.color.g, fadeImg.color.b, fadeImg.color.a - Time.deltaTime);
            yield return null; // 다음 프레임에 실행
        }

        cutsceneStatus = 0;
        fadeImg.gameObject.SetActive(false);
    }
}