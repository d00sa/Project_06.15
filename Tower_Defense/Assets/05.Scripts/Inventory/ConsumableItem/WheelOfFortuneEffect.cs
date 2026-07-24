using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Consumable/WheelOfFortuneEffect")]

public class WheelOfFortuneEffect : ConsumableEffect
{
    [Header("버프 설정")]
    public float Duration = 10f;

    public override void Execute()
    {
        if (Player.Instance != null)
        {
            Player.Instance.StartCoroutine(DoubleBuffRoutine());
        }
    }

    private IEnumerator DoubleBuffRoutine()
    {
        float currentExpStat = Player.Instance.Stat.GetStat(StatType.EXPGained);
        float currentLuckStat = Player.Instance.Stat.GetStat(StatType.Luck);

        float addedExp = 1f + currentExpStat;
        float addedLuck = 1f + currentLuckStat;

        Player.Instance.Stat.AddStat(StatType.EXPGained, addedExp);
        Player.Instance.Stat.AddStat(StatType.Luck, addedLuck);

        yield return new WaitForSeconds(Duration);

        Player.Instance.Stat.AddStat(StatType.EXPGained, -addedExp);
        Player.Instance.Stat.AddStat(StatType.Luck, -addedLuck);
    }
}
