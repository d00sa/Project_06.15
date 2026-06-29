using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

public enum ItemType
{
    //장비템, 랜덤박스
    Equipment, RandomBox
}

//아이템 강화 수치가 각각 다를 수도 있으니 Item 클래스로 분리.
public class Item
{
    public ItemData Data;
    public int Upgrade; //강화 수치

    public float Increase
    {
        get
        {
            return Data.Add + Upgrade * 5; //임시값
        }
    }

    public Item(ItemData data)
    {
        Data = data;
        Upgrade = 0;
    }
}
