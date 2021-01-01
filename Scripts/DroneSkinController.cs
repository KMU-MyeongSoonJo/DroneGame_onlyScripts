using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/************************************
* 
* 설명:
*   PlayerPrefs의 정보를 받아 드론 외형을 변화시키는 스크립트
* 
************************************/

public class DroneSkinController : MonoBehaviour
{
    public static DroneSkinController instance;

    public GameObject whiteDrone;
    public GameObject redDrone;
    public GameObject racingDrone;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        UpdateDroneSkin();
    }

    public void UpdateDroneSkin() // 드론 외형 업데이트
    {
        switch (PlayerPrefs.GetString("droneDecoration"))
        {
            case "Drone.White":
                whiteDrone.SetActive(true);
                redDrone.SetActive(false);
                racingDrone.SetActive(false);
                GetComponent<DronePropelers>().elisaAngle = 90;
                break;
            case "Drone.Red":
                whiteDrone.SetActive(false);
                redDrone.SetActive(true);
                racingDrone.SetActive(false);
                GetComponent<DronePropelers>().elisaAngle = 90;
                break;
            case "Drone.Racing":
                whiteDrone.SetActive(false);
                redDrone.SetActive(false);
                racingDrone.SetActive(true);
                GetComponent<DronePropelers>().elisaAngle = 0;
                break;
        }
    }
}
