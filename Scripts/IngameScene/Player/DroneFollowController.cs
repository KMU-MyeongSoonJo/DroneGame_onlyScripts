using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/************************************
* 
* 버전: 
* 작성자: 진민준     최초 작성날짜:             최근 수정날짜:       
* 설명:
*   드론이 플레이어를 따라다니게 하는 스크립트.
* 
************************************/

public class DroneFollowController : MonoBehaviour
{
    //public DroneMovement DM;
    
    // Player/dronePos
    public GameObject dronePos;

    // 드론 내부의 빈 오브젝트
    // LookTarget이 미리 특정 방향을 바라보고(LookAt())
    // LookTarget의 y rotate 값만 가져와서 드론을 회전시킨다
    public GameObject lookTarget;

    // 드론 follow 자체 회전 속도
    public float angleSpeed;

    float errorRange = 1f;

    //following flag
    public bool coroutineFlag;
    public bool followFlag;
    bool fixedPosFlag;

    IEnumerator coroutine;

    RaycastHit hit;
    

    private void Awake()
    {
        coroutine = RecallDrone();
    }
    
    private void Start()
    {
        coroutineFlag = false;
        followFlag = true;
        fixedPosFlag = false;
    }

    /**
     * 
     * 1. 바닥과 수평이 되게, 목표 거리만큼 ray를 발사 후 오브젝트가 없을 때까지 고도 상승
     * 2. 타겟 머리 위까지 수평 이동
     * 3. 타겟으로 하강
     * 
     * */

    private void FixedUpdate()
    {
        // follow 중이라면
        if (followFlag)
        {
            // 이동해야할 거리가 조금 멀 때 너무 빨리 움직이지 않게 하기 위한 구문
            // 가깝다면 계속 이동.
            if (Vector3.Distance(transform.position, dronePos.transform.position) < 7)
            {
                gameObject.transform.position = Vector3.Lerp(transform.position, dronePos.transform.position, 0.05f);
            }

            // 멀다면 Recall process 재시작 (플레이어 위치 확인, 고도 상승, 위치 이동, 고도 하강)
            else
            {
                StopRecall();
                StartRecall();
            }

            // #1
            // 드론 플레이어가 바라보는 방향을 따라 회전 

            Vector3 targetVector = PlayerController.instance.transform.eulerAngles;
            float turnAngle = PlayerController.instance.transform.eulerAngles.y - transform.eulerAngles.y;
    
            if (turnAngle < 5 && turnAngle > -5) return;

            if (turnAngle > 180) { targetVector.y -= 360; }
            else if (turnAngle < -180) { targetVector.y += 360; }

            targetVector.x = gameObject.transform.eulerAngles.x;
            targetVector.z = gameObject.transform.eulerAngles.z;

            gameObject.transform.eulerAngles =
                Vector3.Lerp(gameObject.transform.eulerAngles,
                targetVector,
                0.025f);


            PlayerController.instance.dm.SetStartingRotation(); // ??? 필요?

            // 접근 종료 시점은 플레이어 주변의 트리거에 닿았을 떄 호출
        }

    }

    //public void stopRecall() => recallFlag = false;
    public void StopRecall()
    {
        print($"[test] StopRecall is called");

        coroutineFlag = false;
        followFlag = false;
        GetComponent<Rigidbody>().isKinematic = false;

        StopCoroutine(coroutine);
    }
    //public void startRecall() => recallFlag = true;

    public void StartRecall() {

        // 리콜 불가 조건
        if (PlayerController.instance.dm.GetStatus() == DroneStatus.Landing
            || PlayerController.instance.dm.GetStatus() == DroneStatus.Rest)
            return;

        PlayerController.instance.dm.SetStatus(DroneStatus.Recall);

        if (!coroutineFlag)
        {

            Timer(1);

            coroutineFlag = true;
            followFlag = false;
            GetComponent<Rigidbody>().isKinematic = true;

            coroutine = RecallDrone();
            StartCoroutine(coroutine);
        }
    }

    public void StartFixPos(GameObject pos = null)
    {
        StartCoroutine(FixPosition(pos));
    }
    public void StopFixPos()
    {
        fixedPosFlag = false;
    }
    

    IEnumerator Timer(float t)
    {
        yield return new WaitForSeconds(t);
    }


    IEnumerator FixPosition(GameObject fixedPos)
    {

        fixedPosFlag = true;
        while (fixedPosFlag)
        {
            //transform.position = Vector3.Lerp(transform.position, fixedPos, 0.05f);
            //transform.eulerAngles = p``os.transform.eulerAngles;
            

            transform.position = fixedPos.transform.position;
            transform.eulerAngles = fixedPos.transform.eulerAngles;

            yield return null;
        }
    }

    IEnumerator RecallDrone()
    {
        // 잠시 대기하며 기울어진 드론 기체 회복
        DroneMovement DM = PlayerController.instance.drone.GetComponent<DroneMovement>();

        DM.DirectDroneRecoverMoment();
        
        // 드론이 도착할 지점을 바라본다(y축 회전)
        Vector3 targetPos = dronePos.transform.position;
        lookTarget.transform.LookAt(targetPos);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, lookTarget.transform.eulerAngles.y, transform.eulerAngles.z);

        DM.SetStartingRotation(); // ??? 필요?

        Debug.DrawLine(transform.position, dronePos.transform.position, Color.blue);

        //// 상승
        //DM.CallMovementUpDown(true, 0.5f);

        float speed = 10.0f;

        // 드론 -> 목표 방향으로 바닥과 수평이 되도록 ray 발사.
        Vector3 start = transform.position, end = dronePos.transform.position;
        start.y = 0; end.y = 0;
        while (Physics.Raycast(
            transform.position + new Vector3(0, -1, 0),
            //dronePos.transform.position - transform.position
            end - start
            , out hit, Vector3.Distance(transform.position, dronePos.transform.position))
            ||
            transform.position.y < dronePos.transform.position.y
            )
        {

            if (!enabled) StopCoroutine(coroutine);

            gameObject.transform.Translate(Vector3.up * speed * Time.deltaTime);
            
            yield return null;
        }

        lookTarget.transform.LookAt(targetPos);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, lookTarget.transform.eulerAngles.y, transform.eulerAngles.z);

        DM.SetStartingRotation();

        float leftDistance = Vector3.Distance(transform.position,
            new Vector3(targetPos.x, transform.position.y, targetPos.z)
            );

        // 드론이 도착 지점을 찾지 못해 지나쳐서
        // 남은 거리가 다시 멀어지는 경우를 체크하기 위한 변수
        float rightBeforeLeftDist = float.MaxValue;
        
        while (leftDistance > 2f && rightBeforeLeftDist >= leftDistance)
        {

            rightBeforeLeftDist = leftDistance;

            // 남은 거리 계산
            leftDistance = Vector3.Distance(transform.position,
            new Vector3(targetPos.x, transform.position.y, targetPos.z)
            );


            gameObject.transform.Translate(Vector3.forward * speed * Time.deltaTime);


            yield return null;
        }
        

        // 정확한 타겟의 상단으로 위치 고정
        //gameObject.transform.position = new Vector3(targetPos.x, transform.position.y, targetPos.z);

        DM.SetStartingRotation(); // 



        // 하강

        leftDistance = Vector3.Distance(transform.position, targetPos);

        while (transform.position.y > targetPos.y + 3)
        {

            gameObject.transform.Translate(Vector3.down * speed * Time.deltaTime);

            yield return null;
        }

        DM.SetStartingRotation(); // ??? 필요?


        coroutineFlag = false;
        followFlag = true;
    }






    // 플레이어 근처에 근접한 경우 follow 종료 후 stay
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("DronePos"))
        {
            // 빠른 recall 종료
            // follow 시작
            
            StopRecall();
        }
    }

    // 플레이어와 거리가 떨어진 경우 follow 재시작
    private void OnTriggerExit(Collider other)
    {
        if (PlayerController.instance.drone.GetComponent<DroneMovement>().GetStatus() != DroneStatus.Control
            && other.CompareTag("Player"))
        {
            followFlag = true;
        }

    }
}
