using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 변수를 잠시 기록할 수 있지만, PlayPrefsData 처럼 항상 존재합니다.
// 마이룸을 통해 꾸미기씬에 들어가고 나올 때, 드론 상점을 통해 꾸미기 씬에 들어가고 나올 때를 구분하기 위함.

public class Recorder : MonoBehaviour
{
    public static Recorder instance;

    public string previousScene;

    private void Awake()
    {
        if (instance != null) // 중복 생성 방지
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this);
    }
}
