using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/************************************
* 
* 설명:
*   Directional Light의 밝기에 따라 하늘 메터리얼의 속성을 변경시켜주는 스크립트
*   
*   * Directional Light에 컴포넌트 해주세요
* 
************************************/

public class SkyManager : MonoBehaviour
{
    public Material sky;

    private Light directionalLight;
    private float lightIntensity;
    private float cloudIntensity;
    private float shadowIntensity;

    private void Awake()
    {
        directionalLight = GetComponent<Light>();
    }

    private void Start()
    {
        sky.SetColor("_SkyColor", new Color(0, 0.3f, 1.0f));
        sky.SetColor("_EquatorColor", new Color(1.0f, 1.0f, 1.0f));
        sky.SetColor("_GroundColor", new Color(1.0f, 1.0f, 1.0f));
        sky.SetFloat("_StarsIntensity", 0);
        sky.SetColor("_CloudsLightColor", new Color(1.0f, 1.0f, 1.0f));
        sky.SetColor("_CloudsShadowColor", new Color(1.0f, 1.0f, 1.0f));
        RenderSettings.fogColor = new Color(1.0f, 1.0f, 1.0f);
    }

    private void Update()
    {
        lightIntensity = directionalLight.intensity;
        cloudIntensity = Mathf.Clamp(lightIntensity, 0.1f, 1.0f);

        sky.SetColor("_SkyColor", new Color(0, lightIntensity * 0.3f, lightIntensity));
        sky.SetColor("_EquatorColor", new Color(lightIntensity * 0.8f, lightIntensity * 0.8f, lightIntensity * 0.8f));
        sky.SetColor("_GroundColor", new Color(lightIntensity * 0.8f, lightIntensity * 0.8f, lightIntensity * 0.8f));
        sky.SetFloat("_StarsIntensity", 1.5f - lightIntensity);
        sky.SetColor("_CloudsLightColor", new Color(cloudIntensity, cloudIntensity, cloudIntensity));
        sky.SetColor("_CloudsShadowColor", new Color(cloudIntensity - 0.1f, cloudIntensity - 0.1f, cloudIntensity - 0.1f));
        RenderSettings.fogColor = new Color(lightIntensity * 0.8f, lightIntensity * 0.8f, lightIntensity * 0.8f);
    }
}
