using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/************************************
* 
* 설명:
*   가구 불러오기용 버튼들을 관리하는 스크립트
* 
************************************/

public class InteriorButtonManager : MonoBehaviour
{
    public static InteriorButtonManager instance;

    public enum Category { Bed, Chair, Table, Other }

    public List<GameObject> interiorList;
    public GameObject interiorListUI;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        foreach (InteriorButton button in interiorListUI.GetComponentsInChildren<InteriorButton>()) // 가구 버튼 리스트 만들기
        {
            interiorList.Add(button.gameObject);
        }
    }

    public void SetCategory(string _category) // 현재 카테고리 설정 (가구 불러오기 UI에서 카테고리 버튼을 눌렀을 때 작동됩니다)
    {
        switch (_category)
        {
            case "Bed":
                foreach (GameObject button in interiorList)
                {
                    if (button.GetComponent<InteriorButton>().category == Category.Bed) // 플레이어가 선택한 카테고리와 동일한 카테고리의 가구는 보이게 하기
                        button.SetActive(true);
                    else // 나머지는 숨기기
                        button.SetActive(false);
                }
                break;
            case "Chair":
                foreach (GameObject button in interiorList)
                {
                    if (button.GetComponent<InteriorButton>().category == Category.Chair)
                        button.SetActive(true);
                    else
                        button.SetActive(false);
                }
                break;
            case "Table":
                foreach (GameObject button in interiorList)
                {
                    if (button.GetComponent<InteriorButton>().category == Category.Table)
                        button.SetActive(true);
                    else
                        button.SetActive(false);
                }
                break;
            case "Other":
                foreach (GameObject button in interiorList)
                {
                    if (button.GetComponent<InteriorButton>().category == Category.Other)
                        button.SetActive(true);
                    else
                        button.SetActive(false);
                }
                break;
        }
    }
}
