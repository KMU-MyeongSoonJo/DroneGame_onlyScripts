using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

/************************************
* 
* 버전: 
* 작성자: 진민준     최초 작성날짜:             최근 수정날짜:       
* 설명:
*   PlayerPrefs로부터 데이터를 save/load 하는 기능을 총괄하는 스크립트
* 
************************************/

public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    private void Awake()
    {
        instance = this;

        LoadPlayerCurPos();
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey("deviceName"))
        {
            string curDeviceName = PlayerPrefs.GetString("deviceName");
            print("data manager cur device name : " + curDeviceName);
            print($"inputDeviceChecker is {InputDeviceChecker.instance} now ..");
            InputDeviceChecker.instance.SetInputDevice(curDeviceName);
        }
    }

    private void OnApplicationQuit()
    {
        // 건물로 진입한 다음(포탈 위치 저장) 프로그램을 종료한 경우
        // 다음 접속시 해당 위치정보에서 시작하는 경우를 방지하기 위함
        if (PlayerPrefs.HasKey("playerCurPos"))
            PlayerPrefs.DeleteKey("playerCurPos");
    }


    public void SavePlayerCurPos(Vector3 playerCurPos) {
        print(playerCurPos);
        PlayerPrefs.SetString("playerCurPos", playerCurPos.ToString());
    }

    public  void LoadPlayerCurPos()
    {
        if (PlayerPrefs.HasKey("playerCurPos"))
        {
            print("== PLAYER CUR POS LOADED == ");
            
            string target;
            Vector3 output;

            target = PlayerPrefs.GetString("playerCurPos");

            target = target.Substring(1, target.Length - 2);
            string[] axis = target.Split(',');

            output = new Vector3(float.Parse(axis[0]), float.Parse(axis[1]), float.Parse(axis[2]));

            print(output);
            PlayerController.instance.gameObject.transform.position = output;

            // Drone Teleport
            if (PlayerPrefsData.instance.isTutorialFinish == 1)
                PlayerController.instance.drone.transform.position = PlayerController.instance.dronePos.transform.position;


            PlayerPrefs.DeleteKey("playerCurPos");
        }
    }
}

















