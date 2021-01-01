using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestTimeLimiter : MonoBehaviour
{
    [SerializeField] private Text m_timer;
    private float m_leftTime;

    public float TimeLimit
    {
        set { m_leftTime = value; }
    }

    private void Start()
    {
        Debug.Assert(m_timer);
    }

    private void OnEnable()
    {
        m_timer.color = Color.white;
    }

    private void Update()
    {
        m_leftTime -= Time.deltaTime;
        m_timer.text = string.Format("{0:F2}", m_leftTime);

        if (m_leftTime < 5)
            m_timer.color = Color.red;

        if (m_leftTime < 0)
            QuestManager.instance.QuestFail();
    }
}
