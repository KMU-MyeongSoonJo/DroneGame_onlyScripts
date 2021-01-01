using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/************************************
* 
* 설명:
*   꾸미기 씬에서의 카메라를 제어하는 스크립트
* 
************************************/

public class DecorationRoomCamera : MonoBehaviour
{
    public static DecorationRoomCamera instance;

    public enum ViewMode { lookDrone, lookDefault, lookCharacter };
    public ViewMode viewMode = ViewMode.lookDefault;

    [Header("Camera Settings")]
    public Transform lookDroneCameraPos;
    public Transform lookDroneCameraLook;
    [Space]
    public Transform defaultCameraPos;
    public Transform defaultCameraLook;
    [Space]
    public Transform lookCharCameraPos;
    public Transform lookCharCameraLook;
    [Space]
    public float smoothSpeed;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        UpdateCamera(); // 카메라 위치 업데이트
    }

    public void GoLeft() // 카메라 왼쪽으로 이동
    {
        if (viewMode > ViewMode.lookDrone)
            viewMode -= 1;
    }

    public void GoRight() // 카메라 오른쪽으로 이동
    {
        if (viewMode < ViewMode.lookCharacter)
            viewMode += 1;
    }

    private void UpdateCamera() // 카메라 위치 업데이트
    {
        Vector3 smoothedPosition;

        switch (viewMode)
        {
            case ViewMode.lookDrone:
                if (this.transform.position == lookDroneCameraPos.position) // 이미 카메라가 목적지에 도달했을 경우 더 이상 이동할 필요 없음
                    return;
                smoothedPosition = Vector3.Lerp(transform.position, lookDroneCameraPos.position, smoothSpeed * Time.deltaTime); // 카메라의 자연스러운 이동을 위한 보간 처리
                this.transform.position = smoothedPosition;
                this.transform.LookAt(lookDroneCameraLook); // 카메라가 꾸밀 오브젝트를 보기
                break;
            case ViewMode.lookDefault:
                if (this.transform.position == defaultCameraPos.position)
                    return;
                smoothedPosition = Vector3.Lerp(transform.position, defaultCameraPos.position, smoothSpeed * Time.deltaTime);
                this.transform.position = smoothedPosition;
                this.transform.LookAt(defaultCameraLook);
                break;
            case ViewMode.lookCharacter:
                if (this.transform.position == lookCharCameraPos.position)
                    return;
                smoothedPosition = Vector3.Lerp(transform.position, lookCharCameraPos.position, smoothSpeed * Time.deltaTime);
                this.transform.position = smoothedPosition;
                this.transform.LookAt(lookCharCameraLook);
                break;
        }
    }
}
