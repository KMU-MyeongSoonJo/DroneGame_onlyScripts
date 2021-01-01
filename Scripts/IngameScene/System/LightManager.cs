using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Directional Light 에 컴포넌트 해주세요

public class LightManager : MonoBehaviour
{
    public static LightManager instance;

    Light light;
    public float maxTime = 3600.0f; // 하루를 몇초로 설정할것인가 기본 : 3600초(1시간)
    private int myComHour; // 컴퓨터의 시각 저장변수
    public float timer; // 시간흐름
    public int timeSpeed = 1; // 시간 배속


    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this);


        light = GetComponent<Light>();

        // 컴퓨터의 시각을 가져옴 (ex.오후3시3분 -> 15) -6한건 0이 해뜨는 시각
        if (DateTime.Now.Hour >= 6)
            myComHour = DateTime.Now.Hour - 6;
        else
            myComHour = DateTime.Now.Hour + 18;

        timer = maxTime / 24 * myComHour; // 현실 시간을 게임 시간으로 변경
        //Debug.Log(DateTime.Now.Hour);
        //Debug.Log("Timer" + timer);

        //조명 초기화
        if (myComHour > 1 && myComHour < 15)
            light.intensity = 1.5f;
        if (myComHour.Equals(0) || myComHour.Equals(15))
            light.intensity = 0.75f;
        if (15 < myComHour && myComHour < 23)
            light.intensity = 0.0f;

        
    }

    // Update is called once per frame
    void Update()
    {
        // 해 위치(조명 각도)
        if (timer <= 2400.0f)
        {
            //before :: light.transform.localRotation = Quaternion.Euler(180.0f / 2400.0f * timer, 50, 0);
            light.transform.localRotation = Quaternion.Euler(0.075f * timer, 50, 0);
        }
        if (timer > 2400.0f)
        {
            //before :: light.transform.localRotation = Quaternion.Euler(180.0f + 180.0f / 1200.0f * timer, 50, 0);
            light.transform.localRotation = Quaternion.Euler(180.0f + 0.15f * timer, 50, 0);
        }

        // 밝기 밝기 1.5f / 300.0f(초) = 0.005f로 지정
        if (timer > 3450 || timer < 150) // 일출
        {
            if (light.intensity < 1.5f)
                light.intensity += 0.005f * Time.deltaTime * timeSpeed;
        }
        if (timer > 2250 && timer < 2550 && light.intensity > 0.005) // 일몰
        {
            light.intensity -= 0.005f * Time.deltaTime * timeSpeed;
        }

        timer += Time.deltaTime * timeSpeed;

        if (timer > 3600.0f)
            timer -= 3600.0f;

        light.intensity = Mathf.Clamp(light.intensity, 0.005f, 1.5f);
    }
}
