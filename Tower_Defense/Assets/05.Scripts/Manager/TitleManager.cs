using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    public static TitleManager Instance;
    [SerializeField] GameObject _title;
    [SerializeField] GameObject _difficulty;
    [SerializeField] Image _startButton;
    [SerializeField] List<Button> _buttonList;
    [SerializeField] List<Sprite> _panels; //0: Default, 1:Select

    private int _curIdx = -1;
    private TMP_Text _startText;
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.Current != GameState.Idle) {
            EnterToGame();
            SoundManager.Instance.PlayBGM("BGM");
        }
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

        _startText = _startButton.gameObject.GetComponentInChildren<TMP_Text>();
        SettingColor(200f / 255f);

        for (int i = 0; i < _buttonList.Count; i++) {
            int idx = i;

            _buttonList[i].onClick.RemoveAllListeners();
            _buttonList[i].onClick.AddListener(() => SelectDifficulty(idx));
        }

        GameManager.Instance.ChangeState(GameState.LoadDifficultData);
    }

    public void Setup()
    {
        SoundManager.Instance.PlaySFX("ButtonClick");
        SetupManager.Instance.Open();
    }

    public void GameQuit()
    {
        SoundManager.Instance.PlaySFX("ButtonClick");
        GameManager.Instance.GameQuit();
    }

    public void StartGame()
    {
        if (GameManager.Instance.Data != null) {
            SoundManager.Instance.PlaySFX("ButtonClick");
            GameManager.Instance.IsSelectDifficulty = true;
        }
    }

    private void SelectDifficulty(int idx)
    {
        if (idx < 0) 
            return;

        if (idx == _curIdx) {
            GameManager.Instance.Data = null;

            _buttonList[idx].GetComponent<Image>().sprite = _panels[0]; //default

            SettingColor(200f / 255f);

            _curIdx = -1;
            return;
        }


        if (_curIdx >= 0) 
            _buttonList[_curIdx].GetComponent<Image>().sprite = _panels[0]; //이전 거 해제

        _buttonList[idx].GetComponent<Image>().sprite = _panels[1]; //Select
        _curIdx = idx;

        SoundManager.Instance.PlaySFX("ButtonClick");

        SettingColor(1f);
        GameManager.Instance.Data = DifficultyManager.Instance.GetData(idx);
    }

    private void SettingColor(float value)
    {
        Color color2 = _startButton.color;
        color2.a = value;
        _startButton.color = color2;

        Color textColor2 = _startText.color;
        textColor2.a = value;
        _startText.color = textColor2;
    }
}
