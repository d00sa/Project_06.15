using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

public class StoreManager : MonoBehaviour
{
    public static StoreManager Instance;
    public event Action OnBuyGoods;

    [SerializeField] List<Goods> _goods;

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

    public IReadOnlyList<Goods> Goods => _goods;

    public bool TryBuy(ItemData item)
    {
        if (!CanBuy(item))
            return false;

        GameManager.Instance.Money -= item.Price;
        OnBuyGoods?.Invoke();
        InventoryManager.Instance.Add(item);

        return true;
    }

    public bool CanBuy(ItemData item) => GameManager.Instance.Money >= item.Price;

    public void SetRandomGoods()
    {
        List<ItemData> candidates = new List<ItemData>(ItemDataBase.Instance.ItemDatas);

        foreach (var slot in _goods) {
            if (candidates.Count == 0) {
                slot.SetGoods(null);
                continue;
            }

            ItemData item = GetRandomItem(candidates);
            slot.SetGoods(item);
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

    private int GetWeight(ItemRarity rarity) => _rarityWeight[rarity];

    [Button("Test!")]
    public void Test()
    {
        UIManager.Instance.ShowStore();
    }
}
