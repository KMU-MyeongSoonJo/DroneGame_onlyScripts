using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 밤에만 활성화 할 Light 에 컴포넌트 해주세요

public class EnableLightWhenNight : MonoBehaviour
{
    Light light; // 조정할 조명

    public float maxIntensity; // 조명 최대 Intencity
    bool turnon = true; // 활성화


    private void Awake()
    {
        light = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        if (turnon == true && LightManager.instance.timer < 2400) // 켜져 있음 & 낮
            SetBrightness(0, false); // 조명 끄기
        else if (turnon == false && LightManager.instance.timer >= 2400) // 꺼져 있음 & 밤
            SetBrightness(maxIntensity, true); // 조명 켜기
    }

    void SetBrightness(float _intensity, bool _bool) // 밝기 조정 (intensity, 활성화 여부)
    {
        light.intensity = _intensity;
        turnon = _bool;
    }
}
