using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestTypePass : MonoBehaviour
{
    WayPointController w;
    private void Awake()
    {
        w = GetComponent<WayPointController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (w.getIsEnable() && other.CompareTag("Drone"))
        {
            w.Clear();
        }
    }
}
