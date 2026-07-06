using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fireball 등 발사체 계열 스킬 담당
/// </summary>
public class SkillShooter : SkillBase
{
    private Transform target = null;

    private void Start()
    {
        // 옵젝풀 등록
        foreach (var data in skillDataList)
        {
            if (data != null && data.skillPrefab != null)
                ObjectPool.Instance.RegisterPoolElement(data.skillPrefab, 50);
        }
    }

    protected override void Execute(ActiveSkill skill)
    {
        if (skill.data.skillPrefab == null) return;

        if (target == null || !target.gameObject.activeInHierarchy || !target.CompareTag("Enemy"))
            target = FindClosestEnemy();

        if (target == null) return;

        GameObject obj = ObjectPool.Instance.GetObj(skill.data.skillPrefab.name, transform.position, null, true);

        // 새로운 발사체 종류가 추가되어도 이 매니저는 건드릴 필요 없음.
        // 프리팹에 ISkillEffect를 구현한 컴포넌트만 붙이면 됨.
        if (obj.TryGetComponent<ISkillEffect>(out var effect))
        {
            effect.Initialize(new SkillEffectContext(skill.CurrentStat, skill.data.damageBonusType, caster: transform, target: target));
        }
        else
        {
            Debug.LogWarning($"[SkillShooter] '{skill.data.skillPrefab.name}' 프리팹에 ISkillEffect 구현체가 없습니다.");
        }
    }

    private Transform FindClosestEnemy()
    {
        List<Enemy> topPriorityEnemies = TargetingHelper.GetHighestPriorityEnemies();
        if (topPriorityEnemies == null || topPriorityEnemies.Count == 0)
            return null;

        Transform closestEnemy = null;
        float minSqrDist = Mathf.Infinity;
        Vector2 myPos = transform.position;

        foreach (Enemy enemy in topPriorityEnemies)
        {
            Vector2 dirToEnemy = (Vector2)enemy.transform.position - myPos;
            float sqrDist = dirToEnemy.sqrMagnitude;

            if (sqrDist < minSqrDist)
            {
                minSqrDist = sqrDist;
                closestEnemy = enemy.transform;
            }
        }

        return closestEnemy;
    }

    protected override void OnSkillRemoved(ActiveSkill skill)
    {
        base.OnSkillRemoved(skill);
    }
}