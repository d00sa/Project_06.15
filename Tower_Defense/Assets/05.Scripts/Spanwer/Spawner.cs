using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Spawner : MonoBehaviour
{
    public static Spawner Instance;
    public bool IsSummonOk; //소환해도 되는가?
    public DifficultyData CurDifficulty;
    public bool IsBoss; //보스 스테이지인가? -> 보스가 죽으면 끄도록 할거임
    public int CurrentStage => _currentStage;
    public event Action<int> OnWaveChanged;

    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private int _currentStage;
    private StageData curStageData;
    private List<int> _stageSpawnList = new List<int>(); //소환한 스테이지를 저장한 리스트
    private List<int> _counterList = new List<int>();      //각 스테이지마다 소환해야할 몬스터의 수 저장소
    private List<float> _delayTimersList = new List<float>();  //몬스터 소환 타이머 리스트
    private List<float> _termTimersList = new List<float>();    //몬스터 소환 주기 타이머 리스트   
    public bool IsFinished => (CurrentStage >= CurDifficulty.StageDataList.Count && !IsSummonOk); //모든 스테이지 소환 완료
    public bool IsFinal => (CurrentStage >= CurDifficulty.StageDataList.Count);

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }

    private void Start()
    {
        IsSummonOk = false;
        IsBoss = false;
        _currentStage = 0;
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
                        GameObject obj = ObjectPool.Instance.GetObj(
                            id: curStageData.SpawnDataList[i].Prefab.name,
                            spawn: _spawnPoint.position,
                            parent: null,
                            enable: false
                        );

                        obj.GetComponent<Enemy>().Setting(curStageData.SpawnDataList[i].Exp, curStageData.SpawnDataList[i].Hp);
                        obj.SetActive(true);

                        GameManager.Instance.EnemyCount++;

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

    private bool StartSpawn(int stage)
    {
        //소환하려는 스테이지가 유효한지 검사 / 이미 소환 중인지 체크.
        if ((stage < 1 || stage > CurDifficulty.StageDataList.Count) || _stageSpawnList.Contains(stage))
            return false;

        _stageSpawnList.Add(stage);
        curStageData = CurDifficulty.StageDataList[stage - 1];

        if (curStageData.bossStage) 
            IsBoss = true;

        int length = curStageData.SpawnDataList.Count;

        for (int i = 0; i < length; i++) {
            _counterList.Add(curStageData.SpawnDataList[i].Num);
            _delayTimersList.Add(curStageData.SpawnDataList[i].SpawnDelay);
            _termTimersList.Add(curStageData.SpawnDataList[i].Term);
        }

        IsSummonOk = true;
        return true;
    } 

    public bool SpawnNext()
    {
        _currentStage++;
        OnWaveChanged?.Invoke(_currentStage);
        return StartSpawn(_currentStage);
    }

    /// <summary> 만약 플레이어가 죽었다면 모든 몬스터들은 되돌아가야함 </summary>
    public IEnumerator IsAllMonsterReturn()
    {
        //1초 기다림. ( 몬스터가 소환되기를 기다리는 것)
        yield return new WaitForSeconds(1.0f);

        //모든 몬스터들이 다 돌아갔는지 확인
        ObjectPool.Instance.AllObjectReturn();

        IsSummonOk = false;
    }

    /// <summary> 난이도 설정 </summary>
    public void SetDifficulty(DifficultyData data)
    {
        CurDifficulty = data;
        RegisterPoolElements();
    }

    /// <summary> 스테이지에 있는 몬스터들 풀 등록 </summary>
    private void RegisterPoolElements()
    {
        if (CurDifficulty == null)
        {
            Debug.LogError("🚨 [Spawner] 난이도(CurDifficulty) 데이터가 비어있습니다! 인스펙터 연결을 확인하세요.");
            return;
        }

        Debug.Log($"✅ [Spawner] 총 {CurDifficulty.StageDataList.Count}개의 스테이지 풀 등록을 시작합니다...");

        for (int i = 0; i < CurDifficulty.StageDataList.Count; i++)
        {
            for (int j = 0; j < CurDifficulty.StageDataList[i].SpawnDataList.Count; j++)
            {

                GameObject monsterPrefab = CurDifficulty.StageDataList[i].SpawnDataList[j].Prefab;

                if (monsterPrefab != null)
                {
                    ObjectPool.Instance.RegisterPoolElement(
                        monsterPrefab,
                        CurDifficulty.StageDataList[i].SpawnDataList[j].Num
                    );
                    Debug.Log($"➔ [풀 등록 완료] {i + 1}스테이지 몬스터 : {monsterPrefab.name}");
                }
                else
                {
                    Debug.LogWarning($"🚨 [Spawner] {i + 1}스테이지 데이터에 몬스터 프리팹이 빠져있습니다 (None 상태)!");
                }
            }
        }

        Debug.Log("✅ [Spawner] 모든 몬스터 풀 등록 작업이 끝났습니다!");
    }
}