using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public event Action OnInventoryChanged;
    public int Count => _items.Count;
    
    private List<Item> _items = new List<Item>();
    [SerializeField] private int _maxCount = 6;

    private void Awake()
    {
        Debug.Log("InventoryManager Awake");
        Instance = this;
    }

    public Item Add(ItemData data)
    {
        if (IsFull())
            return null;

        Item item = new Item(data);
        _items.Add(item);
        StatManager.Instance.AddStat(data.Stat, item.Increase);

        OnInventoryChanged?.Invoke();
        return item;
    }
    public bool Remove(Item item)
    {
        bool result = _items.Remove(item);

        if (result) {
            StatManager.Instance.AddStat(item.Data.Stat, -item.Increase);
            OnInventoryChanged?.Invoke();
        }

        return result;
    }    
    public IReadOnlyList<Item> Items => _items;
    public bool Contains(ItemData data) => _items.Any(x => x.Data == data);
    public Item FindFirst(ItemData data) => _items.FirstOrDefault(x => x.Data == data);
    public List<Item> FindAll(ItemData data) => _items.Where(x => x.Data == data).ToList();
    public bool IsFull() => _items.Count >= _maxCount;
    public void Clear()
    {
        _items.Clear();
        OnInventoryChanged?.Invoke();
    }
    public int GetCount(ItemData data) => _items.Count(x => x.Data == data);
    public void Use(Item item)
    {
        switch (item.Data.ItemType) {
            case ItemType.RandomBox: {
                    ItemData data = item.Data.LootTable.GetRandomItem();
                    if (Remove(item))
                        Add(data);
                }
                break;
        }
    }
}
