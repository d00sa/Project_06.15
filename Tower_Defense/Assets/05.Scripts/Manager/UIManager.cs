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
    [SerializeField] private List<InventorySlot> _slots;
    [SerializeField] private List<SkillSlot> _skillSlots;

    [Header("[ItemInfos]")]
    [SerializeField] private RectTransform _itemInfoPanel;
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _name;
    [SerializeField] private TMP_Text _type;
    [SerializeField] private TMP_Text _description;

    [Header("[SkillInfos]")]
    [SerializeField] private RectTransform _skillInfoPanel;
    [SerializeField] private Image _sIcon;
    [SerializeField] private TMP_Text _sName;
    [SerializeField] private TMP_Text _sType;
    [SerializeField] private TMP_Text _sDescription;

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
        InventoryManager.Instance.OnInventoryChanged += RefreshInventory;
        Player.Instance.OnSkillLevelChanged += RefreshSkills;
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

        if (Player.Instance != null)
            Player.Instance.OnSkillLevelChanged -= RefreshSkills;
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

    private void ChangeMoney(int value)
    {
        if (value < 0)
            value = 0;

        _money.text = $"Money : {value}";
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

        for (int i = 0; i < _skillSlots.Count; i++) {
            _skillSlots[i].SetSkill(i < skills.Count ? skills[i] : null);
        }
    }

    private IEnumerator Setting()
    {
        //한 프레임 기다렸다가 호출
        yield return null;
        RefreshInventory();
        RefreshSkills("",1);
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
}
