using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/************************************
* 
* 설명:
*  플레이어가 특정 지역에 진입할 경우 그에 해당하는 BGM을 MusicManager에게 전달하는 스크립트
* 
************************************/

[RequireComponent(typeof(Collider))]
public class LocalMusic : MonoBehaviour
{
    public AudioClip dayMusic; // 낮에 재생되는 음악
    public AudioClip nightMusic; // 밤에 재생되는 음악
    public int priority; // 우선 순위

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            MusicManager.instance.playableLocalMusics.Add(this); // MusicManager의 재생가능한 지역 음악 목록에 추가
            MusicManager.instance.UpdateLocalMusic();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            MusicManager.instance.playableLocalMusics.Remove(this); // MusicManager의 재생가능한 지역 음악 목록에서 제거
            MusicManager.instance.UpdateLocalMusic();
        }
    }
}
