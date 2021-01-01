using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestTypeAct : MonoBehaviour
{
    WayPointController w;
    private void Awake()
    {
        w = GetComponent<WayPointController>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (w.getIsEnable() && other.CompareTag("Drone"))
        {
            // 장비 장착 / 사용
            //if (Input.GetKeyDown(KeyCode.Space))
            if(InputDeviceChecker.instance.Interact())
            {
                // 사용(장착 해제)
                if (PlayerController.instance.drone.transform.Find("EquipArea").childCount > 0)
                {
                    GameObject equip = PlayerController.instance.drone.transform.Find("EquipArea").GetChild(0).gameObject;
                    //transform.parent.GetComponent<Rigidbody>().isKinematic = false;
                    equip.GetComponent<Rigidbody>().isKinematic = false;
                    equip.transform.parent = null;
                }

                // 장착
                else
                {
                    transform.parent.GetComponent<Rigidbody>().isKinematic = true;
                    transform.parent.transform.position = PlayerController.instance.drone.transform.Find("EquipArea").position;
                    transform.parent.parent = PlayerController.instance.drone.transform.Find("EquipArea");
                }

                w.Clear();
            }

            //w.clear();
        }
    }
    
}
