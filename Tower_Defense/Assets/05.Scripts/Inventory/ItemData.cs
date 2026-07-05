using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

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

    [Header("장비 정보")] //그 외에는 나중에 추가
    public StatType Stat; //타입
    public float Add; //추가치

    [Header("랜덤 뽑기 정보")]
    public LootTable LootTable;
}
