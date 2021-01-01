using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CarMover : MonoBehaviour
{
    public float speed;

    public List<Transform> corners;

    Animator m_animator;
    bool isCorneringNow;

    Queue<Transform> nextCorner = new Queue<Transform>();

    // Start is called before the first frame update
    void Start()
    {
        m_animator = GetComponent<Animator>();

        for (int i = 0; i < corners.Count; i++)
            nextCorner.Enqueue(corners[i]);

        transform.LookAt(nextCorner.Peek());
        Vector3 myAngle = transform.eulerAngles;
        myAngle.x = 0; myAngle.z = 0;
        transform.eulerAngles = myAngle;
    }
    

    // Update is called once per frame
    void Update()
    {
        if (isCorneringNow) return;

        // 정면에서 장애물 탐지
        Collider[] c = Physics.OverlapBox(transform.position + transform.forward * 5 + transform.up * 1.2f, new Vector3(2, 1, 1));

        if(c.Length > 0)
        {
            // 탐지됨!! 차량 정지
        }

        // 앞에 사물이 없다면 계속 이동
        else
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }

        // 주행 방향 보정
        transform.LookAt(nextCorner.Peek());
        Vector3 myAngle = transform.eulerAngles;
        myAngle.x = 0; myAngle.z = 0;
        transform.eulerAngles = myAngle;

        // 코너링
        if (Vector3.Distance(transform.position, nextCorner.Peek().position) < 1f)
        {
            Transform t = nextCorner.Dequeue();
            nextCorner.Enqueue(t);

            m_animator.SetTrigger("isCorner");
            
        }
    }

    public void CurnerBegin()
    {
        isCorneringNow = true;
    }

    public void CornerEnd()
    {
        isCorneringNow = false;

        transform.LookAt(nextCorner.Peek());
        Vector3 myAngle = transform.eulerAngles;
        myAngle.x = 0; myAngle.z = 0;
        transform.eulerAngles = myAngle;
    }

}
