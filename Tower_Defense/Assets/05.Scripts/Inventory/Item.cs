using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

public enum ItemType
{
    //여기에 이제 추가할 듯 (검,해머,총, 등등) -> 아이템 종류
    //장비템, 랜덤박스
    Equipment, RandomBox
}

public class Item
{
    public ItemData Data;
    public int Upgrade; //강화 수치.. 10렙까지?

    public float Increase
    {
        get
        {
            return Data.Add[Upgrade];
        }
    }

    public Item(ItemData data)
    {
        Data = data;
        Upgrade = 0;
    }
}
