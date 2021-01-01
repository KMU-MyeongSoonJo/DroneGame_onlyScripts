using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/************************************
* 
* 설명:
*   밤에만 특정 오브젝트의 Mesh를 활성화 시켜주는 스크립트
* 
************************************/

public class EnableMeshWhenNight : MonoBehaviour
{
    private void Start()
    {
        if (LightManager.instance.timer < 2400) // 낮
        {
            GetComponent<MeshRenderer>().enabled = false; // Mesh 활성화
        }
        else if (LightManager.instance.timer >= 2400) // 밤
        {
            GetComponent<MeshRenderer>().enabled = true; // Mesh 비활성화
        }
    }

    private void Update()
    {
        if (GetComponent<MeshRenderer>().enabled == true && LightManager.instance.timer < 2400) // 켜져 있음 & 낮
            GetComponent<MeshRenderer>().enabled = false; // Mesh 활성화
        else if (GetComponent<MeshRenderer>().enabled == false && LightManager.instance.timer >= 2400) // 꺼져 있음 & 밤
            GetComponent<MeshRenderer>().enabled = true; // Mesh 비활성화

    }
}
