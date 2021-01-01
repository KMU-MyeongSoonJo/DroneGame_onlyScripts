using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarricateParticle : MonoBehaviour
{
    private void OnParticleCollision(GameObject other)
    {
        if(other.name == "Drone")
        {
            QuestManager.instance.QuestFail();
            //transform.parent.SetActive(false);
        }
    }
}
