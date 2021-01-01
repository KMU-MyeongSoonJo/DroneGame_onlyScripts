using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetMarker : MonoBehaviour
{
    public static TargetMarker instance;

    [Header("목표 표시기가 들어간 캔버스")]
    public RectTransform CanvasRect;

    [Header("목표 표시기(UI)")]
    public GameObject targetMarkerUI;
    public RectTransform marker;
    public Text topText;
    public Text distanceText;

    [Header("목표 대상")]
    public Transform target;
    public string targetString;
    

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }

        instance = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        if(PlayerPrefsData.instance.isTutorialFinish == 0)
        {
            SetNewTarget(GameObject.Find("Examiner").transform, "튜토리얼");
        }
    }

    private void Update()
    {
        if (target == null)
            return;

        // 목표 표시기의 위치 구하기
        // 1. 각도 확인
        float angle = Vector3.Angle(Camera.main.transform.forward, target.transform.position - Camera.main.transform.position);
        // 2. 목표의 뷰포트 위치 계산 
        Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(target.transform.position);
        // 3. 캔버스 크기 맞게 목표 표시기 위치 조정 + 특수 조건: 카메라와 목표와의 각도가 카메라의 fieldOfView * 1.5f를 넘어선다면 목표 표시기의 y위치를 항상 아래로 두기
        Vector2 WorldObject_ScreenPosition = new Vector2();
        if (angle < Camera.main.fieldOfView * 1.5f)
        {
            WorldObject_ScreenPosition =
            new Vector2(Mathf.Clamp((ViewportPosition.x * CanvasRect.sizeDelta.x) - (0.5f * CanvasRect.sizeDelta.x), CanvasRect.sizeDelta.x * -0.5f + 50, CanvasRect.sizeDelta.x * 0.5f - 50),
                        Mathf.Clamp((ViewportPosition.y * CanvasRect.sizeDelta.y) - (0.5f * CanvasRect.sizeDelta.y), CanvasRect.sizeDelta.y * -0.5f + 80, CanvasRect.sizeDelta.y * 0.5f - 80));
        }
        else
        {
            WorldObject_ScreenPosition =
            new Vector2(Mathf.Clamp(-((ViewportPosition.x * CanvasRect.sizeDelta.x) - (0.5f * CanvasRect.sizeDelta.x)), CanvasRect.sizeDelta.x * -0.5f + 50, CanvasRect.sizeDelta.x * 0.5f - 50),
                        CanvasRect.sizeDelta.y * -0.5f + 80);
        }
        marker.anchoredPosition = WorldObject_ScreenPosition;

        // 목표 표시기의 텍스트 업데이트
        if (topText.text != targetString)
            topText.text = targetString;
        distanceText.text = Vector3.Distance(Camera.main.transform.position, target.position).ToString("0") + "M";
    }

    /// <summary>
    /// 새 목표 설정하기 (targetTransform = 목표 위치, targetDescription = 목표 아이콘 위 설명)
    /// </summary>
    /// <param name="targetTransform"></param>
    /// <param name="targetDescription"></param>
    public void SetNewTarget(Transform targetTransform, string targetDescription)
    {
        targetMarkerUI.SetActive(true);
        target = targetTransform;
        targetString = targetDescription;
    }
    
    public void SetNoTarget()
    {
        targetMarkerUI.SetActive(false);
        target = null;
    }
}
