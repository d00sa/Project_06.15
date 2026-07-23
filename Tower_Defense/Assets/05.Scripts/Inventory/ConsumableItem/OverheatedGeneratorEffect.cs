using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Item/Consumable/OverheatedGenerator")]
public class OverheatedGeneratorEffect : ConsumableEffect
{
    [Header("발전기 설정")]
    public float BuffDuration = 10f;
    public float BuffValue = 3.0f;
    public float FreezeDuration = 5f;
    public float FreezeValue = -999f;

    public override void Execute()
    {
        if (Player.Instance != null)
        {
            Player.Instance.StartCoroutine(OverheatRoutine());
        }
    }

    private IEnumerator OverheatRoutine()
    {
        Player.Instance.Stat.AddStat(StatType.AttackSpeed, BuffValue);

        yield return new WaitForSeconds(BuffDuration);

        Player.Instance.Stat.AddStat(StatType.AttackSpeed, -BuffValue);
        Player.Instance.Stat.AddStat(StatType.AttackSpeed, FreezeValue);

        yield return new WaitForSeconds(FreezeDuration);

        Player.Instance.Stat.AddStat(StatType.AttackSpeed, -FreezeValue);
    }
}
