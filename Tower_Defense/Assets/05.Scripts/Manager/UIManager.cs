using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("[GameUI - UserInfo]")]
    [SerializeField] private TMP_Text _enemyCountText;
    [SerializeField] private TMP_Text _stageTime;
    [SerializeField] private TMP_Text _exp;
    [SerializeField] private List<TMP_Text> _stats;
    [Header("[GameUI - Func]")]
    [SerializeField] private TMP_Text _acceleration;
    [SerializeField] private Image _pauseButton;
    [SerializeField] private List<Sprite> _pauseStart;
    [Header("[GameUI - Items]")]
    [SerializeField] private List<InventorySlot> _slots;
    [Header("[GameUI - Skills]")]
    public Transform SkillPanel;
    public List<SkillSlot> SkillSlots = new();

    [Header("[ItemPanels]")]
    [SerializeField] private RectTransform _itemInfoPanel;
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _name;
    [SerializeField] private TMP_Text _type;
    [SerializeField] private TMP_Text _description;

    [Header("[SkillPanels]")]
    [SerializeField] private RectTransform _skillInfoPanel;
    [SerializeField] private Image _sIcon;
    [SerializeField] private TMP_Text _sName;
    [SerializeField] private TMP_Text _sType;
    [SerializeField] private TMP_Text _sDescription;

    [Header("[Stores]")]
    [SerializeField] private RectTransform _rewardsPanel;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Remove();
        Add();
        HideInfo();
        StartCoroutine(Setting());
    }

    private void OnDestroy()
    {
        Remove();
    }

    private void Add()
    {
        GameManager.Instance.OnEnemyCountChanged += ChangeEnemyCount;
        GameManager.Instance.OnTimeChanged += ChangeStageTime;
        InventoryManager.Instance.OnInventoryChanged += RefreshInventory;
        Player.Instance.OnSkillLevelChanged += RefreshSkills;
        Player.Instance.OnExpChanged += ChangeExp;
        Player.Instance.Stat.OnStatChanged += ChangeStat;
    }

    private void Remove()
    {
        if (GameManager.Instance != null) {
            GameManager.Instance.OnTimeChanged -= ChangeStageTime;
            GameManager.Instance.OnEnemyCountChanged -= ChangeEnemyCount;
        }

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshInventory;

        if (Player.Instance != null) {
            Player.Instance.OnSkillLevelChanged -= RefreshSkills;
            Player.Instance.OnExpChanged -= ChangeExp;
            Player.Instance.Stat.OnStatChanged -= ChangeStat;
        }
    }

    private void ChangeEnemyCount(int count, int deadLine)
    {
        if (count < 0)
            count = 0;

        _enemyCountText.text = $"{count:D2} / {deadLine:D2}";
    }

    private void ChangeStageTime(int time, int stage)
    {
        int minute = time / 60;
        int second = time % 60;

        if (GameManager.Instance.Current == GameState.WaitStage)
            _stageTime.text = $"Next Stage\n{minute:00} : {second:00}";
        else if (GameManager.Instance.Current == GameState.StartStage)
            _stageTime.text = $"Stage {stage}\n{minute:00} : {second:00}";
    }

    private void ChangeExp(int curExp, int maxExp)
    {
        _exp.text = $"{curExp} / {maxExp}";
    }

    private void ChangeStat()
    {
        foreach (StatType stat in Enum.GetValues(typeof(StatType)))
        {
            switch (stat)
            {
                // 숫자만 출력
                case StatType.AttackDamage:
                case StatType.AttackSpeed:
                    _stats[(int)stat].text = $"{Player.Instance.Stat.GetStat(stat)}";
                    break;

                // %붙여서 출력
                case StatType.EXPGained:
                case StatType.CritChance:
                case StatType.CritDamageMultiplier:
                    _stats[(int)stat].text = $"{Player.Instance.Stat.GetStat(stat)}%";
                    break;
            }
        }
    }

    private void RefreshInventory()
    {
        var items = InventoryManager.Instance.Items;

        for (int i = 0; i < _slots.Count; i++) {
            _slots[i].SetItem(i < items.Count ? items[i] : null);
        }
    }

    private void RefreshSkills(string name, int level)
    {
        var skills = Player.Instance.GetCurrentSkill();

        for (int i = 0; i < SkillSlots.Count; i++) {
            SkillSlots[i].SetSkill(i < skills.Count ? skills[i] : null);
        }
    }

    private IEnumerator Setting()
    {
        //한 프레임 기다렸다가 호출
        yield return null;
        RefreshInventory();
        RefreshSkills("",1);
        ChangeStat();
        ChangeEnemyCount(0, GameManager.Instance.Data.UnitCount);
        Player.Instance.AddExp(0);
    }

    public void ShowItemInfo(ItemData data, RectTransform slotPos)
    {
        if (_itemInfoPanel.localScale == Vector3.one) 
            return;

        _itemInfoPanel.localScale = Vector3.one;

        Vector3[] corners = new Vector3[4];
        slotPos.GetWorldCorners(corners);
        _itemInfoPanel.position = corners[1] +
                                  Vector3.left * _itemInfoPanel.rect.width / 1.9f +
                                  Vector3.up * _itemInfoPanel.rect.height / 1.9f;

        _icon.sprite = data.Icon;
        _name.text = data.ItemName;
        _type.text = data.ItemType.ToString();
        _description.text = data.Description;
        //아마 스텟 설명도 들어갈 듯.
    }

    public void ShowSkillInfo(ActiveSkill data, RectTransform slotPos)
    {
        if (_skillInfoPanel.localScale == Vector3.one)
            return;

        _skillInfoPanel.localScale = Vector3.one;

        Vector3[] corners = new Vector3[4];
        slotPos.GetWorldCorners(corners);
        _skillInfoPanel.position = Vector3.right * (corners[1].x + corners[2].x) * 0.5f +
                                  Vector3.up * (corners[1].y + _skillInfoPanel.rect.height / 1.8f);

        _sIcon.sprite = data.data.icon;
        _sName.text = data.data.skillName;
        _sType.text = data.data.GetType().ToString();
        _sDescription.text = data.data.description + '\n' + data.level.ToString();
    }

    public void ShowRewards()
    {
        Time.timeScale = 0f;
        _rewardsPanel.localScale = Vector3.one;

        RewardManager.Instance.SetRandomRewards();
    }

    public void HideRewards()
    {
        _rewardsPanel.localScale = Vector3.zero;
        Time.timeScale = GameManager.Instance.CurSpeed;
    }

    public void HideInfo()
    {
        _itemInfoPanel.localScale = Vector3.zero;

        _icon.sprite = null;
        _name.text = "";
        _type.text = "";
        _description.text = "";

        _skillInfoPanel.localScale = Vector3.zero;

        _sIcon.sprite = null;
        _sName.text = "";
        _sType.text = "";
        _sDescription.text = "";
    }

    public void SetUp()
    {
        SetupManager.Instance.Open();
    }

    public void Pause()
    {
        if (Time.timeScale > 0f) {
            _pauseButton.sprite = _pauseStart[1];
            Time.timeScale = 0f;
        }
        else if (Time.timeScale < 0.1f) {
            _pauseButton.sprite = _pauseStart[0];
            Time.timeScale = GameManager.Instance.CurSpeed;
        }
    }

    public void Acceleration()
    {
        float speed = GameManager.Instance.SpeedUp();
        _acceleration.text = $"{speed:F1}x";
    }
}
