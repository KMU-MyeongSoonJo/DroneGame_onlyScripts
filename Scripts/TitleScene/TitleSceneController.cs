using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TitleSceneController : MonoBehaviour
{
    public Camera m_camera;
    public Transform skyViewPos;
    public Transform spawnPosSelector;
    public Image fadeImg;

    [Header("Canvas")]
    public Transform pressAnyBtnCanvas;
    public Transform spawnPosSelectorCanvas;
    public Animator pressAnyBtnCanvasAnim;

    [Range(0, 20)] public float skyViewRotSpeed = 5f;

    private void Awake()
    {
        spawnPosSelector.gameObject.SetActive(false);   
    }

    private void Start()
    {
        pressAnyBtnCanvas.gameObject.SetActive(true);
        spawnPosSelectorCanvas.gameObject.SetActive(false);

        StartCoroutine(StartFadeIn());
        StartCoroutine(SkyViewDirection());
    }

    public void StartToSelectSpawnPos()
    {
        pressAnyBtnCanvas.gameObject.SetActive(false);
        spawnPosSelectorCanvas.gameObject.SetActive(true);

        if (!spawnPosSelector.gameObject.activeSelf)
            spawnPosSelector.gameObject.SetActive(true);
    }

    IEnumerator StartFadeIn()
    {
        while(SceneManager.GetActiveScene() == SceneManager.GetSceneByName("LoadingScene"))
            yield return null;

        float timer = 0f;
        Vector3 camStartPos = skyViewPos.position + new Vector3(0, 30, 0);
        Vector3 camDestPos = skyViewPos.position;

        // fade in process
        while (timer < 2f)
        {
            timer += Time.deltaTime;

            Color dColor = fadeImg.color;

            dColor.a = Mathf.Cos(timer * 0.5f * Mathf.PI * 0.5f);

            fadeImg.color = dColor;

            m_camera.transform.position = Vector3.Lerp(camStartPos, camDestPos, timer * 0.5f);

            yield return null;
        }

        fadeImg.color = new Color(0, 0, 0, 0);

        pressAnyBtnCanvasAnim.SetTrigger("start");
    }

    /// <summary>
    /// 스카이 뷰 연출.
    /// 아무 키 입력시 스폰 지점 선택 화면으로 진행된다.
    /// </summary>
    IEnumerator SkyViewDirection()
    {
        // set start pos and rot
        //m_camera.transform.position 
        //    = skyViewPos.position;
        m_camera.transform.eulerAngles 
            = skyViewPos.eulerAngles;

        while (!Input.anyKeyDown)
        {
            if (m_camera.transform.position.y > skyViewPos.position.y)
            {
                yield return null;
                continue;
            }

            m_camera.transform.Rotate(Vector3.up * Time.deltaTime * skyViewRotSpeed, Space.World);

            yield return null;
        }
        Input.ResetInputAxes();

        pressAnyBtnCanvasAnim.SetBool("outro",true);
    }

    public void MouseLock()
    {
        // Mouse Lock
        Cursor.lockState = CursorLockMode.Locked;

        // Cursor visible
        Cursor.visible = false;
    }
    
}
