using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/************************************
* 
* 설명:
*   플레이어 방에서의 조작을 담당하는 스크립트
* 
************************************/

public class MyRoomManager : MonoBehaviour
{
    public static MyRoomManager instance;

    public GameObject Player; // 플레이어 오브젝트
    public GameObject Drone; // 드론 오브젝트
    public GameObject Cursor; // 가구 수정시 활성화할 커서
    public Interior PickedObject; // 현재 선택된 인테리어 오브젝트
    [Space]
    public Material red;
    public Material green;
    public Material blue;
    [Space]
    public bool menuMode; // 메뉴 활성화 모드
    public bool modifyMode; // 가구 & 트랙 수정 모드 (On & Off)
    public bool interfererAtCursor; // 커서 위치에 가구가 아닌 장애물이 있으면 true (ex 플레이어, 드론)

    private bool throttleNeutral;
    private bool yawNeutral;
    private bool pitchNeutral;
    private bool rollNeutral;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (Cursor.activeInHierarchy)
            CursorColorChange(); // 커서 상태에 따라 색상 변화주기

        SwitchPlayerStatus(); // 플레이어 상태 전환

        #region GET INPUT
        if (TalkManager.instance.talking == false) // 대화 중이 아닐 때만 작동
        {
            if (Input.GetButtonDown("Vertical"))
            {
                if (Input.GetAxis("Vertical") > 0)
                    W();
                else
                    S();
            }

            if (throttleNeutral == true && Input.GetAxis("Throttle") != 0) // 조이스틱 GetButtonDown 수동 인식
            {
                if (Input.GetAxis("Throttle") > 0.5f)
                    W();
                if (Input.GetAxis("Throttle") < -0.51f)
                    S();
                throttleNeutral = false;
            }
            else if (Mathf.Abs(Input.GetAxis("Throttle")) <= 0.5f)
                throttleNeutral = true;

            if (Input.GetButtonDown("Horizontal"))
            {
                if (Input.GetAxis("Horizontal") > 0)
                    D();
                else
                    A();
            }

            if (yawNeutral == true && Input.GetAxis("Yaw") != 0) // 조이스틱 GetButtonDown 수동 인식
            {
                if (Input.GetAxis("Yaw") > 0.5f)
                    D();
                if (Input.GetAxis("Yaw") < -0.5f)
                    A();
                yawNeutral = false;
            }
            else if (Mathf.Abs(Input.GetAxis("Yaw")) <= 0.5f)
                yawNeutral = true;

            if (Input.GetButtonDown("Vertical_r"))
            {
                if (Input.GetAxis("Vertical_r") > 0)
                    K();
                else
                    I();
            }

            if (pitchNeutral == true && Input.GetAxis("Pitch") != 0) // 조이스틱 GetButtonDown 수동 인식
            {
                if (Input.GetAxis("Pitch") > 0.5f)
                    K();
                if (Input.GetAxis("Pitch") < -0.5f)
                    I();
                pitchNeutral = false;
            }
            else if (Mathf.Abs(Input.GetAxis("Pitch")) <= 0.5f)
                pitchNeutral = true;

            if (Input.GetButtonDown("Horizontal_r"))
            {
                if (Input.GetAxis("Horizontal_r") > 0)
                    L();
                else
                    J();
            }

            if (rollNeutral == true && Input.GetAxis("Roll") != 0) // 조이스틱 GetButtonDown 수동 인식
            {
                if (Input.GetAxis("Roll") > 0.5f)
                    L();
                if (Input.GetAxis("Roll") < -0.5f)
                    J();
                rollNeutral = false;
            }
            else if (Mathf.Abs(Input.GetAxis("Roll")) <= 0.5f)
                rollNeutral = true;
        }
        #endregion
    }

    private void W()
    {
        if (menuMode == false) // 메뉴가 열려있지 않을 때
        {
            if (modifyMode) // 가구 & 트랙 수정 모드
            {
                #region CURSOR MOVE UP
                if (PickedObject == false) // 오브젝트를 선택중이 아닐 때
                    Cursor.transform.position = new Vector3(Cursor.transform.position.x, Mathf.Clamp(Cursor.transform.position.y + 1, 0, InteriorArrayManager.instance.interiorArray.GetLength(1) - 1), Cursor.transform.position.z); // 커서 이동 (Y좌표 + 1)

                else if (PickedObject) // 오브젝트를 선택중인 경우
                {
                    Cursor.transform.position = new Vector3(Cursor.transform.position.x, Mathf.Clamp(Cursor.transform.position.y + 1, 0 + PickedObject.placeRangeY, InteriorArrayManager.instance.interiorArray.GetLength(1) - 1 - PickedObject.placeRangeY), Cursor.transform.position.z); // 커서 이동 (Y좌표 + 1), 선택중인 오브젝트 크기에 따라 이동 제한 있음
                    PickedObject.transform.position = Cursor.transform.position; // 오브젝트가 커서를 따라옵니다.
                }
                #endregion
            }
            else // 플레이어 이동 모드
            {
                
            }
        }
    }

    private void S()
    {
        if (menuMode == false) // 메뉴가 열려있지 않을 때
        {
            if (modifyMode) // 가구 & 트랙 수정 모드
            {
                #region CURSOR MOVE DOWN
                if (PickedObject == false) // 오브젝트를 선택중이 아닐 때
                    Cursor.transform.position = new Vector3(Cursor.transform.position.x, Mathf.Clamp(Cursor.transform.position.y - 1, 0, InteriorArrayManager.instance.interiorArray.GetLength(1) - 1), Cursor.transform.position.z); // 커서 이동 (Y좌표 - 1)

                else if (PickedObject) // 오브젝트를 선택중인 경우
                {
                    Cursor.transform.position = new Vector3(Cursor.transform.position.x, Mathf.Clamp(Cursor.transform.position.y - 1, 0 + PickedObject.placeRangeY, InteriorArrayManager.instance.interiorArray.GetLength(1) - 1 - PickedObject.placeRangeY), Cursor.transform.position.z); // 커서 이동 (Y좌표 - 1), 선택중인 오브젝트 크기에 따라 이동 제한 있음
                    PickedObject.transform.position = Cursor.transform.position; // 오브젝트가 커서를 따라옵니다.
                }
                #endregion
            }
        }
    }

    private void A()
    {
        if (menuMode == false) // 메뉴가 열려있지 않을 때
        {
            if (modifyMode) // 가구 & 트랙 수정 모드
            {
                #region TURN SELECTED OBJECT LEFT
                if (PickedObject) // 오브젝트를 선택중인 경우
                {
                    PickedObject.transform.rotation = Quaternion.Euler(0, PickedObject.transform.eulerAngles.y - 90, 0); // 오브젝트를 왼쪽으로 90도 회전
                }
                #endregion
            }
        }
    }

    private void D()
    {
        if (menuMode == false) // 메뉴가 열려있지 않을 때
        {
            if (modifyMode) // 가구 & 트랙 수정 모드
            {
                #region TURN SELECTED OBJECT RIGHT
                if (PickedObject) // 오브젝트를 선택중인 경우
                {
                    PickedObject.transform.rotation = Quaternion.Euler(0, PickedObject.transform.eulerAngles.y + 90, 0); // 오브젝트를 오른쪽으로 90도 회전
                }
                #endregion
            }
        }
    }

    private void I()
    {
        if (menuMode == false) // 메뉴가 열려있지 않을 때
        {
            if (modifyMode) // 가구 & 트랙 수정 모드
            {
                #region CURSOR MOVE FRONT
                if (PickedObject == false) // 오브젝트를 선택중이 아닐 때
                    Cursor.transform.position = new Vector3(Cursor.transform.position.x, Cursor.transform.position.y, Mathf.Clamp(Cursor.transform.position.z + 1, 0, InteriorArrayManager.instance.interiorArray.GetLength(2) - 1)); // 커서 이동 (Z좌표 + 1)

                else if (PickedObject) // 오브젝트를 선택중인 경우
                {
                    Cursor.transform.position = new Vector3(Cursor.transform.position.x, Cursor.transform.position.y, Mathf.Clamp(Cursor.transform.position.z + 1, 0 + PickedObject.placeRangeZ, InteriorArrayManager.instance.interiorArray.GetLength(2) - 1 - PickedObject.placeRangeZ)); // 커서 이동 (Z좌표 + 1), 선택중인 오브젝트 크기에 따라 이동 제한 있음
                    PickedObject.transform.position = Cursor.transform.position; // 오브젝트가 커서를 따라옵니다.
                }
                #endregion
            }
        }
    }

    private void K()
    {
        if (menuMode == false) // 메뉴가 열려있지 않을 때
        {
            if (modifyMode) // 가구 & 트랙 수정 모드
            {
                #region CURSOR MOVE BACK
                if (PickedObject == false) // 오브젝트를 선택중이 아닐 때
                    Cursor.transform.position = new Vector3(Cursor.transform.position.x, Cursor.transform.position.y, Mathf.Clamp(Cursor.transform.position.z - 1, 0, InteriorArrayManager.instance.interiorArray.GetLength(2) - 1)); // 커서 이동 (Z좌표 - 1)

                else if (PickedObject) // 오브젝트를 선택중인 경우
                {
                    Cursor.transform.position = new Vector3(Cursor.transform.position.x, Cursor.transform.position.y, Mathf.Clamp(Cursor.transform.position.z - 1, 0 + PickedObject.placeRangeZ, InteriorArrayManager.instance.interiorArray.GetLength(2) - 1 - PickedObject.placeRangeZ)); // 커서 이동 (Z좌표 - 1), 선택중인 오브젝트 크기에 따라 이동 제한 있음
                    PickedObject.transform.position = Cursor.transform.position; // 오브젝트가 커서를 따라옵니다.
                }
                #endregion
            }
        }
    }

    private void J()
    {
        if (menuMode == false) // 메뉴가 열려있지 않을 때
        {
            if (modifyMode) // 가구 & 트랙 수정 모드
            {
                #region CURSOR MOVE LEFT
                if (PickedObject == false) // 오브젝트를 선택중이 아닐 때
                    Cursor.transform.position = new Vector3(Mathf.Clamp(Cursor.transform.position.x - 1, 0, InteriorArrayManager.instance.interiorArray.GetLength(0) - 1), Cursor.transform.position.y, Cursor.transform.position.z); // 커서 이동 (X좌표 - 1)

                else if (PickedObject) // 오브젝트를 선택중인 경우
                {
                    Cursor.transform.position = new Vector3(Mathf.Clamp(Cursor.transform.position.x - 1, 0 + PickedObject.placeRangeX, InteriorArrayManager.instance.interiorArray.GetLength(0) - 1 - PickedObject.placeRangeX), Cursor.transform.position.y, Cursor.transform.position.z); // 커서 이동 (X좌표 - 1), 선택중인 오브젝트 크기에 따라 이동 제한 있음
                    PickedObject.transform.position = Cursor.transform.position; // 오브젝트가 커서를 따라옵니다.
                }
                #endregion
            }
        }
    }

    private void L()
    {
        if (menuMode == false)
        {
            if (modifyMode) // 가구 & 트랙 수정 모드
            {
                #region CURSOR MOVE RIGHT
                if (PickedObject == false) // 오브젝트를 선택중이 아닐 때
                    Cursor.transform.position = new Vector3(Mathf.Clamp(Cursor.transform.position.x + 1, 0, InteriorArrayManager.instance.interiorArray.GetLength(0) - 1), Cursor.transform.position.y, Cursor.transform.position.z); // 커서 이동 (X좌표 + 1)

                else if (PickedObject) // 오브젝트를 선택중인 경우
                {
                    Cursor.transform.position = new Vector3(Mathf.Clamp(Cursor.transform.position.x + 1, 0 + PickedObject.placeRangeX, InteriorArrayManager.instance.interiorArray.GetLength(0) - 1 - PickedObject.placeRangeX), Cursor.transform.position.y, Cursor.transform.position.z); // 커서 이동 (X좌표 + 1), 선택중인 오브젝트 크기에 따라 이동 제한 있음
                    PickedObject.transform.position = Cursor.transform.position; // 오브젝트가 커서를 따라옵니다.
                }
                #endregion
            }
        }
    }

    public void LoadObject(Interior interior) // 오브젝트 불러오기
    {
        Vector3 objectCreatePos = new Vector3((InteriorArrayManager.instance.arraySizeX * 0.5f), 0, (InteriorArrayManager.instance.arraySizeZ * 0.5f)); // 오브젝트 생성 좌표 구하기 (방 중앙)

        if (interior.sizeX % 2 == 0) // 불러올 오브젝트의 X사이즈가 2의 배수라면
            objectCreatePos.x -= 0.5f; // 오브젝트 생성 좌표를 왼쪽으로 0.5칸 만큼 이동
        if (interior.sizeY % 2 == 0) // 불러올 오브젝트의 Y사이즈가 2의 배수라면
            objectCreatePos.y += 0.5f; // 오브젝트 생성 좌표를 위쪽으로 0.5칸 만큼 이동
        if (interior.sizeZ % 2 == 0) // 불러올 오브젝트의 Z사이즈가 2의 배수라면
            objectCreatePos.z -= 0.5f; // 오브젝트 생성 좌표를 앞쪽으로 0.5칸 만큼 이동

        objectCreatePos.y += ((interior.sizeY + 1) / 2) - 1; // 오브젝트의 Y사이즈에 비례해 오브젝트 생성 Y좌표를 증가

        Cursor.transform.position = objectCreatePos; // 커서를 오브젝트 생성 좌표로 이동시키기
        Cursor.transform.localScale = new Vector3(interior.sizeX + 0.01f, interior.sizeY + 0.01f, interior.sizeZ + 0.01f); // 커서 크기를 불러올 오브젝트 크기에 맞게 설정
        PickedObject = Instantiate(interior.gameObject, objectCreatePos, Quaternion.identity).GetComponent<Interior>(); // 오브젝트 생성 좌표에 오브젝트 생성 후 Pick

        if (PickedObject.GetComponentInChildren<Collider>())
            foreach (Collider i in PickedObject.GetComponentsInChildren<Collider>())
                i.enabled = false; // Collider 비활성화
    }

    public void TrySelectObject() // 오브젝트 선택 시도
    {
        if (InteriorArrayManager.instance.interiorArray[(int)Cursor.transform.position.x, (int)Cursor.transform.position.y, (int)Cursor.transform.position.z]) // 현재 위치에 선택가능한 오브젝트가 있다면 (3차원 배열에서 찾음)
        {
            StartCoroutine(RoomUIManager.instance.OpenFurnitureMenu());
        }
        else // 선택할 오브젝트가 없을 때
            StartCoroutine(RoomUIManager.instance.OpenInteriorMenu()); // 가구 수정할 때 전용 메뉴 활성화 (가구 불러오기, 편집 종료)
    }

    public void PickObject() // 오브젝트 잡기
    {
        PickedObject = InteriorArrayManager.instance.interiorArray[(int)Cursor.transform.position.x, (int)Cursor.transform.position.y, (int)Cursor.transform.position.z]; // 커서 위치의 오브젝트 선택

        InteriorArrayManager.instance.UnplaceFromArray(PickedObject); // 배열에서 해당 오브젝트 빼기

        Cursor.transform.position = PickedObject.transform.position; // 커서를 오브젝트 중앙으로 이동
        Cursor.transform.localScale = new Vector3(PickedObject.sizeX + 0.01f, PickedObject.sizeY + 0.01f, PickedObject.sizeZ + 0.01f); // 커서 크기를 오브젝트 크기에 맞게 설정

        if (PickedObject.GetComponentInChildren<Collider>())
            foreach (Collider i in PickedObject.GetComponentsInChildren<Collider>())
                i.enabled = false; // Collider 비활성화
    }

    public void TryPutObject() // 오브젝트 놓기 시도
    {
        if (CheckPickedObjectCanPutHere() == true && interfererAtCursor == false) // 현재 위치에 다른 오브젝트가 없다면
        {
            InteriorArrayManager.instance.PlaceAtArray(PickedObject); // 배열에 해당 오브젝트 넣기

            Cursor.transform.localScale = new Vector3(1.01f, 1.01f, 1.01f); // 커서 크기를 원래대로 되돌리기
            Cursor.transform.position = new Vector3((int)Cursor.transform.position.x, (int)Cursor.transform.position.y, (int)Cursor.transform.position.z); // 커서 XYZ 좌표를 정수로 되돌리기

            if (PickedObject.GetComponentInChildren<Collider>())
                foreach (Collider i in PickedObject.GetComponentsInChildren<Collider>())
                    i.enabled = true; // Collider 활성화

            PickedObject = null; // 오브젝트 선택 해제

            RoomUIManager.instance.UpdateControlGuide(); // 가구 편집 조작 가이드 업데이트
        }
    }

    public void RemoveObject() // 오브젝트 삭제
    {
        PickedObject = InteriorArrayManager.instance.interiorArray[(int)Cursor.transform.position.x, (int)Cursor.transform.position.y, (int)Cursor.transform.position.z]; // 커서 위치의 오브젝트 선택

        for (int x = (int)(PickedObject.transform.position.x - PickedObject.placeRangeX); x <= PickedObject.transform.position.x + PickedObject.placeRangeX; x++) // 가구가 차지하는 X범위 만큼
        {
            for (int y = (int)(PickedObject.transform.position.y - PickedObject.placeRangeY); y <= PickedObject.transform.position.y + PickedObject.placeRangeY; y++) // 가구가 차지하는 Y범위 만큼
            {
                for (int z = (int)(PickedObject.transform.position.z - PickedObject.placeRangeZ); z <= PickedObject.transform.position.z + PickedObject.placeRangeZ; z++) // 가구가 차지하는 Z범위 만큼
                {
                    InteriorArrayManager.instance.interiorArray[x, y, z] = null; // 3차원 배열에서 커서 위치의 오브젝트를 제거
                }
            }
        }

        InteriorArrayManager.instance.UnplaceFromArray(PickedObject); // 배열에 해당 오브젝트 넣기 // 3차원 배열에서 커서 위치의 오브젝트를 제거
        PickedObject.gameObject.SetActive(false); // 선택한 오브젝트 삭제
        PickedObject = null; // 선택된 오브젝트 없음
    }

    private void CursorColorChange() // 커서 상태에 따라 색상 변화주기
    {
        if (PickedObject) // 커서로 선택한 오브젝트가 있을 때
        {
            if (CheckPickedObjectCanPutHere() == true && interfererAtCursor == false) // 현재 위치에 다른 오브젝트가 없다면
                Cursor.GetComponent<Renderer>().material = green;
            else // 현재 위치에 이미 다른 오브젝트가 있다면
                Cursor.GetComponent<Renderer>().material = red;
        }
        else // 커서로 선택한 오브젝트가 없을 떄
            Cursor.GetComponent<Renderer>().material = blue;
    }

    private bool CheckPickedObjectCanPutHere() // 이곳에 선택된 오브젝트를 놓을 수 있는지 체크
    {
        for (int x = (int)(PickedObject.transform.position.x - PickedObject.placeRangeX); x <= PickedObject.transform.position.x + PickedObject.placeRangeX; x++) // 가구가 차지하는 X범위 만큼
        {
            for (int y = (int)(PickedObject.transform.position.y - PickedObject.placeRangeY); y <= PickedObject.transform.position.y + PickedObject.placeRangeY; y++) // 가구가 차지하는 Y범위 만큼
            {
                for (int z = (int)(PickedObject.transform.position.z - PickedObject.placeRangeZ); z <= PickedObject.transform.position.z + PickedObject.placeRangeZ; z++) // 가구가 차지하는 Z범위 만큼
                {
                    if (InteriorArrayManager.instance.interiorArray[x, y, z] != null) // 중복된 위치에 다른 가구가 있는지 검사
                        return false; // 존재하면 false반환
                }
            }
        }

        return true; // 중복된 위치에 다른 가구가 없다면 true 반환
    }

    public void EnableMenuMode() // 메뉴 선택 모드 활성화
    {
        menuMode = true;
        RoomUIManager.instance.UpdateControlGuide(); // 가구 편집 조작 가이드 업데이트
    }

    public void DisableMenuMode() // 메뉴 선택 모드 비활성화
    {
        StartCoroutine(DisableMenuMode2()); // Space 버튼을 눌러 메뉴를 닫자마자 다시 똑같은 메뉴가 열리는 현상 방지를 위해 코루틴 사용
    }

    IEnumerator DisableMenuMode2()
    {
        yield return null; // 아래 문장들은 다음 프레임에 실행
        menuMode = false;
        RoomUIManager.instance.UpdateControlGuide(); // 가구 편집 조작 가이드 업데이트
    }

    public void EnableModifyMode() // 가구 & 트랙 수정 모드 활성화
    {
        modifyMode = true;
        //InRoomCamera.instance.cameraMode = InRoomCamera.CameraMode.RoomView; // 카메라 모드 변경
        InRoomCamera.instance.ChangeCameraMode(InRoomCamera.CameraMode.RoomView);
        Drone.GetComponent<DroneMovement>().SetStatus(DroneStatus.Recall); // 드론 플레이어 위치로 복귀
        Cursor.SetActive(true); // 가구 수정용 커서 활성화
        Cursor.transform.position = new Vector3((int)Player.transform.position.x, (int)Player.transform.position.y, (int)Player.transform.position.z); // 현재 플레이어 위치로 커서 이동
        RoomUIManager.instance.UpdateControlGuide(); // 가구 편집 조작 가이드 업데이트
    }

    public void DisableModifyMode() // 가구 & 트랙 수정 모드 비활성화
    {
        modifyMode = false;
        //InRoomCamera.instance.cameraMode = InRoomCamera.CameraMode.CharacterView; // 카메라 모드 변경
        InRoomCamera.instance.ChangeCameraMode(InRoomCamera.CameraMode.CharacterView);
        Cursor.SetActive(false); // 가구 수정용 커서 비활성화
        RoomUIManager.instance.UpdateControlGuide(); // 가구 편집 조작 가이드 업데이트

        InteriorSaver.instance.Save();
    }

    private void SwitchPlayerStatus() // 플레이어 상태 전환
    {
        if (PlayerController.instance.GetStatus() == PlayerStatus.Control)
            if (menuMode || modifyMode)
                PlayerController.instance.setStatus(PlayerStatus.Trouble); // 메뉴가 열려 있거나 가구 수정 중이라면 플레이어 조작 불가

        if (PlayerController.instance.GetStatus() == PlayerStatus.Trouble)
            if (!menuMode && !modifyMode)
                PlayerController.instance.setStatus(PlayerStatus.Control); // 열린 메뉴가 없고 가구 수정 중이 아니라면 플레이어 조작 가능
    }

    public void LoadScene(string SceneName)
    {
        Recorder.instance.previousScene = SceneManager.GetActiveScene().name;
        LoadingManager.LoadScene(SceneName, SceneManager.GetActiveScene().name);
    }
}