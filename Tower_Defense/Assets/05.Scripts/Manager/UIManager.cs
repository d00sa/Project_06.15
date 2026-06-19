using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private TMP_Text _enemyCountText;
    [SerializeField] private TMP_Text _stageTime;
    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
            Destroy(this.gameObject);
    }

    private void OnEnable()
    {
        GameManager.Instance.OnEnemyCountChanged -= ChangeEnemyCount;
        GameManager.Instance.OnEnemyCountChanged += ChangeEnemyCount;

        GameManager.Instance.OnTimeChanged -= ChangeStageTime;
        GameManager.Instance.OnTimeChanged += ChangeStageTime;
    }
    
    //안해도 되는 데 혹시 모르니
    private void OnDisable()
    {
        GameManager.Instance.OnEnemyCountChanged -= ChangeEnemyCount;
        GameManager.Instance.OnTimeChanged -= ChangeStageTime;
    }


    private void ChangeEnemyCount(int count)
    {
        if (count < 0)
            count = 0;

        _enemyCountText.text = $"Enemy : {count}";
    }

    private void ChangeStageTime(int time)
    {
        int minute = time / 60;
        int second = time % 60;
        _stageTime.text = $"{minute:00} : {second:00}";
    }
}
