using NaughtyAttributes;
using Space;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
                    ChangeState(GameState.GameJudge);
                }
            }
        }
    }

    public event Action<int> OnTimeChanged;
    [Header("[시간 설정 (초 단위)]")]
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

    [Header("[게임 설정]")]
    public GameState Current;
    public DifficultyData Data;
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

        ChangeState(GameState.Idle);
    }

    private void Update()
    {
        WorkFlow();
    }

    private void WorkFlow()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame) {
            Time.timeScale = 0f;
            SetupManager.Instance.gameObject.SetActive(!SetupManager.Instance.gameObject.activeSelf);
        }

        switch (Current) {
            case GameState.Idle:
                {
                    //게임시작 창
                    if ((Keyboard.current.anyKey.wasPressedThisFrame && !Keyboard.current.escapeKey.wasPressedThisFrame) || Mouse.current.leftButton.wasPressedThisFrame) {
                        TitleManager.Instance.EnterToGame();
                        ChangeState(GameState.LoadDifficultData);
                    }
                }
                break;
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
                    _timeTimer += Time.deltaTime;

                    if (_timeTimer >= 1f) {
                        CurTime--;
                        _timeTimer -= 1f;

                        if (CurTime <= 0) {
                            // 시간이 다 되면 다음 상태로 전환
                            if (Current == GameState.WaitStage)
                                ChangeState(GameState.StartStage);

                            else if (Current == GameState.StartStage)
                                ChangeState(GameState.GameJudge);
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
            case GameState.LoadDifficultData:
                    Data = null;
                break;
            case GameState.GameJudge:
            {
                    //여기서 승리인지 아니면 패배인지 확인
            }
                break;
            case GameState.WaitStage:
                CurTime = _maxReadyTime;
                break;
            case GameState.StartStage:
                Spawner.Instance.SpawnNext();
                CurTime = _maxStageTime;
                break;
            case GameState.GameClear: {
                    Time.timeScale = 0f;                    
                }
                break;
            case GameState.GameLose: {
                    Time.timeScale = 0f;
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

    [Button("Sample Scene Game Start")]
    private void Testing()
    {
        ChangeState(GameState.GameJudge);
    }
}
