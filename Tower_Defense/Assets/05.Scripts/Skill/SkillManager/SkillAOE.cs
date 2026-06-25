using System.Collections.Generic;
using UnityEngine;

public class SkillAOE : SkillBase
{
    [Header("장판 계열 스킬 SO 목록")]
    [SerializeField] private List<SkillData> aoeSkills = new List<SkillData>();
    
    private void Awake()
    {
        base.skillDataList = aoeSkills;
    }

    private void Start()
    {
        foreach (var data in skillDataList)
        {
            if (data != null && data.skillPrefab != null)
                ObjectPool.Instance.RegisterPoolElement(data.skillPrefab, 10);
        }
    }

    protected override void Execute(ActiveSkill skill)
    {
        Debug.Log($"<color=yellow>[AOE 테스트]</color> {skill.data.skillName} 스킬 발동 시도1!");

        if (skill.data.skillPrefab == null) return;

        Debug.Log($"<color=yellow>[AOE 테스트]</color> {skill.data.skillName} 스킬 발동 시도!2");
        Transform target = FindClosestEnemy();
        if (target == null) return;

        GameObject aoe = ObjectPool.Instance.GetObj(skill.data.skillPrefab.name, target.position, null, true);

        if (aoe.TryGetComponent<AoeEffect>(out var effect))
        {
            effect.Initialize(skill.CurrentStat);
            SoundManager.Instance.PlaySFX("Trap");
        }
        else if (aoe.TryGetComponent<InstantAoeEffect>(out var trap))
        {
            trap.Initialize(skill.CurrentStat);
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
