using System.Collections.Generic;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager _instance;
    public static DifficultyManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = Instantiate(Resources.Load<DifficultyManager>("DifficultyManager"));

            return _instance;
        }
    }

    [SerializeField] List<DifficultyData> _dataList;

    private void Awake()
    {
        if (_instance == this)
            DontDestroyOnLoad(this.gameObject);
        else
            Destroy(gameObject);
    }

    /// <summary> 난이도 에셋 가져오기 </summary>
    /// <param name="idx"> 0: Easy, 1: Medium, 2: Hard </param>
    public DifficultyData GetData(int idx) => _dataList[idx];
}
