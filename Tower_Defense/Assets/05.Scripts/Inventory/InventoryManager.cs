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
    public List<Item> FindAll(ItemType type) => _items.Where(x => x.Type == type).ToList();
    public bool IsFull() => _items.Count >= _maxCount;
    public void Clear()
    {
        _items.Clear();
        OnInventoryChanged?.Invoke();
    }
    public int GetCount(ItemData data) => _items.Count(x => x.Data == data);
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
        Item item = new Item(data);
        _items.Add(item);

        foreach (StatModifier stat in data.Modifiers)
            Player.Instance.Stat.AddStat(stat.StatType, stat.Value);

        OnInventoryChanged?.Invoke();
        return item;
    }
}
