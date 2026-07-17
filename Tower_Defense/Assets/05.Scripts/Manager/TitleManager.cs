using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    public static TitleManager Instance;
    [SerializeField] GameObject _title;
    [SerializeField] GameObject _difficulty;
    [SerializeField] List<Button> _buttonList;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.Current != GameState.Idle)
            EnterToGame();
        else
            FirstEnterToGame();
    }

    public void FirstEnterToGame()
    {
        _title.SetActive(true);
        _difficulty.SetActive(false);

        for (int i = 0; i < _buttonList.Count; i++) {
            int idx = i;

            _buttonList[i].onClick.RemoveAllListeners();
            _buttonList[i].onClick.AddListener(() => SelectDifficulty(idx));
        }
    }

    public void EnterToGame()
    {
        _title.SetActive(false);
        _difficulty.SetActive(true);

        for (int i = 0; i < _buttonList.Count; i++) {
            int idx = i;

            _buttonList[i].onClick.RemoveAllListeners();
            _buttonList[i].onClick.AddListener(() => SelectDifficulty(idx));
        }

        GameManager.Instance.ChangeState(GameState.LoadDifficultData);
    }

    public void Setup()
    {
        SetupManager.Instance.Open();
    }

    public void GameQuit()
    {
        GameManager.Instance.GameQuit();
    }

    private void SelectDifficulty(int idx)
    {
        GameManager.Instance.Data = DifficultyManager.Instance.GetData(idx);
    }
}
