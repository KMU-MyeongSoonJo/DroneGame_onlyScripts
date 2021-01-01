using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * 
 * 스폰 지점을 선택하기 위한 스크립트
 * 
 * */



public class SpawnPosSelector : MonoBehaviour
{
    static public SpawnPosSelector instance;
    
    [SerializeField] Camera titleCamera;
    [SerializeField] Text spawnPosTitle;
    
    [Header("효과음")]
    [SerializeField] AudioClip windEffect; // 바람소리 효과

    public HighlightSound highlightSound;

    // 조이스틱의 경우 Input.GetAxisRaw()가 키보드와 같이 동작되지 않음
    // 그로 인해 매 프레임 입력 호출이 발생, 이를 해결하기 위한 대안
    // 본 변수에 값을 담고, 0 등으로 초기화 되거나 일정 시간이 지나기 전까지 추가 입력을 받지 않는다.
    bool isAlreadyJoyInput;

    IEnumerator camMoveCoroutine;

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        //SpawnPosContainer.instance.gameObject.SetActive(true);
        SpawnPosContainer.instance.enabled = true;
    }

    private void Update()
    {
        if (InputDeviceChecker.instance != null)
            Select();

        titleCamera.transform.Rotate(Vector3.up, 10 * Time.deltaTime);
    }



    /// <summary>
    /// 스폰 지점 선택
    /// </summary>
    public void Select()
    {
        // 화면 전환 효과가 재생중이라면 오작동 방지를 위해 입력 제한
        if(camMoveCoroutine != null)
        {
            return;
        }

        // 선택 완료 및 게임 시작
        if (InputDeviceChecker.instance.Interact())
        {
            SpawnPosContainer.instance.isSpawnFromTitle = true;

            highlightSound.PlayClickSound();

            LoadingManager.LoadScene("PlayableScene", "TitleScene");

            //SceneManager.UnloadSceneAsync("TitleScene");
        }

        if (isAlreadyJoyInput)
        {
            // 반복 입력 방지

            if (InputDeviceChecker.instance.Yaw_Raw() == 0 && InputDeviceChecker.instance.Roll_RAW() == 0)
            {
                isAlreadyJoyInput = false;
            }

            return;
        }

        //if (Input.GetAxisRaw("Horizontal") < 0)
        if (InputDeviceChecker.instance.Yaw_Raw() > 0 || InputDeviceChecker.instance.Roll_RAW() > 0)
        {
            isAlreadyJoyInput = true;

            NextSpawnPos();
            Input.ResetInputAxes();
        }

        //else if (Input.GetAxisRaw("Horizontal") > 0)
        else if (InputDeviceChecker.instance.Yaw_Raw() < 0 || InputDeviceChecker.instance.Roll_RAW() < 0)
        {
            isAlreadyJoyInput = true;

            PreviousSpawnPos();
            Input.ResetInputAxes();
        }
    }

    /// <summary>
    /// 스폰할 위치를 미리 보여주기 위한 함수
    /// </summary>
    public void MoveCameraToSpawnPos()
    {
        // skyView라면, 해당 정보를 함께 전달
        if (titleCamera.transform.position.y > 190)
        {
            camMoveCoroutine = CamMoveToNewSpawnPos(SpawnPosContainer.instance.spawnPos[SpawnPosContainer.instance.selectedSpawnPosIdx].transform, true);
            StartCoroutine(camMoveCoroutine);
        }

        // 해당 위치로 카메라 이동
        else
        {
            camMoveCoroutine = CamMoveToNewSpawnPos(SpawnPosContainer.instance.spawnPos[SpawnPosContainer.instance.selectedSpawnPosIdx].transform);
            StartCoroutine(camMoveCoroutine);
        }
    }

    /// <summary>
    /// 다음 스폰 지점 보기
    /// </summary>
    public void NextSpawnPos()
    {
        if (SpawnPosContainer.instance.selectedSpawnPosIdx == SpawnPosContainer.instance.spawnPos.Length - 1)
            SpawnPosContainer.instance.selectedSpawnPosIdx = 0;
        else
            SpawnPosContainer.instance.selectedSpawnPosIdx++;

        highlightSound.PlayHighlightSound();
        MoveCameraToSpawnPos();
        DescTitle();
    }

    /// <summary>
    /// 이전 스폰 지점 보기
    /// </summary>
    public void PreviousSpawnPos()
    {
        if (SpawnPosContainer.instance.selectedSpawnPosIdx == 0)
            SpawnPosContainer.instance.selectedSpawnPosIdx = SpawnPosContainer.instance.spawnPos.Length - 1;
        else
            SpawnPosContainer.instance.selectedSpawnPosIdx--;

        highlightSound.PlayHighlightSound();
        MoveCameraToSpawnPos();
        DescTitle();
    }

    IEnumerator CamMoveToNewSpawnPos(Transform targetTransform, bool isFromSkyView = false)
    {
        float timer = 0f;

        Vector3 startPos = titleCamera.transform.position;
        Vector3 startRot = titleCamera.transform.eulerAngles;

        float minHeight = targetTransform.position.y + 2, maxHeight = targetTransform.position.y + 150;
        float maxRotRange = 60; // 카메라 이동시 지평선이 보이지 않게 아래로 기울이는 최대 각도

        SoundEffect.instance.audioSource.PlayOneShot(windEffect);

        while (timer < 1f)
        {
            timer += Time.deltaTime;

            Vector3 dPos = Vector3.Lerp(startPos, targetTransform.position + new Vector3(0, 2, 0), timer);
            
            Vector3 dRot = titleCamera.transform.eulerAngles;
            
            // 최초 SkyView에서 아래로 내려올 땐 y 좌표를 변환시키지 않는다.
            // 그 이외의 경우라면 sin graph(0 ~ pi) 를 그리며 y 좌표가 변화한다.
            if (!isFromSkyView)
            {
                float dy = Mathf.Sin(Mathf.Clamp(timer, 0, 1) * Mathf.PI) * maxHeight + minHeight;
                dPos.y = dy;

                dRot.x = Mathf.Sin(Mathf.Clamp(timer, 0, 1) * Mathf.PI) * maxRotRange;
            }
            else
            {
                dRot = Vector3.Lerp(startRot, targetTransform.eulerAngles, timer);
            }
            
            titleCamera.transform.position = dPos;
            dRot.y = Mathf.Lerp(startRot.y, targetTransform.eulerAngles.y, timer);
            dRot.z = 0;

            //titleCamera.transform.eulerAngles = Vector3.Lerp(startRot, targetTransform.eulerAngles, timer);
            titleCamera.transform.eulerAngles = dRot;

            yield return null;
        }

        titleCamera.transform.position = targetTransform.position + new Vector3(0, 2, 0);
        titleCamera.transform.eulerAngles = targetTransform.eulerAngles;

        camMoveCoroutine = null;

    }

    /// <summary>
    /// 해당 스폰 지점의 설명을 화면 상단 UI에 표시
    /// </summary>
    public void DescTitle()
    {
        spawnPosTitle.text = SpawnPosContainer.instance.spawnPos[SpawnPosContainer.instance.selectedSpawnPosIdx].desc;
    }
}
