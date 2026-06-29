using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class DropItem
{
    public ItemData Item; //아이템 정보
    public float Weight; //가중치
}

[CreateAssetMenu(menuName = "Item/LootTable")]
public class LootTable : ScriptableObject
{
    public List<DropItem> Items;

    public ItemData GetRandomItem()
    {
        float total = 0f;

        foreach (var item in Items)
            total += item.Weight;

        float random = Random.Range(0f, total);

        foreach (var item in Items) {
            random -= item.Weight;

            if (random <= 0)
                return item.Item;
        }

        return null;
    }
}