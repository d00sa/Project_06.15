using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

public enum ItemRarity
{
    Common = 1, Rare = 2, Epic = 3, Legendary = 4
}

public enum ItemType
{
    //장비템, 랜덤박스 ,소모템
    Equipment, RandomBox, Consumable
}

public class Item
{
    public ItemData Data { get; }
    public string Name => Data.ItemName;
    public string Description => Data.Description;
    public ItemType Type => Data.ItemType;
    public Sprite Icon => Data.Icon;
    public ItemRarity Rarity => Data.Rarity;
    public List<StatModifier> stats => Data.Modifiers;
    public LootTable root => Data.LootTable;

    public Item(ItemData data)
    {
        Data = data;
    }
}
