using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private TMP_Text _enemyCountText;
    [SerializeField] private TMP_Text _stageTime;
    [SerializeField] private List<InventorySlot> _slots;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        RefreshInventory();
    }

    private void OnEnable()
    {
        Remove();
        Add();
    }
    
    //안해도 되는 데 혹시 모르니   
    private void OnDisable()
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
        GameManager.Instance.OnTimeChanged -= ChangeStageTime;
        GameManager.Instance.OnEnemyCountChanged -= ChangeEnemyCount;
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
}
