using NaughtyAttributes;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum GameState
{
    Idle,
    LoadDifficultData,
    StartGame,
    WaitUntilStartGame,
    LoadData,
    GameJudge,
    WaitStage, //스테이지 대기
    StartStage, //스테이지 진행
    GameClear, //게임 클리어
    GameLose, //게임패배
    WaitForUser //유저 대기
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private readonly float[] _speedTable = { 1f, 1.5f, 2f };
    private int _speedIndex = 0;
    public event Action<int,int> OnEnemyCountChanged;
    private int _enemyCount;
    public int EnemyCount
    {
        get => _enemyCount;
        set
        {
            if (_enemyCount != value) {
                _enemyCount = value;
                OnEnemyCountChanged?.Invoke(_enemyCount, Data.UnitCount);

                //die
                if (_enemyCount >= Spawner.Instance.CurDifficulty.UnitCount) {
                    Lose = true;
                    ChangeState(GameState.GameJudge);
                }
            }
        }
    }

    private int _money;

    public event Action<int,int> OnTimeChanged;
    [Header("[시간 설정 (초 단위)]")]
    [SerializeField] private int _maxReadyTime = 5;
    [SerializeField] private int _maxStageTime = 60;
    [SerializeField] private int _maxBossTime = 30;

    private int _curTime;
    private float _timeTimer;
    public int CurTime
    {
        get => _curTime;
        private set
        {
            _curTime = value;
            OnTimeChanged?.Invoke(_curTime, Spawner.Instance.CurrentStage);
        }
    }

    [Header("[게임 설정]")]
    public GameState Current;
    public DifficultyData Data;
    public bool Win = false;
    public bool Lose = false;
    public float CurSpeed = 1.0f;

    public bool IsTimeStopped = false;

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
        ChangeState(GameState.Idle);
    }

    private void Update()
    {
        WorkFlow();
    }

    private void WorkFlow()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame) {
            if (!SetupManager.Instance.Open())
                SetupManager.Instance.Closed();
        }

        switch (Current) {
            case GameState.LoadDifficultData: {
                    if (Data != null)
                        ChangeState(GameState.StartGame);
                }
                break;
            case GameState.StartGame: {
                    LoadSceneManager.Instance.LoadScene("GameStart");
                    ChangeState(GameState.WaitUntilStartGame);
                }
                break;
            case GameState.WaitUntilStartGame: {
                    if (SceneManager.GetActiveScene().name == "GameStart")
                        ChangeState(GameState.LoadData);
                }
                break;
            case GameState.LoadData: {
                    if (Spawner.Instance != null) {
                        Spawner.Instance.SetDifficulty(Data);
                        ChangeState(GameState.GameJudge);
                    }
                }
                break;
            case GameState.GameJudge: {
                    if (Win)
                        ChangeState(GameState.GameClear);
                    else if (Lose)
                        ChangeState(GameState.GameLose);
                    else
                        ChangeState(GameState.WaitStage);
                }
                break;
            case GameState.WaitStage:
            case GameState.StartStage: {

                    if (!IsTimeStopped)
                    {
                        _timeTimer += Time.deltaTime;

                        if (_timeTimer >= 1f)
                        {
                            CurTime--;
                            _timeTimer -= 1f;

                            if (CurTime <= 0)
                            {
                                // 시간이 다 되면 다음 상태로 전환
                                if (Current == GameState.WaitStage)
                                    ChangeState(GameState.StartStage);

                                else if (Current == GameState.StartStage)
                                    ChangeState(GameState.GameJudge);
                            }
                        }
                    }
                }
                break;
            case GameState.GameClear:
                break;
            case GameState.GameLose: 
                break;
            case GameState.WaitForUser:
                break;
        }
    }

    public void ChangeState(GameState newState)
    {
        Current = newState; // 상태 변경
        _timeTimer = 0f; //타이머 초기화

        switch (newState) {
            case GameState.Idle:
                SoundManager.Instance.PlayBGM("BGM");
                break;
            case GameState.LoadDifficultData:
                    Data = null;
                break;
            case GameState.GameJudge:
            {
                    //마지막 스테이지까지 소환이 다 되었고 적 유닛이 더 이상 존재하지 않는다면
                    if (Spawner.Instance.IsFinished && EnemyCount <= 0)
                        Win = true;
                    //스테이지가 끝났는데 보스가 살아있다. -> 사망
                    else if (Spawner.Instance.IsBoss)
                        Lose = true;
            }   
                break;
            case GameState.WaitStage:
                CurTime = _maxReadyTime;
                break;
            case GameState.StartStage: {
                    Spawner.Instance.SpawnNext();

                    if (Spawner.Instance.IsBoss)
                        CurTime = _maxBossTime;
                    else
                        CurTime = _maxStageTime;
                }
                break;
            case GameState.GameClear: {
                    Time.timeScale = 0f;
                    Instantiate(Resources.Load<GameClearUI>("Canvas - Victory"));
                }
                break;
            case GameState.GameLose: {
                    Time.timeScale = 0f;
                    Instantiate(Resources.Load<GameLoseUI>("Canvas - Lose"));
                }
                break;
            case GameState.WaitForUser:
                break;
            default:
                break;
        }
    }

    public void GameQuit()
    {
        PlayerPrefs.Save();

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void GoToDifficultySelect()
    {
        ChangeState(GameState.LoadDifficultData);

        if (SceneManager.GetActiveScene().name == "GameStart")
            LoadSceneManager.Instance.LoadScene("Enter");
    }

    public float SpeedUp()
    {
        _speedIndex = (_speedIndex + 1) % _speedTable.Length;
        CurSpeed = _speedTable[_speedIndex];
        Time.timeScale = CurSpeed;

        return CurSpeed;
    }

    [Button("Sample Scene Game Start")]
    private void Testing()
    {
        ChangeState(GameState.GameJudge);
    }

    [Button("Sample Scene Test")]
    private void Testing2()
    {
        Current = GameState.GameClear;
    }
}
