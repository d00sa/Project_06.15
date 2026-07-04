using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
    public static StoreManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void Upgrade(ItemType type)
    {

    }

    public int GetUpgradeCost(ItemType type)
    {
        switch (type) {            
        }

        return 0;
    }
}
