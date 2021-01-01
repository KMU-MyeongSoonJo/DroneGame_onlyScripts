using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * 
 * 사진 촬영 퀘스트의 촬영 타겟(고양이)
 * 
 * */

public class PhotographQuestTarget : MonoBehaviour
{
    
    // 플레이어 진입
    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Drone"))
        {
            float dist = Vector3.Distance(transform.position, other.transform.position);

            print($"[TEST] dist between cat and drone is {dist}");

            //if (dist < 15) return; // 너무 근접하면 고양이는 도망

            
            
            
            
            
        }
    }
}
