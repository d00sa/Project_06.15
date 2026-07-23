using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "Item/Consumable/HammerAndAnvil")]
public class HammerAndAnvilEffect : ConsumableEffect
{
    public override void Execute()
    {

        var items = InventoryManager.Instance.Items;
        if (items == null || items.Count == 0) return;

        var equipments = items.Where(x => x.Type == ItemType.Equipment).ToList();
        if (equipments.Count == 0) return;

        ItemRarity lowestRarity = equipments.Min(x => x.Rarity);

        if (lowestRarity >= ItemRarity.Epic)
        {
            Debug.Log("인벤토리에 에픽 등급 이상의 장비만 존재하여 망치와 모루가 효과 없이 소모되었습니다");
            return;
        }

        var targetItems = equipments.Where(x => x.Rarity == lowestRarity).ToList();
        Item targetItem = targetItems[Random.Range(0, targetItems.Count)];

        ItemRarity nextRarity = lowestRarity + 1;

        var nextRarityItems = ItemDataBase.Instance.ItemDatas
            .Where(x => x.ItemType == ItemType.Equipment && x.Rarity == nextRarity)
            .ToList();

        if (nextRarityItems.Count > 0)
        {
            ItemData newItemData = nextRarityItems[Random.Range(0, nextRarityItems.Count)];

            InventoryManager.Instance.Remove(targetItem);
            InventoryManager.Instance.Add(newItemData);

            Debug.Log($"{targetItem.Name} ({lowestRarity}) -> {newItemData.ItemName} ({nextRarity}) 교체 완료!");
        }
    }
}
