using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/************************************
* 
* 설명:
*   시작할 때, 해당 가구를 가구 배열에 추가해주는 스크립트
* 
************************************/

public class Interior : MonoBehaviour
{
    public int sizeX;
    public int sizeY;
    public int sizeZ;

    [HideInInspector] public float placeRangeX; // 해당 가구 설치했을 때 다른 가구를 설치 못하게 하는 범위 (X)
    [HideInInspector] public float placeRangeY; // 해당 가구 설치했을 때 다른 가구를 설치 못하게 하는 범위 (Y)
    [HideInInspector] public float placeRangeZ; // 해당 가구 설치했을 때 다른 가구를 설치 못하게 하는 범위 (Z)

    private void Awake()
    {
        placeRangeX = (sizeX - 1) / 2.0f;
        placeRangeY = (sizeY - 1) / 2.0f;
        placeRangeZ = (sizeZ - 1) / 2.0f;
    }
}
