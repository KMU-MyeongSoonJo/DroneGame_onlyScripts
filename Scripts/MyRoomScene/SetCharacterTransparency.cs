using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/************************************
* 
* 작성자: 송창하
* 설명:
*   오브젝트 가까이에 카메라가 있으면 오브젝트를 투명하게 만드는 스크립트
* 
************************************/

public class SetCharacterTransparency : MonoBehaviour
{
    private Renderer renderer;

    private void Awake()
    {
        renderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        CheckDistanceFromCamera(); // 카메라와의 거리 검사
    }

    private void CheckDistanceFromCamera() // 카메라와의 거리 검사
    {
        if (Vector3.Distance(transform.position, Camera.main.transform.position) < 1.0f) // 해당 오브젝트와 카메라와의 거리가 1보다 작다면
            SetMaterialTransparency(); // 투명화
        else
            SetMaterialOpacity(); // 불투명화
    }

    private void SetMaterialTransparency() // 투명화
    {
        Color altColor = renderer.material.color; // 현재 메터리얼 색상 가져오기
        altColor.a = 0; // 메터리얼 알파값 0 (Color32 -> 0)
        renderer.material.color = altColor; // 대입
    }

    private void SetMaterialOpacity() // 불투명화
    {
        Color altColor = renderer.material.color; // 현재 메터리얼 색상 가져오기
        altColor.a = 1; // 메터리얼 알파값 1 (Color32 -> 255)
        renderer.material.color = altColor; // 대입
    }
}
