using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * 
 * 드론 카메라에 촬영될 대상 오브젝트에 적용되는 스크립트
 * 드론에서 발사한 ray가 본 오브젝트에 닿았을 때 클리어된다.
 * 
 * */

public class QuestTypeCamera : MonoBehaviour
{
    WayPointController w;

    private void Awake()
    {
        w = GetComponent<WayPointController>();
    }

    private void FixedUpdate()
    {
        if (w.getIsEnable())
            if (DroneRayController.instance.ShotRay() == gameObject.name)
            {
                w.Clear();
            }

        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (w.getIsEnable() && other.CompareTag("Drone"))
        {
            w.Clear();
        }
    }

}
