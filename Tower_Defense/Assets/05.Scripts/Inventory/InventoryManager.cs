using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using static UnityEditor.Progress;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public event Action OnInventoryChanged;
    public int Count => _items.Count;
    
    private List<Item> _items = new List<Item>();
    [SerializeField] private int _maxCount = 6;
    [SerializeField] private ItemSwapUI swapUI;

    private void Awake()
    {
        Instance = this;
    }

    public Item Add(ItemData data)
    {
        if (IsFull()) {
            //랜덤박스 거나 소모템이면
            if (data.ItemType == ItemType.RandomBox || data.ItemType == ItemType.Consumable) {
                Use(data);
                return null;
            }

            //UI를 열면서, 결과를 받아 처리할 콜백을 넘김
            swapUI.OpenWindow(data, (isConfirmed, targetIndex) =>
            {
                if (isConfirmed) 
                    SwapItem(targetIndex, data);
            });

            return null;
        }

        return ExecuteAdd(data);
    }
    public bool Remove(Item item)
    {
        bool result = _items.Remove(item);

        if (result) {
            foreach (StatModifier stat in item.Data.Modifiers)
                Player.Instance.Stat.AddStat(stat.StatType, -stat.Value);
            OnInventoryChanged?.Invoke();
        }

        return result;
    }    
    public IReadOnlyList<Item> Items => _items;
    public bool Contains(ItemData data) => _items.Any(x => x.Data == data);
    public Item Find(ItemData data) => _items.Find(x => x.Data == data);
    public bool IsFull() => _items.Count >= _maxCount;
    public void Clear()
    {
        _items.Clear();
        OnInventoryChanged?.Invoke();
    }
    public int GetCount(ItemData data) => _items.Count(x => x.Data == data);
    /// <summary> 소비템 + 랜덤상자 사용 (인벤토리 내에서)  </summary>
    public void Use(Item item)
    {
        switch (item.Type) {
            case ItemType.RandomBox: {
                    if (item.root == null)
                        return;

                    ItemData data = item.root.GetRandomItem();
                    if (Remove(item))
                        Add(data);
                }
                break;
            //소비템
            case ItemType.Consumable: {
                    if (item.Data.Effect != null)
                    {
                        item.Data.Effect.Execute();
                        Remove(item);
                    }
                }
                break;
        }
    }
    /// <summary> 인벤토리가 꽉 찼을 때 소비템 + 랜덤상자 사용 함수 </summary>
    public void Use(ItemData data)
    {
        switch (data.ItemType) {
            case ItemType.RandomBox: {
                    if (data.LootTable == null)
                        return;

                    ItemData item = data.LootTable.GetRandomItem();
                    Add(item);
                }
                break;
            //소비템
            case ItemType.Consumable: {


                }
                break;
        }
    }
    /// <summary> 아이템 스왑 </summary>
    private void SwapItem(int index, ItemData newItem)
    {
        Item oldItem = _items[index];
        Remove(oldItem); //기존 아이템 제거
        ExecuteAdd(newItem); //아이템 추가.
    }
    /// <summary> 실제로 인벤토리에 아이템을 집어넣는 내부 전용 함수 </summary>
    private Item ExecuteAdd(ItemData data)
    {
        if (data == null) 
            return null;

        Item item = new Item(data);
        _items.Add(item);

        foreach (StatModifier stat in data.Modifiers)
            Player.Instance.Stat.AddStat(stat.StatType, stat.Value);

        OnInventoryChanged?.Invoke();
        return item;
    }
}
