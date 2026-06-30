using NUnit.Framework;
using System;
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
    [SerializeField] private List<InventorySlot> _slots;

    [Header("[ItemInfos]")]
    [SerializeField] private RectTransform _itemInfoPanel;
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _name;
    [SerializeField] private TMP_Text _type;
    [SerializeField] private TMP_Text _description;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Remove();
        Add();
        HideItemInfo();
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
    }

    private void Remove()
    {
        if (GameManager.Instance != null) {
            GameManager.Instance.OnTimeChanged -= ChangeStageTime;
            GameManager.Instance.OnEnemyCountChanged -= ChangeEnemyCount;
        }
        
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshInventory;
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

    private void RefreshInventory()
    {
        var items = InventoryManager.Instance.Items;

        for (int i = 0; i < _slots.Count; i++) {
            _slots[i].SetItem(i < items.Count ? items[i] : null);
        }
    }

    public void ShowItemInfo(ItemData data, RectTransform slotPos)
    {
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

    public void HideItemInfo()
    {
        _itemInfoPanel.localScale = Vector3.zero;

        _icon.sprite = null;
        _name.text = "";
        _type.text = "";
        _description.text = "";
    }
}
