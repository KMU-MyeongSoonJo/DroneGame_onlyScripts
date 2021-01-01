using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/************************************
* 
* 설명:
*   시작할 때 PlayerPrefs의 정보를 불러오는 스크립트
* 
************************************/

public class PlayerPrefsData : MonoBehaviour
{
    public static PlayerPrefsData instance;

    public int repute; // 평판
    public int licenseGrade; // 라이센스 등급

    /// <summary>
    /// 튜토리얼 종료 여부.
    /// 0: false,  1: true
    /// </summary>
    public int isTutorialFinish;

    private void Awake()
    {
        if (instance != null) // 중복 생성 방지
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this);

        GetPlayerPrefsData(); // PlayerPrefs 데이터 불러오기
    }

    private void Update()
    {
        // REMOVE ALL DATA
        if (Input.GetKeyDown(KeyCode.X))
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("저장 데이터 삭제!");
            GetPlayerPrefsData();
        }

        // CHEAT
        if (Input.GetKeyDown(KeyCode.C))
        {
            repute += 10;
            PlayerPrefs.SetInt("repute", repute);
        }
    }

    private void GetPlayerPrefsData() // PlayerPrefs 데이터 불러오기
    {
        if (PlayerPrefs.HasKey("repute")) // 평판 데이터 불러오기
            repute = PlayerPrefs.GetInt("repute");
        else // 데이터가 없을 경우
            repute = 0; // 기본값

        if (PlayerPrefs.HasKey("licenseGrade")) // 라이센스 등급 데이터 불러오기
            licenseGrade = PlayerPrefs.GetInt("licenseGrade");
        else // 데이터가 없을 경우
            licenseGrade = 0; // 기본값

        if (PlayerPrefs.HasKey("droneDecoration")) // 드론 외형 데이터 불러오기
            ;
        else // 데이터가 없을 경우
            PlayerPrefs.SetString("droneDecoration", "Drone.White"); // 기본값

        if (PlayerPrefs.HasKey("operatorDecoration")) // 오퍼레이터 외형 데이터 불러오기
            ;
        else // 데이터가 없을 경우
            PlayerPrefs.SetString("operatorDecoration", "operator_1"); // 기본값

        if (PlayerPrefs.HasKey("isTutorialFinish"))
            isTutorialFinish = PlayerPrefs.GetInt("isTutorialFinish");
        else
        // 데이터가 없을 경우
            isTutorialFinish = 0; // false. 튜토리얼이 종료되지 않았음.
            
        
        
    }

    
}
