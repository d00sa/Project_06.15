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

    [Header("[GameUI]")]
    [SerializeField] private TMP_Text _enemyCountText;
    [SerializeField] private TMP_Text _stageTime;
    [SerializeField] private TMP_Text _money;
    [SerializeField] private TMP_Text _exp;
    [SerializeField] private TMP_Text _acceleration;
    [SerializeField] private Image _pauseButton;
    [SerializeField] private List<Sprite> _pauseStart;
    [SerializeField] private List<TMP_Text> _stats;
    [SerializeField] private List<InventorySlot> _slots;


    [Header("[Skills]")]
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
    [SerializeField] private RectTransform _storePanel;
    [SerializeField] private Button _reRoll;
    [SerializeField] private RectTransform _goodsInfoPanel;
    [SerializeField] private TMP_Text _gName;
    [SerializeField] private TMP_Text _gType;
    [SerializeField] private TMP_Text _gDescription;

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
        GameManager.Instance.OnMoneyChanged += ChangeMoney;
        StoreManager.Instance.OnBuyGoods += RefreshStore;
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
            GameManager.Instance.OnMoneyChanged -= ChangeMoney;
        }

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshInventory;

        if (Player.Instance != null) {
            Player.Instance.OnSkillLevelChanged -= RefreshSkills;
            Player.Instance.OnExpChanged -= ChangeExp;
            Player.Instance.Stat.OnStatChanged -= ChangeStat;
        }

        if (StoreManager.Instance != null)
            StoreManager.Instance.OnBuyGoods -= RefreshStore;
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

    private void ChangeMoney(int value)
    {
        if (value < 0)
            value = 0;

        _money.text = $"{value}";
    }

    private void ChangeStat()
    {
        foreach (StatType stat in Enum.GetValues(typeof(StatType))) {
            switch (stat) {
                case StatType.ProjectileDamage:
                case StatType.AoeDamage:
                case StatType.PetDamage:
                case StatType.AttackSpeed:
                    _stats[(int)stat].text = $"{Player.Instance.Stat.GetStat(stat)}";
                    break;
                case StatType.EXPGained:
                    _stats[(int)stat].text = $"{Player.Instance.Stat.GetStat(stat)}%";
                    break;
                case StatType.AoeDuration:
                case StatType.ProjectileSpeed:
                    _stats[(int)stat].text = $"{Player.Instance.Stat.GetStat(stat)}";
                    break;
                case StatType.CritChance:
                case StatType.CritDamageMultiplier:
                case StatType.MoneyBonus:
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

    private void RefreshStore()
    {
        //상점이 닫혀있다면 return
        if (_storePanel.localScale == Vector3.zero)
            return;

        foreach (var goods in StoreManager.Instance.Goods) {
            goods.Refresh();
        }
    }

    private IEnumerator Setting()
    {
        //한 프레임 기다렸다가 호출
        yield return null;
        RefreshInventory();
        RefreshSkills("",1);
        ChangeStat();
        ChangeMoney(0);
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

    public void ShowGoodsInfo(ItemData data, RectTransform slotPos)
    {
        if (_goodsInfoPanel.localScale == Vector3.one)
            return;

        _goodsInfoPanel.localScale = Vector3.one;

        Vector3[] corners = new Vector3[4];
        slotPos.GetWorldCorners(corners);
        _goodsInfoPanel.position = corners[1] +
                                  Vector3.left * _goodsInfoPanel.rect.width / 1.9f +
                                  Vector3.up * _goodsInfoPanel.rect.height / 1.9f;

        _gName.text = data.ItemName;
        _gType.text = data.ItemType.ToString();
        _gDescription.text = data.Description;
        //아마 스텟 설명도 들어갈 듯. (아니면 상품 팔 때 띄우거나)
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

    public void ShowStore()
    {
        Time.timeScale = 0f;
        _storePanel.localScale = Vector3.one;

        _reRoll.interactable = GameManager.Instance.Money >= 100;
        _reRoll.GetComponent<Image>().color = new Color32(255, 255, 255, 255);

        StoreManager.Instance.SetRandomGoods();
        RefreshStore();
    }

    public void HideStore()
    {
        _storePanel.localScale = Vector3.zero;
        Time.timeScale = GameManager.Instance.CurSpeed;
    }

    public void ReRoll()
    {
        _reRoll.interactable = false;
        _reRoll.GetComponent<Image>().color = new Color32(150, 150, 150, 255);

        StoreManager.Instance.SetRandomGoods();
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

        _goodsInfoPanel.localScale = Vector3.zero;

        _gName.text = "";
        _gType.text = "";
        _gDescription.text = "";
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
