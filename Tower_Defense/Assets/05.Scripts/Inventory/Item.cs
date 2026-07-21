using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

public enum ItemRarity
{
    Common, Rare, Epic, Legendary
}

public class Item
{
    public ItemData Data { get; }
    public string Name => Data.ItemName;
    public string Description => Data.Description;
    public Sprite Icon => Data.Icon;
    public ItemRarity Rarity => Data.Rarity;
    //public int Price => Data.Price;
    public List<StatModifier> stats => Data.Modifiers;

    public Item(ItemData data)
    {
        Data = data;
    }
}
