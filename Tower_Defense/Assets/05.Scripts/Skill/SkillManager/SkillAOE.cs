using System.Collections.Generic;
using UnityEngine;

public class SkillAOE : SkillBase
{

    [Header("타겟팅 설정")]
    [Tooltip("스킬 Range가 0 이하일 때 사용할 기본 탐색 반경")]
    [SerializeField] private float defaultSearchRadius = 3f;
    [SerializeField] private LayerMask enemyLayer;

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

        if (skill.data.skillPrefab == null) return;

        Transform target = FindMostCrowdedEnemy(skill.CurrentStat.range);
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

    private Transform FindMostCrowdedEnemy(float skillRange)
    {
        List<Enemy> allEnemies = ObjectPool.Instance.GetEnemy();

        if (allEnemies == null || allEnemies.Count == 0) return null;

        Transform bestTarget = null;
        int highestScore = -1; // 마리수가 아니라 점수로 계산

        float actualRadius = skillRange > 0f ? (skillRange / 2f) : defaultSearchRadius;

        // 모든 적들의 위치를 한 번씩 중심점으로 삼아 확인
        foreach (Enemy centerEnemy in allEnemies)
        {
            if (!centerEnemy.gameObject.activeInHierarchy) continue;

            // 중심점 주변 반경에 있는 모든 적 텀색
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(centerEnemy.transform.position, actualRadius, enemyLayer);

            int currentScore = 0;

            // 반경 내에 들어온 적들의 우선순위에 따라 가중치 점수 플러스
            foreach (Collider2D hit in hitEnemies)
            {
                if (hit.TryGetComponent<Enemy>(out Enemy caughtEnemy))
                {
                    // 일반몹 1점, 엘리트몹 3점, 보스 5점으로 계산
                    if (caughtEnemy.priority == EnemyPriority.Normal) currentScore += 1;
                    else if (caughtEnemy.priority == EnemyPriority.Elite) currentScore += 3;
                    else if (caughtEnemy.priority == EnemyPriority.Boss) currentScore += 10;
                }
            }

            // 가장 점수가 높은 구역을 타겟
            if (currentScore > highestScore)
            {
                highestScore = currentScore;
                bestTarget = centerEnemy.transform;
            }
        }

        return bestTarget;
    }

    protected override float GetInterval(SkillBase.ActiveSkill skill)
    {
        // 총 발사 간격 = (원래 쿨타임) + (장판 지속시간)
        return base.GetInterval(skill) + skill.CurrentStat.speed;
    }

    //private Transform FindClosestEnemy()
    //{
    //    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
    //    Transform closestEnemy = null;

    //    float minSqrDist = Mathf.Infinity;
    //    Vector2 myPos = transform.position;

    //    foreach (GameObject enemy in enemies)
    //    {
    //        Vector2 dirToEnemy = (Vector2)enemy.transform.position - myPos;
    //        float sqrDist = dirToEnemy.sqrMagnitude;

    //        if (sqrDist < minSqrDist)
    //        {
    //            minSqrDist = sqrDist;
    //            closestEnemy = enemy.transform;
    //        }
    //    }

    //    return closestEnemy;
    //}


}
