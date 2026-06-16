using System;
using System.Collections.Generic;
using UnityEngine;
public class Spawner : MonoBehaviour
{
    public static Spawner Instance;
    public bool IsSummonOk; //소환해도 되는가?
    public DifficultyData CurDifficulty; //현재 난이도(Easy,Medium,Hard) 데이터
    public int CurrentStage => _currentStage;

    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private int _currentStage;
    private StageData curStageData;
    private List<int> _stageSpawnList = new List<int>(); //소환한 스테이지를 저장한 리스트
    private List<int> _counterList = new List<int>();      //각 스테이지마다 소환해야할 몬스터의 수 저장소
    private List<float> _delayTimersList = new List<float>();  //몬스터 소환 타이머 리스트
    private List<float> _termTimersList = new List<float>();    //몬스터 소환 주기 타이머 리스트   

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }

    private void Start()
    {
        RegisterPoolElements();
        SpawnNext();
    }

    private void Update()
    {
        if (!IsSummonOk)
            return;

        bool isAllSpawnFinished = true;
        for (int i = 0; i < curStageData.SpawnDataList.Count; i++) {
            //소환해야하는 것이 있다면
            if (_counterList[i] > 0) {
                isAllSpawnFinished = false;
                //소환 시작 딜레이 타이머가 종료 되었으면
                if (_delayTimersList[i] <= 0) {
                    //소환 주기 타이머가 종료 되었으면
                    if (_termTimersList[i] <= 0) {
                        //소환
                        GameObject enemy = ObjectPool.Instance.GetObj(
                            id: curStageData.SpawnDataList[i].Prefab.name,
                            spawn: _spawnPoint.position
                            );

                        _termTimersList[i] = curStageData.SpawnDataList[i].Term;
                        _counterList[i]--;
                    }
                    else
                        _termTimersList[i] -= Time.deltaTime;
                }
                else
                    _delayTimersList[i] -= Time.deltaTime;
            }
        }

        if (isAllSpawnFinished) {
            IsSummonOk = false; // 더 이상 소환을 돌지 않도록 잠금

            _delayTimersList.Clear();
            _termTimersList.Clear();
            _counterList.Clear();

            Debug.Log("이번 스테이지의 모든 몬스터 소환 완료!");
        }
    }

    public void StartSpawn(int stage)
    {
        //소환하려는 스테이지가 유효한지 검사 / 이미 소환 중인지 체크.
        if ((stage < 1 || stage > CurDifficulty.StageDataList.Count) || _stageSpawnList.Contains(stage))
            return;

        _stageSpawnList.Add(stage);
        curStageData = CurDifficulty.StageDataList[stage - 1];

        int length = curStageData.SpawnDataList.Count;

        for (int i = 0; i < length; i++) {
            _counterList.Add(curStageData.SpawnDataList[i].Num);
            _delayTimersList.Add(curStageData.SpawnDataList[i].SpawnDelay);
            _termTimersList.Add(curStageData.SpawnDataList[i].Term);
        }

        IsSummonOk = true;
    } 

    public void SpawnNext()
    {
        _currentStage++;
        StartSpawn(_currentStage);
    }

    /// <summary> 스테이지에 있는 몬스터들 풀 등록 </summary>
    private void RegisterPoolElements()
    {
        for (int i = 0; i < CurDifficulty.StageDataList.Count; i++) {

            for (int j = 0; j < CurDifficulty.StageDataList[i].SpawnDataList.Count; j++) {

                ObjectPool.Instance.RegisterPoolElement(
                    CurDifficulty.StageDataList[i].SpawnDataList[j].Prefab,
                    CurDifficulty.StageDataList[i].SpawnDataList[j].Num
                );
            }
        }
    }
}