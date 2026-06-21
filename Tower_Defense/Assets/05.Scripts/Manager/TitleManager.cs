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
    }

    private void SelectDifficulty(int idx)
    {
        GameManager.Instance.Data = DifficultyManager.Instance.GetData(idx);
    }
}
