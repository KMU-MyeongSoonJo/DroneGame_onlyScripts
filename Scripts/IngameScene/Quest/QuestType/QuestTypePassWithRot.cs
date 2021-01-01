using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestTypePassWithRot : MonoBehaviour
{
    WayPointController w;

    [Header("sensibilities")]
    [HideInInspector]public float limitAngleRange; // 통과 시점 바라보는 방향 제한 범위

    private void Awake()
    {
        w = GetComponent<WayPointController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (w.getIsEnable() && other.CompareTag("Drone"))
        {
            float droneYRot = other.transform.eulerAngles.y;

            float minYRot = transform.eulerAngles.y - (limitAngleRange * 0.5f);
            float maxYRot = transform.eulerAngles.y + (limitAngleRange * 0.5f);
            
            // Enter 시점의 드론이 바라보는 방향 체크
            if ((droneYRot > minYRot && droneYRot < maxYRot)
                || (droneYRot > minYRot + 360 && droneYRot < maxYRot + 360)
                )
            {
                print($"Clear !!");
                print($"dront rot : {droneYRot}");
                print($"wayPoint rot : {transform.eulerAngles.y}");

                w.Clear();
            }
            else
            {
                print($"Fail !!");
                print($"dront rot : {droneYRot}");
                print($"wayPoint rot : {transform.eulerAngles.y}");

                w.Fail();
            }
        }
    }
}
