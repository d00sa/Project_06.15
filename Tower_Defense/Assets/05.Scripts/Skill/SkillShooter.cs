using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fireball 등 발사체 계열 스킬 담당
/// </summary>
public class SkillShooter : SkillBase
{
    [Header("발사체 계열 스킬 SO 목록")]
    [SerializeField] private List<SkillData> shooterSkills = new List<SkillData>();

    private void Awake()
    {
        // 인스펙터에서 넣은 리스트를 부모(SkillBase)의 시스템에 전달
        base.skillDataList = shooterSkills;
    }

    protected override void Execute(ActiveSkill skill)
    {
        if (skill.data.skillPrefab == null) return;
        // 공격 로직 구현 예정 (ノ・∀・)ノ
    }


}