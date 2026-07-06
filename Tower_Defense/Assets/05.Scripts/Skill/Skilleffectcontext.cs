using UnityEngine;

/// <summary>
/// ISkillEffect.Initialize()에 전달되는 정보 묶음.
/// 이펙트 종류에 따라 caster/target이 필요 없으면 null로 넘기면 됨
/// (예: AoeEffect, InstantAoeEffect, HeavySnow는 target/caster를 안 씀).
/// </summary>
public readonly struct SkillEffectContext
{
    /// <summary>현재 스킬 레벨의 스탯</summary>
    public readonly SkillLevelStat stat;

    /// <summary>이 스킬의 데미지가 어떤 StatManager 보정 카테고리를 받는지 (SkillData에서 옴)</summary>
    public readonly StatType damageBonusType;

    /// <summary>이 이펙트를 발사/소환한 주체 (플레이어, 펫 등). 필요 없으면 null.</summary>
    public readonly Transform caster;

    /// <summary>조준 대상. 필요 없으면 null.</summary>
    public readonly Transform target;

    public SkillEffectContext(SkillLevelStat stat, StatType damageBonusType, Transform caster = null, Transform target = null)
    {
        this.stat = stat;
        this.damageBonusType = damageBonusType;
        this.caster = caster;
        this.target = target;
    }
}