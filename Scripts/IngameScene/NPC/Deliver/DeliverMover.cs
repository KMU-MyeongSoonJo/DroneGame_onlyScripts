using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class DeliverPosition
{
    public Vector3 deliverPos; // 배달부 위치. NPC는 이 위치를 순회하며 위치한다.
    public Vector3 deliverRot;

    public Vector3 destPos; // 배달할 목적지
}

public class DeliverMover : MonoBehaviour
{
    [Header("Data")]
    public List<DeliverPosition> pos;
    public Transform bike;

    [Header("Sensibilities")]
    public bool testChangePosition;
    float waitTerm = 1200f;

    float timer = 0;

    private void Start()
    {
        //ChangePosition();
    }


    private void Update()
    {
       // timer += Time.deltaTime;

        if(timer > waitTerm || testChangePosition == true)
        {
            ChangePosition();
        }
    }

    public void ChangePosition()
    {
        testChangePosition = false;

        // change npc position
        DeliverPosition tmp = pos[0];
        pos.RemoveAt(0);
        pos.Add(tmp);

        transform.position = pos[0].deliverPos;
        transform.eulerAngles = pos[0].deliverRot;

        bike.transform.position = transform.position + transform.TransformVector(1, 0, 0);
        bike.transform.eulerAngles = pos[0].deliverRot + new Vector3(0, -45, 0);

        if (QuestManager.instance != null)
        {
            //QuestManager.instance.deliverDestPos = pos[0].destPos;

            QuestManager.instance.questList["0100"].GetWayPoints()[0].position = pos[0].destPos;
            QuestManager.instance.questList["0100"].GetWayPoints()[0].rotation = pos[0].deliverRot;
        }
    }

}
