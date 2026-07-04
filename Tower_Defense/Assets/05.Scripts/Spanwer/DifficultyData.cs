using System;
using System.Collections.Generic;
using UnityEngine;

public enum Degree
{
    Easy,
    Normal,
    Hard
}

[CreateAssetMenu(fileName = "Difficulty", menuName = "Scriptable Objects/Difficulty")]
public class DifficultyData : ScriptableObject
{
    public Degree Difficulty; //난이도
    public int UnitCount;
    public List<StageData> StageDataList;
}

[Serializable]
public class StageData
{
    public int Stage;   //스테이지 숫자
    public bool bossStage; //보스 스테이지 유무
    public List<SpawnData> SpawnDataList;   //스테이지마다 소환할 몬스터들 저장소
}

[Serializable]
public class SpawnData
{
    public GameObject Prefab;   //소환할 몬스터 오브젝트
    public int Num; //소환할 몬스터 수
    public float SpawnDelay;    //소환 시작 딜레이
    public float Term;  //소환 주기
    public int Exp; //적이 죽으면 주는 경험치
    public int Money; //적이 죽으면 주는 돈
}
