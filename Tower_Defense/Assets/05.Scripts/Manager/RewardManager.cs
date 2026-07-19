using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance;

    [SerializeField] List<Reward> _rewards;
    [SerializeField] float _dropRate = 0.05f;

    //웨이브가 올라갈 수록 가중치를 변경.
    private readonly Dictionary<ItemRarity, int> _rarityWeight = new(){
    { ItemRarity.Common, 100 },
    { ItemRarity.Rare, 40 },
    { ItemRarity.Epic, 15 },
    { ItemRarity.Legendary, 5 }};

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Spawner.Instance.OnWaveChanged += UpdateRarityWeight;
    }

    private void OnDestroy()
    {
        Spawner.Instance.OnWaveChanged -= UpdateRarityWeight;
    }

    public IReadOnlyList<Reward> Rewards => _rewards;

    public void TrySpawnReward(Enemy enemy, bool isBoss)
    {
        //무조건 스폰
        if (isBoss) {
            DropReward(enemy.transform);
            return;
        }

        if (Random.value <= _dropRate) {
            DropReward(enemy.transform);
        }     
    }

    public void SelectItem(ItemData item)
    {
        //여기서 이제 아이템 갯수 확인하고 풀이면 교체하는 식으로 ?
        InventoryManager.Instance.Add(item);

        UIManager.Instance.HideRewards();
    }

    public void SetRandomRewards()
    {
        List<ItemData> candidates = new List<ItemData>(ItemDataBase.Instance.ItemDatas);

        foreach (var slot in _rewards) {
            if (candidates.Count == 0) {
                slot.SetRewards(null);
                continue;
            }

            ItemData item = GetRandomItem(candidates);
            slot.SetRewards(item);
            candidates.Remove(item);
        }
    }

    private ItemData GetRandomItem(List<ItemData> candidates)
    {
        int totalWeight = 0;

        foreach (var item in candidates)
            totalWeight += GetWeight(item.Rarity);

        int random = Random.Range(0, totalWeight);

        foreach (var item in candidates) {
            random -= GetWeight(item.Rarity);

            if (random < 0)
                return item;
        }

        return null;
    }

    private void UpdateRarityWeight(int wave)
    {
        if (wave >= 10) {
            _rarityWeight[ItemRarity.Common] = 70;
            _rarityWeight[ItemRarity.Rare] = 60;
            _rarityWeight[ItemRarity.Epic] = 25;
            _rarityWeight[ItemRarity.Legendary] = 10;
        }
        else {
            _rarityWeight[ItemRarity.Common] = 100;
            _rarityWeight[ItemRarity.Rare] = 40;
            _rarityWeight[ItemRarity.Epic] = 15;
            _rarityWeight[ItemRarity.Legendary] = 5;
        }
    }

    private void DropReward(Transform transform)
    {
        ObjectPool.Instance.GetObj("Treasure_Chest", transform.position);
    }

    /// <summary> 아직은 모르겠음. 스테이지가 진행되면 될 수록 보물상자 드랍률을 올릴지  </summary>
    private void UpdateDropDate(int wave)
    {


    }

    private int GetWeight(ItemRarity rarity) => _rarityWeight[rarity];

    [Button("Test!")]
    public void Test()
    {
        UIManager.Instance.ShowRewards();
    }
}
