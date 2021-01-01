using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/**
 * 
 * QuestTypeCamera 진행시 드론에서 ray를 발사케 하기 위한 스크립트
 * 추후 DroneController(singleton)에 이식
 * 
 * */

public class DroneRayController : MonoBehaviour
{
    public static DroneRayController instance;

    RaycastHit hit;
    RaycastHit[] hits;
    float maxDist = 10f;

    private void Awake()
    {
        instance = this;
    }

    public string ShotRay()
    {

        Debug.DrawRay(transform.position, transform.forward * maxDist, Color.blue);

        //if (Physics.Raycast(transform.position, transform.forward, out hit, maxDist))
        //{
        //    return (hit.collider.name);
        //}

        if (Physics.BoxCast(transform.position, new Vector3(1.5f, 1.5f, 1.5f), transform.forward, out hit, Quaternion.identity, maxDist))
        {
            print($"ray cast {hit.collider.name}");
            return (hit.collider.name);
        }

        return null;
    }
}
   