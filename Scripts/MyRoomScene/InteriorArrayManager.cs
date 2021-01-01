using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/************************************
* 
* 설명:
*   플레이어 방의 가구 배열을 관리하는 스크립트
* 
************************************/

public class InteriorArrayManager : MonoBehaviour
{
    public static InteriorArrayManager instance;

    public int arraySizeX; // 배열 크기 (X)
    public int arraySizeY; // 배열 크기 (Y)
    public int arraySizeZ; // 배열 크기 (Z)

    public Interior[,,] interiorArray; // 3차원 가구 배열
    public List<Interior> placedInteriors; // 설치된 인테리어 리스트

    private void Awake()
    {
        instance = this;

        interiorArray = new Interior[arraySizeX,arraySizeY,arraySizeZ];
        placedInteriors = new List<Interior>();
    }

    public void PlaceAtArray(Interior interior) // 배열에 인테리어 넣기
    {
        for (int x = (int)(interior.transform.position.x - interior.placeRangeX); x <= interior.transform.position.x + interior.placeRangeX; x++) // 가구가 차지하는 X범위 만큼
        {
            for (int y = (int)(interior.transform.position.y - interior.placeRangeY); y <= interior.transform.position.y + interior.placeRangeY; y++) // 가구가 차지하는 Y범위 만큼
            {
                for (int z = (int)(interior.transform.position.z - interior.placeRangeZ); z <= interior.transform.position.z + interior.placeRangeZ; z++) // 가구가 차지하는 Z범위 만큼
                {
                    interiorArray[x, y, z] = interior; // 배열에 해당 인테리어를 넣기
                }
            }
        }
        placedInteriors.Add(interior); // 설치된 인테리어 리스트에 해당 인테리어 넣기
    }

    public void UnplaceFromArray(Interior interior) // 배열에서 인테리어 빼기
    {
        for (int x = (int)(interior.transform.position.x - interior.placeRangeX); x <= interior.transform.position.x + interior.placeRangeX; x++) // 가구가 차지하는 X범위 만큼
        {
            for (int y = (int)(interior.transform.position.y - interior.placeRangeY); y <= interior.transform.position.y + interior.placeRangeY; y++) // 가구가 차지하는 Y범위 만큼
            {
                for (int z = (int)(interior.transform.position.z - interior.placeRangeZ); z <= interior.transform.position.z + interior.placeRangeZ; z++) // 가구가 차지하는 Z범위 만큼
                {
                    interiorArray[x, y, z] = null; // 배열에서 해당 인테리어를 빼기
                }
            }
        }
        placedInteriors.Remove(interior); // 설치된 인테리어 리스트에서 해당 인테리어 빼기
    }
}
