using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QuestType {
    Pass,
    PassWithRot, // 통과시 바라보는 방향까지 체크하는 타입
    Camera,
    Act,
    Wait,
}

public class Location
{
    public QuestType questType;

    public Vector3 position;
    public Vector3 rotation;

    [Header("PassWithRot")]
    public float limitAngleRange; // 바라보는 방향 확인시 오차 범위

    [Header("Act")]
    public GameObject target;
    public bool isEquipPoint;

    [Header("Wait")]
    public bool isFailPossible;
    public float waitTime;
    

    // Quest Type Pass
    public Location(Vector3 _position, Vector3 _rotation, QuestType type = QuestType.Pass)
    {
        questType = QuestType.Pass;

        position = _position;
        rotation = _rotation;
    }

    // Quest Type PassWithRot
    public Location(Vector3 _position, Vector3 _rotation, float _limitAngleRange, QuestType type = QuestType.PassWithRot)
    {
        questType = QuestType.PassWithRot;

        position = _position;
        rotation = _rotation;
        limitAngleRange = _limitAngleRange;
    }

    // Quest Type Find
    public Location(GameObject _target, QuestType type = QuestType.Camera)
    {
        questType = QuestType.Camera;

        target = _target;
    }

    // Quest Type Act
    // _equipment : 장착 혹은 사용할 장비의 이름 name[questId]
    // _isEquipment : 해당 포인트가 장비를 장착하는 포인트인가(장비가 준비되어야 하는 포인트인가)
    public Location(Vector3 _position, GameObject _equipment, bool _isEquipPoint = false, QuestType type = QuestType.Act)
    {
        questType = QuestType.Act;

        position = _position;
        if (_isEquipPoint) { _equipment.transform.position = position; }
        target = _equipment;
        isEquipPoint = _isEquipPoint;
    }

    // Quest Type Wait
    public Location(Vector3 _position, Vector3 _rotation, float _waitTime, bool _isFailPossible, QuestType type = QuestType.Wait)
    {
        questType = QuestType.Wait;

        isFailPossible = _isFailPossible;
        position = _position;
        rotation = _rotation;
        waitTime = _waitTime;
    }
}

public class QuestData
{
    private string subtitle;
    private Location[] wayPoints;
    private int reward;
    private float timeLimit;

    /// <summary>
    /// 퀘스트 정보
    /// </summary>
    /// <param name="_subtitle">부제</param>
    /// <param name="_wayPoints">통과 지점</param>
    /// <param name="_reward">보상</param>
    public QuestData(string _subtitle, Location[] _wayPoints, int _reward, float _timeLimit = -1)
    {
        subtitle = _subtitle;
        wayPoints = _wayPoints;
        reward = _reward;
        timeLimit = _timeLimit;
    }

    public string GetSubtitle() => subtitle;
    public int GetReward() => reward;
    public float GetTimeLimit() => timeLimit;

    public Location[] GetWayPoints() => wayPoints;
}
