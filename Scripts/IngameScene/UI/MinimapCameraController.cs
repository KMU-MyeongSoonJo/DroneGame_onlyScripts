using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/**
 * 
 * 미니맵 카메라가 타겟을 정상적으로 따라다니도록 제어하는 스크립트
 * 
 * */


public class MinimapCameraController : MonoBehaviour
{
    public static MinimapCameraController instance;

    [Header("Components")]
    public Camera cam;
    public RenderTexture minimapTexture;
    public RenderTexture worldmapTexture;
    [Space]
    public GameObject fadeImg;
    public Transform droneFollowPos;
    public GameObject worldMapUI;
    public GameObject droneIcon; // 미니맵상에 출력되던 드론 아이콘

    [Header("Sensibilities")]
    public bool isWorldmap;
    
    [Header("Monitor")]
    // 카메라가 앵글의 중심으로 지정할 대상
    // 플레이어 or 드론 조종 상태에 따라 변경?
    public GameObject target;

    IEnumerator coroutine;

    private void Awake()
    {
        instance = this;
    }

    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if(coroutine == null)
            {
                coroutine = FadeOut();
                StartCoroutine(coroutine);
            }
        }
        
        if (!isWorldmap)
            MinimapCamMove();
        else
            WorldmapCamMove();
    }

    public void WorldmapOn()
    {
        isWorldmap = true;
        cam.targetTexture = worldmapTexture;
        target = GameObject.Find("CameraParent");

        IngameCanvasManager.instance.MenuTapClose();
        PlayerController.instance.SetViewPoint(1, true, true); // 플레이어만 조종 가능. 드론은 지도를 보여주는 중?

        //transform.eulerAngles = new Vector3(90, 0, 0);

        worldMapUI.SetActive(true);
    }

    public void WorldmapOff()
    {
        isWorldmap = false;
        cam.targetTexture = minimapTexture;
        target = GameObject.Find("GamerGirl");

        worldMapUI.SetActive(false);
    }

    // 어둡게
    IEnumerator FadeOut()
    {
        fadeImg.SetActive(true);
        Image img = fadeImg.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0);

        DroneFollowController dfc = PlayerController.instance.dm.GetComponent<DroneFollowController>();
        dfc.dronePos = transform.gameObject;

        while(img.color.a < 1)
        {
            float dt = Time.deltaTime;
            img.color += new Color(0, 0, 0, dt);

            yield return null;
        }
        
        // 지도 열기
        if (!isWorldmap)
        {
            droneIcon.SetActive(false);
            WorldmapOn();
        }
        
        // 지도 닫기
        else
        {
            droneIcon.SetActive(true);
            WorldmapOff();
        }
        
        coroutine = FadeIn();
        StartCoroutine(coroutine);
    }

    // 밝게
    IEnumerator FadeIn()
    {
        fadeImg.SetActive(true);
        Image img = fadeImg.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 1);

        DroneFollowController dfc = PlayerController.instance.dm.GetComponent<DroneFollowController>();
        dfc.dronePos = PlayerController.instance.transform.Find("HorizenDronePos").GetChild(0).gameObject;

        while (img.color.a > 0)
        {
            float dt = Time.deltaTime;
            img.color -= new Color(0, 0, 0, dt);

            yield return null;
        }

        fadeImg.SetActive(false);

        coroutine = null;
    }


    void MinimapCamMove()
    {
        cam.orthographicSize = 20;
        transform.position = target.transform.position + new Vector3(0, 150, 0);
        Vector3 tmp = new Vector3(90, target.transform.eulerAngles.y, 0);
        transform.eulerAngles = tmp;
        //transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, tmp, 0.3f);
    }

    void WorldmapCamMove()
    {
        cam.orthographicSize = 60;
        transform.position = target.transform.position + new Vector3(0, 200, 0);


        Vector3 targetVector = target.transform.eulerAngles;
        float turnAngle = target.transform.eulerAngles.y - transform.eulerAngles.y;

        if (turnAngle > 180) { targetVector.y -= 360; }
        else if (turnAngle < -180) { targetVector.y += 360; }

        transform.eulerAngles = Vector3.Lerp(transform.eulerAngles,
            new Vector3(90, targetVector.y, 0),
            Time.deltaTime * 2);
    }

    // 타겟 전환 함수
    public void SetTarget(GameObject _target)
    {
        if (!isWorldmap)
            target = _target;
    }
}
