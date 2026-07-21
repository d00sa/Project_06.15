using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ItemDataBase : MonoBehaviour
{
    public static ItemDataBase Instance;
    [SerializeField] List<ItemData> _datas;
    [SerializeField] private int debugId = 1;
    private void Awake()
    {
        Instance = this;
    }

    public ItemData Find(int id) => _datas.Find(x => x.ID == id);
    public ItemData Find(string name) => _datas.Find(x => x.ItemName == name);
    public IReadOnlyList<ItemData> ItemDatas => _datas;

    [Button("장비장착")]
    public void Test()
    {
        if (Application.isPlaying) {
            Item success = InventoryManager.Instance.Add(Find(debugId));

            if (success is not null)
                Debug.Log($"<color=green>[디버그]</color> '{success.Name}' 획득!");
            else
                Debug.LogWarning($"<color=red>[디버그]</color> 아이템 : {debugId}이 존재하지 않습니다!");
        }
        else 
            Debug.LogWarning("게임이 실행 중일 때만 테스트할 수 있습니다");
    }

    [Button("장비 풀장착")]
    public void Test2()
    {
        if (Application.isPlaying) {
            Item nullItem = InventoryManager.Instance.Add(Find(999));

            if (nullItem is not null)
                Debug.Log($"<color=green>[디버그]</color> '{nullItem.Name}' 획득!");
            else
                Debug.LogWarning($"<color=red>[디버그]</color> 아이템 : 999이 존재하지 않습니다!");


            for (int i = 0; i < 5; i++) {
                Item success = InventoryManager.Instance.Add(Find(i));

                if (success is not null)
                    Debug.Log($"<color=green>[디버그]</color> '{success.Name}' 획득!");
                else
                    Debug.LogWarning($"<color=red>[디버그]</color> 아이템 : {i}이 존재하지 않습니다!");
            }
        }
        else
            Debug.LogWarning("게임이 실행 중일 때만 테스트할 수 있습니다");
    }
}
