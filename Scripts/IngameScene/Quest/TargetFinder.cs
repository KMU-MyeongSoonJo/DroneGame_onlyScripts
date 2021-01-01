using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * 
 * 타겟의 위치를 알려주는 화살표를 제어하는 스크립트
 * 
 * */

public class TargetFinder : MonoBehaviour
{
    public static TargetFinder instance;

    
    public GameObject Pointer;

    private void Awake()
    {
        instance = this;
        Pointer.SetActive(false);
    }


    IEnumerator FindTarget(GameObject target)
    {
        // 드론 하위의 lookTarget obj
        GameObject drone = PlayerController.instance.drone.transform.GetChild(3).gameObject;
        while (target != null)
        {
            drone.transform.LookAt(target.transform);
            Pointer.transform.eulerAngles = drone.transform.eulerAngles;

            yield return null;
        }

        Pointer.SetActive(false);
    }

    public void StartFindTarget(GameObject t)
    {

        Pointer.SetActive(true);
        StartCoroutine("FindTarget", t);
    }
    public void StopFindTarget()
    {
        Pointer.SetActive(false);
        StopCoroutine("FindTarget");
    }
    
}
