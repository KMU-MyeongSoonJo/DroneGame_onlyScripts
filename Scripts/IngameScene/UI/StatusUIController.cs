using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/**
 * 
 * Ingame UI 의 좌상단 Status 탭에 표시되는 정보들을 제어하는 스크립트
 * 
 * */
 
public class StatusUIController : MonoBehaviour
{
    public Image day; // 낮 아이콘
    public Text repute; // 평판 표시 텍스트


    /// <summary>
    /// 낮밤 아이콘을 전환하는 코루틴 함수
    /// </summary>
    IEnumerator lightIconController()
    {
        //night.rectTransform.position.y = 100 : -100 = 0 : 3600
        while (true)
        {

            // 낮->밤 전환
            if (LightManager.instance.timer >= 2250 && LightManager.instance.timer <= 2550)
            {
                float yPos = (2550 - LightManager.instance.timer) / 300; // 1 ~ 0
                yPos *= 100; // 100 ~ 0
                yPos -= 100; // 0 ~ -100

                day.rectTransform.localPosition = new Vector3(0, yPos, 0);
            }
            // 밤->낮 전환
            else if(LightManager.instance.timer >= 3450 || LightManager.instance.timer <= 150)
            {
                float yPos = LightManager.instance.timer;
                if (yPos > 3000) yPos -= 3600; // -150 ~ 150
                yPos *= -1; // 150 ~ -150
                yPos += 150; // 300 ~ 0
                yPos /= 3; // 100 ~ 0

                day.rectTransform.localPosition = new Vector3(0, yPos, 0);
            }
            // 이미 밤이라면
            else if (LightManager.instance.timer > 2550)
            {
                day.rectTransform.localPosition = new Vector3(0, 100, 0);
            }

            repute.text = "평판 : " + PlayerPrefsData.instance.repute;
                       
            yield return null;
        }
    }
    
    private void Start()
    {
        StartCoroutine(lightIconController());
    }
}
