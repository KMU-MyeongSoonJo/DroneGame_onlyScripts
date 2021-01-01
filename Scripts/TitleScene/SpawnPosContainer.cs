using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * 
 * Spawn Pos를 저장하는 스크립트
 * CityScene의 SpawnPosContainer 에서 동작
 * CityScene이 Load됨과 동시에 Title씬에 Spawn Pos 정보를 전달
 * 
 * */

[System.Serializable]
public class SpawnPos
{
    public string desc; // 해당 스폰 지점 설명
    public Transform transform; // 스폰 위치
}

public class SpawnPosContainer : MonoBehaviour
{
    static public SpawnPosContainer instance;

    //public Transform[] spawnPos;
    public SpawnPos[] spawnPos;

    public int selectedSpawnPosIdx;

    // 타이틀을 거치며 true. true일때만 플레이어 스폰 진행
    // 스폰되면서 false.
    // 타이틀을 거친 경우와 단순 씬 전환(Myroom에서 나올 떄 등)을 분리하기 위한 플래그
    public bool isSpawnFromTitle;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        selectedSpawnPosIdx = 1;
        SpawnPosSelector.instance.MoveCameraToSpawnPos();
        SpawnPosSelector.instance.DescTitle();
    }


    /// <summary>
    /// 희망 스폰 지점에서 플레이어 시작
    /// </summary>
    public void SpawnPlayer()
    {
        if (isSpawnFromTitle)
        {
            isSpawnFromTitle = false;
            PlayerController.instance.transform.position = spawnPos[selectedSpawnPosIdx].transform.position;
            PlayerController.instance.transform.rotation= spawnPos[selectedSpawnPosIdx].transform.rotation;
        }
                
    }
    
}
