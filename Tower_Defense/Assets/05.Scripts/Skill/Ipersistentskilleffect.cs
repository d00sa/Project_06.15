using UnityEngine;

/// <summary>
/// 스킬 습득 시 1회만 스폰되고, 스킬이 남아있는 동안 계속 유지되는 지속형 이펙트.
/// 레벨업 시엔 재스폰이 아니라 UpgradeEffect()로 스탯만 갱신
/// 스킬이 제거될 때 매니저가 명시적으로 OnDespawn + 풀 반환을 호출해줘야 함.
/// (ex: Revolver)
/// </summary>
public interface IPersistentSkillEffect : IPoolable
{
    /// <summary>최초 스폰 시 1회 호출</summary>
    void Initialize(SkillLevelStat stat);

    /// <summary>스킬 레벨업 시 호출. 재시작이 아니라 내부 스탯만 갱신해야 함</summary>
    void UpgradeEffect(SkillLevelStat stat);
}