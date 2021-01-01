using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/************************************
* 
* 버전: 
* 작성자: 진민준     최초 작성날짜:             최근 수정날짜:       
* 설명:
*   게임 시작시 인트로 카메라 무브 등을 구현하는 스크립트
* 
************************************/

public class IntroManager : MonoBehaviour
{
    public GameObject introCam, ingameCam;

    // intro Scene Objs
    public GameObject[] unloadObjs;

    // ingame Scene Objs
    public GameObject[] loadObjs;

    // 시작 인트로가 씬을 이동하는 과정에서 연속해서 발생하지 않게 하는 플래그
    public static bool awakeOnceFlag = false;

    private void Awake()
    {
        if (awakeOnceFlag == false)
        {
            awakeOnceFlag = true;

            introCam.SetActive(true);
            ingameCam.SetActive(false);

            setObjs(false);

            StartCoroutine(introCameraMove());
        }
    }

    public void setObjs(bool isOn)
    {
        foreach(var a in loadObjs)
        {
            a.SetActive(isOn);
        }
        foreach (var a in unloadObjs)
        {
            a.SetActive(!isOn);
        }
    }

    IEnumerator introCameraMove()
    {
        while (true)
        {
            if(InputDeviceChecker.instance.Interact())
            {
                introCam.SetActive(false);
                ingameCam.SetActive(true);

                break;
            }

            introCam.transform.eulerAngles += new Vector3(0, 10 * Time.deltaTime, 0);

            yield return null;
        }

        print("Stop Intro");
        setObjs(true);
    }
}

