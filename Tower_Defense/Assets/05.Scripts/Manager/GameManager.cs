using Space;
using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public event Action<int> OnEnemyCountChanged;
    private int _enemyCount;
    public int EnemyCount
    {
        get => _enemyCount;
        set
        {
            if (_enemyCount != value) {
                _enemyCount = value;
                OnEnemyCountChanged?.Invoke(_enemyCount);

                //die
                if (_enemyCount >= Spawner.Instance.CurDifficulty.UnitCount) {
                    Lose = true;
                    ChangeState(GameState.GameSetting);
                }
            }
        }
    }

    public event Action<int> OnTimeChanged;
    [Header("시간 설정 (초 단위)")]
    [SerializeField] private int _maxReadyTime = 5;
    [SerializeField] private int _maxStageTime = 60;

    private int _curTime;
    private float _timeTimer;
    public int CurTime
    {
        get => _curTime;
        private set
        {
            _curTime = value;
            OnTimeChanged?.Invoke(_curTime);
        }
    }

    private GameState _current;
    public bool Win = false;
    public bool Lose = false;

    void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
            Destroy(this.gameObject);
    }

    private void Start()
    {
        ChangeState(GameState.GameSetting);
    }

    private void Update()
    {
        WorkFlow();
    }

    private void WorkFlow()
    {
        switch (_current) {
            case GameState.Idle:
                break;
            case GameState.LoadDifficultData:
                break;
            case GameState.WaitUntilDifficultDataLoaded:
                break;
            case GameState.StartGame:
                break;
            case GameState.LoadData:
                break;
            case GameState.GameSetting: {
                    if (Win)
                        ChangeState(GameState.GameClear);
                    else if (Lose)
                        ChangeState(GameState.GameLose);
                    else
                        ChangeState(GameState.WaitStage);
                }
                break;
            case GameState.WaitStage:
            case GameState.StartStage:
                RunTimer();
                break;
            case GameState.GameClear:
                break;
            case GameState.GameLose:
                break;
            case GameState.WaitForUser:
                break;
        }
    }

    private void RunTimer()
    {
        _timeTimer += Time.deltaTime;

        if (_timeTimer >= 1f) {
            CurTime--;
            _timeTimer -= 1f;

            if (CurTime <= 0) {
                // 시간이 다 되면 다음 상태로 전환
                if (_current == GameState.WaitStage) 
                    ChangeState(GameState.StartStage);

                else if (_current == GameState.StartStage) 
                    ChangeState(GameState.GameSetting);
            }
        }
    }

    public void ChangeState(GameState newState)
    {
        _current = newState; // 상태 변경
        _timeTimer = 0f; //타이머 초기화

        switch (newState) {
            case GameState.GameSetting:
                break;
            case GameState.WaitStage:
                CurTime = _maxReadyTime;
                break;
            case GameState.StartStage:
                Spawner.Instance.SpawnNext();
                CurTime = _maxStageTime;
                break;
            case GameState.GameLose:
                StartCoroutine(Spawner.Instance.IsAllMonsterReturn());
                break;
            default:
                break;
        }
    }
}
