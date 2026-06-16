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
        // 인스펙터에서 넣은 리스트를 부모(SkillBase)시스템에 전달
        base.skillDataList = shooterSkills;
    }

    protected override void Execute(ActiveSkill skill)
    {
        if (skill.data.skillPrefab == null) return;

        Transform target = GetRandomEnemy();
        if (target == null) return;

        GameObject pjt = Instantiate(skill.data.skillPrefab, transform.position, Quaternion.identity);
        
        if (pjt.TryGetComponent<Projectile>(out var projectile))
        {
            projectile.Initialize(target, skill.CurrentStat);
        }
    }

    private Transform GetRandomEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length == 0) return null;

        int randomIndex = Random.Range(0, enemies.Length);

        return enemies[randomIndex].transform;
    }
}