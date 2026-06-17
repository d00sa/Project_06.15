using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fireball 등 발사체 계열 스킬 담당
/// </summary>
public class SkillShooter : SkillBase
{
    private Transform target = null;

    [Header("발사체 계열 스킬 SO 목록")]
    [SerializeField] private List<SkillData> shooterSkills = new List<SkillData>();

    private void Awake()
    {
        // 인스펙터에서 넣은 리스트를 부모(SkillBase)시스템에 전달
        base.skillDataList = shooterSkills;

    }

    private void Start()
    {
        // 옵젝풀 등록
        foreach (var data in skillDataList)
        {
            if (data != null && data.skillPrefab != null)
                ObjectPool.Instance.RegisterPoolElement(data.skillPrefab, 30);
        }
    }

    protected override void Execute(ActiveSkill skill)
    {
        if (skill.data.skillPrefab == null) return;

        if (target == null || !target.gameObject.activeInHierarchy || !target.CompareTag("Enemy"))
            target = FindClosestEnemy();

        if (target == null) return;

        GameObject pjt = ObjectPool.Instance.GetObj(skill.data.skillPrefab.name, transform.position, null, true);
        if (pjt.TryGetComponent<Projectile>(out var projectile))
        {
            projectile.Initialize(target, skill.CurrentStat);
        }
    }

    private Transform FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform closestEnemy = null;

        float minSqrDist = Mathf.Infinity;
        Vector2 myPos = transform.position;

        foreach (GameObject enemy in enemies)
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
}