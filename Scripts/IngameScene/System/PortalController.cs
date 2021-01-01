using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/************************************
* 
* 버전: 
* 작성자: 진민준     최초 작성날짜:             최근 수정날짜:       
* 설명:
*   포탈(single, double)을 구현하는 스크립트
* 
************************************/

public enum PortalType { singleType , doubleType }

public class PortalController : MonoBehaviour
{
    public PortalType portalType;

    // 건물 외부에 설치된 포탈일 경우 체크한다.
    // 건물 진입 직전에 위치를 저장하고, 건물에서 나오면서 해당 위치로 이동한다.
    public bool isOutside;
    public string sceneName;

    public GameObject output;
   
    bool isPlayerEnter;

    private void Update()
    {
        if (InputDeviceChecker.instance.Interact() && isPlayerEnter)
        {
            isPlayerEnter = false;
            portal(PlayerController.instance.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerEnter = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerEnter = false;
    }

    public void portal(GameObject other)
    {
        if (portalType == PortalType.singleType)
        {
            // record previous scene
            Recorder.instance.previousScene = SceneManager.GetActiveScene().name;

            // 이동할 씬이 등록되어있다면
            if (sceneName != "null")
            {
                // 건물 외부/내부를 구분
                // 건물 외부에서 진입하려는 경우, 되돌아올 위치좌표를 저장
                if (isOutside)
                    DataManager.instance.SavePlayerCurPos(transform.parent.position);


                print($"portal, {SceneManager.GetActiveScene().name} to {sceneName}");
                LoadingManager.LoadScene(sceneName, SceneManager.GetActiveScene().name);

                //// 인게임으로 돌아가는 경우.
                //// City Scene + Playable Scene 호출 및 본인 씬 삭제
                //if (sceneName == "IngameScene")
                //{
                //    SceneManager.LoadSceneAsync("CityScene");
                //    SceneManager.LoadSceneAsync("PlayableScene", LoadSceneMode.Additive);
                //}
                //else
                //{
                //    SceneManager.LoadScene(sceneName);
                //}
            }
        }
        else
        {
            other.transform.position = output.transform.position;// + output.transform.forward * 2;

            // Drone Teleport
            if (PlayerPrefsData.instance.isTutorialFinish == 1)
                PlayerController.instance.drone.transform.position = PlayerController.instance.dronePos.transform.position;
        }
    }
}
