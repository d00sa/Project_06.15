using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class StatModifier
{
    public StatType StatType;
    public float Value;
}

[CreateAssetMenu(menuName = "Item/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("기본 아이템 정보")]
    public int ID; //이걸로 ItemDataBase에서 가져올 거임.
    public string ItemName;
    public Sprite Icon;
    public string Description;

    [Header("아이템 타입")]
    public ItemType ItemType;

    [Header("상점")]
    public int Price; //가격
    public ItemRarity Rarity; //희귀도

    [Header("장비 정보")]
    public List<StatModifier> Modifiers; //장비가 주는 스탯들

    [Header("랜덤 뽑기 정보")]
    public LootTable LootTable;
}
