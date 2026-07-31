using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fireball 등 발사체 계열 스킬 담당
/// </summary>
public class SkillShooter : SkillBase
{
    private Transform target = null;

    [SerializeField] private float windUpTime = 1.0f;

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
        {
            target = FindClosestEnemy();
        }

        if (target == null) return;

        if (skill.data.skillName == "Mini Gun")
        {
            StartCoroutine(BurstFireRoutine(skill));
        }
        else
        {
            FireSingleProjectile(skill);
        }
    }

    // 미니건 전용 연사 코루틴
    private IEnumerator BurstFireRoutine(ActiveSkill skill)
    {

        SoundManager.Instance.PlayLoopSFX("MinigunSkillSFXSpin", 0.3f);

        yield return new WaitForSeconds(windUpTime);
        //SoundManager.Instance.StopSound("MinigunSkillSFXSpin");
        SoundManager.Instance.PlayLoopSFX("MinigunSkillSFXShooting", 0.3f);

        float duration = 5f; // 연사 지속 시간, 필요에 따라 조정 가능
        float fireInterval = skill.CurrentStat.fireRate > 0f ? (1f / skill.CurrentStat.fireRate) : 0.1f;
        float timer = 0f;

        while (timer < duration)
        {
            FireSingleProjectile(skill);

            timer += fireInterval;
            yield return new WaitForSeconds(fireInterval);
        }

        // 발사 로직이 끝나면 예열음과 발사음 루프 정지

        SoundManager.Instance.StopSound("MinigunSkillSFXShooting");
        SoundManager.Instance.StopSound("MinigunSkillSFXSpin");
        SoundManager.Instance.PlaySFX("MinigunSkillSFXOverheating", 1.5f);
    }

    // 기존 단발 발사 방식
    private void FireSingleProjectile(ActiveSkill skill)
    {
        if (target == null || !target.gameObject.activeInHierarchy || !target.CompareTag("Enemy"))
            target = FindClosestEnemy();

        if (target == null) return;

        GameObject obj = ObjectPool.Instance.GetObj(skill.data.skillPrefab.name, transform.position, null, true);

        if (obj.TryGetComponent<ISkillEffect>(out var effect))
        {
            effect.Initialize(new SkillEffectContext(skill.CurrentStat, caster: transform, target: target));
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