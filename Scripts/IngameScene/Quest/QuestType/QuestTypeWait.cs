using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestTypeWait : MonoBehaviour
{
    WayPointController w;
    IEnumerator coroutine;

    [Header("sensibilities")]
    public bool isFailPossible; // 범위 밖으로 나갈 경우 실패.
    public float waitTime; // 머물러야하는 대기시간

    private void Awake()
    {
        w = GetComponent<WayPointController>();

        //coroutine = Timer();
    }


    // 범위 내 진입시 시간 체크 시작
    private void OnTriggerEnter(Collider other)
    {
        if (w.getIsEnable() && other.CompareTag("Drone"))
        {
            coroutine = Timer();
            StartCoroutine(coroutine);
            //w.clear();
        }
    }

    // 시간이 되기 전 범위를 벗어나면 퀘스트 실패
    private void OnTriggerExit(Collider other)
    {
        if (w.getIsEnable() && other.CompareTag("Drone"))
        {
            // 범위 벗어나면 바로 실패하는 경우
            if (isFailPossible)
            {
                QuestManager.instance.QuestFail();
                //w.Fail();
            }
            // 실패 없이 계속 진행하는 경우
            else
            {
                w.Exit();
                StopCoroutine(coroutine);
            }
        }
    }


    IEnumerator Timer()
    {
        GetComponent<WayPointController>().Stay();

        float timer = 0f;
        while(timer < waitTime)
        {
            print($"[TEST] {timer} / {waitTime}");

            timer += Time.deltaTime;
            yield return null;
        }
        w.Clear();
    }
}
