using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance;

    [SerializeField] List<Reward> _rewards;
    [SerializeField] private float _globalDropRate = 10f;
    [SerializeField] float _chestDropRate = 5f;
    [SerializeField] float _hourglassDropRate = 5f;
    [SerializeField] float _DiceDropRate = 10f;

    [Header("[리롤 시스템]")]
    [SerializeField] private int _rerollCount = 0; // 기본 리롤 횟수 0

    public int RerollCount
    {
        get => _rerollCount;
        private set
        {
            _rerollCount = value;
            // UI에 리롤 횟수 변경시 이벤트
            OnRerollCountChanged?.Invoke(_rerollCount);
        }
    }
    public event Action<int> OnRerollCountChanged; // 리롤 버튼 텍스트 갱신용 이벤트

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
            DropReward(enemy.transform, "Treasure_Chest");
            return;
        }

        if (Random.Range(0f, 100f) > _globalDropRate * (1f + Player.Instance.Stat.GetStat(StatType.Luck)))
        {
            return;
        }

        float randomValue = Random.Range(0f, _chestDropRate + _DiceDropRate + _hourglassDropRate);

        if (randomValue <= _chestDropRate) {
            DropReward(enemy.transform, "Treasure_Chest");
        }
        else if (randomValue <= _chestDropRate + _hourglassDropRate)
        {
            DropReward(enemy.transform, "Hourglass");
        }
        else if (randomValue <= _chestDropRate + _DiceDropRate)
        {
            DropReward(enemy.transform, "RerollDice");
        }

    }

    public void SelectItem(ItemData item)
    {
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
    // 소모성 아이템으로 리롤 횟수 증가
    public void AddRerollCount(int amount)
    {
        RerollCount += amount;
    }

    public void OnClickRerollButton()
    {
        if (RerollCount > 0)
        {
            RerollCount--; 
            SetRandomRewards();
            // SoundManager.Instance.PlaySFX("Dice_Roll"); // 주사위 굴리는 소리 추가 예정
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

    private void DropReward(Transform transform, string rewardType)
    {
        ObjectPool.Instance.GetObj(rewardType, transform.position);
    }

    /// <summary> 아직은 모르겠음. 스테이지가 진행되면 될 수록 보물상자 드랍률을 올릴지  </summary>
    private void UpdateDropDate(int wave)
    {


    }

    private int GetWeight(ItemRarity rarity)
    {
        float luck = 0f;
        if (Player.Instance != null && Player.Instance.Stat != null)
        {
            luck = Player.Instance.Stat.GetStat(StatType.Luck);
        }
        float weight = _rarityWeight[rarity];

        if (luck > 0)
        {
            // 행운 양수 -> 높은 등급일수록 가중치에 더 큰 보너스
            switch (rarity)
            {
                case ItemRarity.Rare: weight *= (1f + luck * 1.5f); break;
                case ItemRarity.Epic: weight *= (1f + luck * 3.0f); break;
                case ItemRarity.Legendary: weight *= (1f + luck * 5.0f); break;
            }
        }
        else if (luck < 0)
        {
            // 행운 음수 -> Common의 가중치 늘림
            if (rarity == ItemRarity.Common)
            {
                weight *= (1f + Mathf.Abs(luck) * 3.0f);
            }
        }

        return Mathf.RoundToInt(weight);
    }

    [Button("Test!")]
    public void Test()
    {
        UIManager.Instance.ShowRewards();
    }
}
