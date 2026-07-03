using System.Collections.Generic;
using UnityEngine;

public class SkillAOE : SkillBase
{

    [Header("타겟팅 설정")]
    [Tooltip("스킬 Range가 0 이하일 때 사용할 기본 탐색 반경")]
    [SerializeField] private float defaultSearchRadius = 3f;
    [SerializeField] private LayerMask enemyLayer;

    // 리볼버뿐 아니라 앞으로 생길 모든 "지속형" AOE 스킬이 여기에 등록됨
    private Dictionary<string, IPersistentSkillEffect> activePersistentEffects = new Dictionary<string, IPersistentSkillEffect>();

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

        // 지속형 이펙트(리볼버류)는 최초 1회만 스폰하고, 이후엔 OnLevelUp에서 갱신만 함
        if (skill.data.skillPrefab.TryGetComponent<IPersistentSkillEffect>(out _))
        {
            if (!activePersistentEffects.ContainsKey(skill.data.skillName))
            {
                GameObject obj = ObjectPool.Instance.GetObj(skill.data.skillPrefab.name, Vector3.zero, null, true);
                var persistent = obj.GetComponent<IPersistentSkillEffect>();
                persistent.Initialize(skill.CurrentStat);

                activePersistentEffects.Add(skill.data.skillName, persistent);
            }
            return;
        }

        // 나머지는 전부 일회성 이펙트
        Transform target = FindMostCrowdedEnemy(skill.CurrentStat.range);
        if (target == null) return;

        GameObject aoe = ObjectPool.Instance.GetObj(skill.data.skillPrefab.name, target.position, null, true);

        if (aoe.TryGetComponent<ISkillEffect>(out var effect))
        {
            effect.Initialize(new SkillEffectContext(skill.CurrentStat, caster: transform, target: target));
        }
        else
        {
            Debug.LogWarning($"[SkillAOE] '{skill.data.skillPrefab.name}' 프리팹에 ISkillEffect/IPersistentSkillEffect 구현체가 없습니다.");
        }
    }

    protected override void OnLevelUp(ActiveSkill skill)
    {
        base.OnLevelUp(skill);

        if (activePersistentEffects.TryGetValue(skill.data.skillName, out var persistent))
        {
            persistent.UpgradeEffect(skill.CurrentStat);
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

            // 중심점 주변 반경에 있는 모든 적 탐색
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(centerEnemy.transform.position, actualRadius, enemyLayer);

            int currentScore = 0;

            // 반경 내에 들어온 적들의 우선순위에 따라 가중치 점수 플러스
            foreach (Collider2D hit in hitEnemies)
            {
                if (hit.TryGetComponent<Enemy>(out Enemy caughtEnemy))
                {
                    // 일반몹 1점, 엘리트몹 3점, 보스 10점으로 계산
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

    protected override float GetInterval(ActiveSkill skill)
    {
        // 총 발사 간격 = (원래 쿨타임) + (장판 지속시간)
        return base.GetInterval(skill) + skill.CurrentStat.Duration;
    }

    protected override void OnSkillRemoved(ActiveSkill skill)
    {
        base.OnSkillRemoved(skill);

        if (activePersistentEffects.TryGetValue(skill.data.skillName, out var persistent))
        {
            if (persistent != null)
            {
                persistent.OnDespawn();

                // 무한 루프를 끄고 풀로 반환
                ObjectPool.Instance.ReturnObj(((MonoBehaviour)persistent).gameObject);
            }

            activePersistentEffects.Remove(skill.data.skillName);
        }
    }

}