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

    private void Start()
    {
        InventoryManager.Instance.Add(Find(debugId));
    }

    public ItemData Find(int id) => _datas.Find(x => x.ID == id);
    public ItemData Find(string name) => _datas.Find(x => x.ItemName == name);
    public List<ItemData> FindAll(ItemType type) => _datas.FindAll(x => x.ItemType == type);

    [ContextMenu("테스트: 장비 장착")]
    public void Test()
    {
        if (Application.isPlaying) {
            Item success = InventoryManager.Instance.Add(Find(debugId));

            if (success is not null)
                Debug.Log($"<color=green>[디버그]</color> '{success.Data.name}' 획득!");
            else
                Debug.LogWarning($"<color=red>[디버그]</color> '{success.Data.name}'이 존재하지 않습니다!");
        }
        else 
            Debug.LogWarning("게임이 실행 중일 때만 테스트할 수 있습니다");
    }
}
