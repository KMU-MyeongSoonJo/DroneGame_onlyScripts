using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotographRangeTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        //if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
        if(other.CompareTag("Drone"))
        {
            // 사진 촬영 범위 진입 알림
            TalkManager.instance.TalkStart("Data/Talk/Photographer/Photographer_quest_91");
        }
    }
}
