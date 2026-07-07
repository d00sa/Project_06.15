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
        Instance = this;
    }

    public Item Add(ItemData data)
    {
        if (IsFull())
            return null;

        Item item = new Item(data);
        _items.Add(item);

        foreach(StatModifier stat in data.Modifiers)
            Player.Instance.Stat.AddStat(stat.StatType, stat.Value);

        OnInventoryChanged?.Invoke();
        return item;
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
}
