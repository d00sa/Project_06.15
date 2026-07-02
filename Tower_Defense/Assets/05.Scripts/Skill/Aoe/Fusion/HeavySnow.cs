using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HeavySnow : MonoBehaviour, IPoolable
{
    private SkillLevelStat myStat;

    [Header("폭설 설정")]
    [Tooltip("느려지는 비율 (예: 0.9 = 90% 슬로우)")]
    [Range(0f, 1f)] public float slowPercentage = 0.5f;
    [Tooltip("지속 시간 동안 떨어뜨릴 라이트닝 트랩의 개수")]
    [SerializeField] private int trapCount = 5;
    [Tooltip("라이트닝 트랩 프리팹")]
    [SerializeField] private GameObject lightningTrapPrefab;

    public void Initialize(SkillLevelStat stat)
    {
        myStat = stat;
        StartCoroutine(HeavySnowRoutine());
    }

    public void OnSpawn() { }

    public void OnDespawn()
    {
        StopAllCoroutines();
    }

    private IEnumerator HeavySnowRoutine()
    {
        if (myStat == null)
        {
            yield break;
        }

        ApplyGlobalSlow();

        float dropInterval = myStat.speed > 0 ? myStat.speed / trapCount : 0.5f;
        for (int i = 0; i < trapCount; i++)
        {
            DropTrapOnRandomEnemy();
            yield return new WaitForSeconds(dropInterval);
        }
        ObjectPool.Instance.ReturnObj(gameObject);
    }

    private void ApplyGlobalSlow()
    {
        List<Enemy> allEnemies = ObjectPool.Instance.GetEnemy();
        foreach (var enemy in allEnemies)
        {
            if (enemy.gameObject.activeInHierarchy && !enemy.IsDead)
            {
                enemy.ApplySlow(slowPercentage, myStat.speed);
            }
        }
    }

    private void DropTrapOnRandomEnemy()
    {
        List<Enemy> validEnemies = ObjectPool.Instance.GetEnemy().Where(e => e.gameObject.activeInHierarchy && !e.IsDead).ToList();

        if (validEnemies.Count > 0)
        {
            Enemy target = validEnemies[Random.Range(0, validEnemies.Count)];

            if (lightningTrapPrefab != null)
            {
                GameObject trap = ObjectPool.Instance.GetObj(lightningTrapPrefab.name, target.transform.position, null, true);

                if (trap.TryGetComponent<InstantAoeEffect>(out var trapEffect))
                {
                    trapEffect.Initialize(myStat);
                }
            }
        }
    }
}