using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/************************************
* 
* 작성자: 송창하
* 설명:
*   방 안에서의 카메라의 제어를 담당하는 스크립트
* 
************************************/

public class InRoomCamera : MonoBehaviour
{
    public static InRoomCamera instance;

    public enum CameraMode { CharacterView, PlayerDroneView, TPDroneView, FPDroneView, RoomView, Free }
    //public CameraMode cameraMode;
    [SerializeField] private CameraMode cameraMode;
    public Transform target; // 카메라가 바라보는 오브젝트 위치
    public Vector3 TPDroneViewOffset; // 카메라가 target으로부터 떨어진 위치 (TP Drone View 전용)
    public Vector3 roomViewOffset; // 카메라가 target으로부터 떨어진 위치 (Room View 전용)
    public float maxDistance; // 목표와 카메라간의 최대 거리 (Character View 전용)
    public float height; // 카메라의 높이 (Character View 전용)
    public float smoothSpeed; // 부드러운 카메라 이동을 위한 변수
    [Header("[Targets]")]
    public Transform character;
    public Transform drone;
    public Transform droneFPCameraPos; // 드론 1인칭 카메라 위치
    public Transform room;
    [Space]
    [Tooltip("Drone/FixRotation/AnimatedGo/####Drone")]public Transform[] droneModelsRoot; // 드론 모델

    private RaycastHit hit; // 카메라 Raycast (벽 감지용)

    private void Awake()
    {
        instance = this;
    }

    private void FixedUpdate()
    {
        switch (cameraMode)
        {
            case CameraMode.CharacterView:
                CharacterViewCamera(); // 플레이어 관찰 카메라
                break;
            case CameraMode.PlayerDroneView:
                PlayerDroneViewCamera(); // 드론 관찰 카메라
                break;
            case CameraMode.TPDroneView:
                TPDroneViewCamera(); // 드론 3인칭 카메라
                break;
            case CameraMode.FPDroneView:
                FPDroneViewCamera(); // 드론 1인칭 카메라
                break;
            case CameraMode.RoomView:
                transform.parent = null; // 1인칭의 경우, 드론의 자식으로 카메라 설정. 그 외는 부모관계 X
                RoomViewCamera(); // 방 관찰 카메라
                break;
        }
    }
    public void ChangeCameraMode(CameraMode myMode)
    {
        cameraMode = myMode;

        switch (myMode)
        {
            case CameraMode.CharacterView:
                DroneVisualizeController(true);

                break;
            case CameraMode.PlayerDroneView:
                DroneVisualizeController(true);

                break;
            case CameraMode.TPDroneView:
                DroneVisualizeController(true);

                break;
            case CameraMode.FPDroneView:
                DroneVisualizeController(false);

                break;
            case CameraMode.RoomView:
                DroneVisualizeController(true);

                break;
            case CameraMode.Free:
                DroneVisualizeController(true);

                break;
        }
    }

    public void DroneVisualizeController(bool isOn)
    {

        int idx = -1;
        switch (PlayerPrefs.GetString("droneDecoration"))
        {
            case "Drone.White":
                idx = 0;
                break;
            case "Drone.Red":
                idx = 1;
                break;
            case "Drone.Racing":
                idx = 2;
                break;
        }


        switch (idx)
        {
            case 0: // WhiteDrone
            case 1: // RedDrone

                MeshRenderer mr = droneModelsRoot[idx].GetChild(0).GetComponent<MeshRenderer>();
                mr.enabled = isOn;

                break;

            case 2: // RacingDrone

                Transform tmpRoot = droneModelsRoot[idx].GetChild(0);
                for(int i=0;i<tmpRoot.childCount; i++)
                {
                    tmpRoot.GetChild(i).gameObject.SetActive(isOn);
                }

                break;
        }
    }


    private void CharacterViewCamera() // 플레이어 관찰 카메라
    {
        transform.parent = null; // 1인칭의 경우, 드론의 자식으로 카메라 설정. 그 외는 부모관계 X

        target = character; // 플레이어 기준으로 카메라 설정

        Quaternion rot = Quaternion.Euler(target.eulerAngles.x, target.eulerAngles.y, 0); // 플레이어의 y축 각도 구하기
        Vector3 desiredPosition = target.position - (rot * Vector3.forward * maxDistance) + (Vector3.up * height); // 카메라가 이동할 최종 위치 구하기
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed); // 카메라가 현재 프레임에서 이동중인 위치 구하기
        this.transform.position = smoothedPosition;

        Vector3 rayWay = transform.forward * -1;

        if (Physics.Raycast(target.position, rayWay, out hit, maxDistance)
            #region RAY EXCEPTION LAYER
              && hit.collider.gameObject.layer != LayerMask.NameToLayer("DronePos")
              && hit.collider.gameObject.layer != LayerMask.NameToLayer("MinimapLookObj")
            #endregion
            ) // 레이케스트 검사
        {

            Camera.main.transform.position = hit.point; // 무언가에 걸렸다면 Ray가 충돌한 위치로 카메라 이동

            Camera.main.transform.Translate(Vector3.forward * 1); // 카메라의 위치 플레이어 쪽으로 약간 이동 (벽 너머 보이는 현상 최소화 1)

            if (Vector3.Distance(Camera.main.transform.position, target.position) < 0.5f)
            {
                print("Too close");
                Camera.main.transform.position = target.position; // 카메라가 플레이어에 가까이 있으면 1인칭으로 전환 (벽 너머 보이는 현상 최소화 2)
            }
        }

        else // 하나도 닿지 않았다면. 즉 사이에 방해되는 오브젝트가 없다면
        {
            Camera.main.transform.position = this.transform.position; // 카메라를 자기 부모 오브젝트의 위치로 이동
        }

        transform.LookAt(target); // 카메라가 목표를 바라보기
    }

    private void PlayerDroneViewCamera() // 드론 관찰 카메라
    {
        transform.parent = null; // 1인칭의 경우, 드론의 자식으로 카메라 설정. 그 외는 부모관계 X

        target = drone; // 드론 기준으로 카메라 설정

        transform.position = character.position;

        transform.LookAt(target); // 카메라가 목표를 바라보기
    }

    private void TPDroneViewCamera() // 3인칭 드론 카메라
    {
        transform.parent = null; // 1인칭의 경우, 드론의 자식으로 카메라 설정. 그 외는 부모관계 X

        target = drone; // 드론 기준으로 카메라 설정

        Quaternion rot = Quaternion.Euler(0, target.eulerAngles.y, 0); // 드론의 y축 각도 구하기
        Vector3 desiredPosition = target.position - (rot * Vector3.forward * TPDroneViewOffset.z) + (Vector3.up * TPDroneViewOffset.y); // 카메라가 이동할 최종 위치 구하기
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed); // 카메라가 현재 프레임에서 이동중인 위치 구하기
        this.transform.position = smoothedPosition;

        Vector3 rayWay = transform.forward * -1;

        if (Physics.Raycast(target.position, rayWay, out hit, maxDistance)
         #region RAY EXCEPTION LAYER
             && hit.collider.gameObject.layer != LayerMask.NameToLayer("DronePos")
        #endregion
             ) // 레이케스트 검사
        {


            Camera.main.transform.position = hit.point; // 무언가에 걸렸다면 Ray가 충돌한 위치로 카메라 이동
            
            Camera.main.transform.Translate(Vector3.forward * 1); // 카메라의 위치 플레이어 쪽으로 약간 이동 (벽 너머 보이는 현상 최소화 1)

            if (Vector3.Distance(Camera.main.transform.position, target.position) < 0.5f)
            {
                print("Too close");
                Camera.main.transform.position = target.position; // 카메라가 플레이어에 가까이 있으면 1인칭으로 전환 (벽 너머 보이는 현상 최소화 2)
            }
        }
        else // 하나도 닿지 않았다면. 즉 사이에 방해되는 오브젝트가 없다면
        {
            Camera.main.transform.position = this.transform.position; // 카메라를 자기 부모 오브젝트의 위치로 이동
        }

        transform.LookAt(target); // 카메라가 목표를 바라보기
    }

    private void FPDroneViewCamera() // 1인칭 드론 카메라
    {
        transform.parent = droneFPCameraPos; // 1인칭의 경우, 드론의 자식으로 카메라 설정. 그 외는 부모관계 X

        target = drone; // 드론 기준으로 카메라 설정

        transform.position = droneFPCameraPos.position;
        transform.eulerAngles = droneFPCameraPos.eulerAngles;
    }

    private void RoomViewCamera() // 방 관찰 카메라
    {
        transform.parent = null; // 1인칭의 경우, 드론의 자식으로 카메라 설정. 그 외는 부모관계 X

        target = room; // 방 기준으로 카메라 설정

        Vector3 desiredPosition = target.position + roomViewOffset; // 카메라가 이동할 최종 위치 구하기
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed); // 카메라가 현재 프레임에서 이동중인 위치 구하기
        transform.position = smoothedPosition;

        transform.LookAt(target); // 카메라가 목표를 바라보기

        if (Camera.main.transform.position != this.transform.position) // 카메라 위치 보정
            Camera.main.transform.position = this.transform.position;
    }
}
