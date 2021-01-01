using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/************************************
* 
* 설명:
*   꾸미기 아이템의 속성을 지정해주고 이동이나 애니메이션을 실행시키는 스크립트
*   잠금 해제된 아이템은 PlayerPrefs에 저장되며, 숫자를 부여받습니다. -> 1: 보유중, 2: 착용중
* 
************************************/

public class DecorationItem : MonoBehaviour
{
    public Text costText; // 구입에 필요한 평판
    public Image lockedImage; // 잠김 이미지
    [Space]
    public int licenseNeed; // 최소 요구 라이센스 등급
    public int reputeCost; // 구입에 필요한 평판
    public bool locked; // 아이템 잠김 (구입시 해제됩니다)

    private Vector2 moveDestination; // UI가 이동할 때, 목적지
    private RectTransform UItransform;
    private Animator animator;

    private void Awake()
    {
        UItransform = GetComponent<RectTransform>();
        animator = this.GetComponent<Animator>();
        moveDestination = this.UItransform.anchoredPosition; // 목적지 기본값 설정

        CheckItemStatus(); // 꾸미기 아이템 상태 확인 (구입 필요, 보유중, 착용중)
    }

    private void Update()
    {
        UpdateMove(); // 꾸미기 아이템 UI 위치 업데이트
    }

    private void UpdateMove() // 꾸미기 아이템 UI 위치 업데이트
    {
        if (UItransform.anchoredPosition == moveDestination) // 이미 UI가 목적지에 도착했으면 이동 생략
            return;

        UItransform.anchoredPosition = Vector2.Lerp(UItransform.anchoredPosition, moveDestination, 0.1f); // 자연스러운 이동을 위한 보간
    }

    public void SetDestination(Vector2 _destination) // 서서히 목적지 까지 이동할 때 사용할 함수
    {
        moveDestination = _destination;
    }

    public void SetPosition(Vector2 _destination) // 즉시 목적지 까지 이동할 때 사용할 함수
    {
        moveDestination = _destination;
        UItransform.anchoredPosition = _destination;
    }

    public void SetBoolAnimation(string _id, bool _bool) // 애니메이션 작동을 위한 애니메이터의 Bool 변수 수정
    {
        animator.SetBool(_id, _bool);
    }

    public void CheckItemStatus() // 꾸미기 아이템 상태 확인 (구입 필요, 보유중, 착용중)
    {
        // 이미 착용중인 아이템 확인
        if (PlayerPrefs.GetString("droneDecoration") == this.name ||
            PlayerPrefs.GetString("operatorDecoration") == this.name)
            Worn();
        // 꾸미기 아이템 보유 상태 확인 (이전에 구입했거나 처음부터 사용 가능한 아이템은 잠금 해제)
        else if (PlayerPrefs.HasKey(this.name) || locked == false)
            Unlock();
        else // 이전에 구입한 적 없음
            costText.text = reputeCost + " 평판"; // 가격 보여주기
    }

    private void Unlock()
    {
        locked = false; // 잠금 해제
        lockedImage.color = new Color(1, 1, 1, 0); // 잠금 이미지 숨기기
        costText.text = "보유중"; // 가격 숨기기
        costText.color = new Color(1, 1, 1, 0.5f); // 글자를 회색으로 변경
    }

    private void Worn()
    {
        locked = false; // 잠금 해제
        lockedImage.color = new Color(1, 1, 1, 0); // 잠금 이미지 숨기기
        costText.text = "착용중"; // 현재 착용중임을 표시
        costText.color = new Color(1, 1, 0); // 글자를 노란색으로 변경
    }
}