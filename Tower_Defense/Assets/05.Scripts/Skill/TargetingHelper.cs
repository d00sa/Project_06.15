using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TargetingHelper
{
    /// <summary>
    /// 맵에 있는 모든 적 중 가장 우선순위가 높은 적들만 추려내서 반환
    /// (예: 보스가 있으면 보스만 반환, 보스가 없으면 엘리트들만 반환, 엘리트도 없으면 일반 몹 전부 반환)
    /// </summary>
    public static List<Enemy> GetHighestPriorityEnemies()
    {
        List<Enemy> allEnemies = ObjectPool.Instance.GetEnemy();

        if (allEnemies.Count == 0) return null;

        int maxPriority = -1;
        foreach (Enemy enemy in allEnemies)
        {
            if (!enemy.gameObject.activeInHierarchy) continue;

            if ((int)enemy.priority > maxPriority)
            {
                maxPriority = (int)enemy.priority;
            }
        }

        List<Enemy> topPriorityEnemies = new List<Enemy>();
        foreach (Enemy enemy in allEnemies)
        {
            if (!enemy.gameObject.activeInHierarchy) continue;

            if ((int)enemy.priority == maxPriority)
            {
                topPriorityEnemies.Add(enemy);
            }
        }

        return topPriorityEnemies;
    }

    /// <summary>
    /// 살아있고 활성화된 적들 중 exclude에 포함되지 않은 "유효한" 적 목록을 반환.
    /// Curse(저주 대상 재선정), Revolver(조준 대상 목록) 등 "적 몇 명을 골라 뭔가 붙였다 뗀다"는
    /// 패턴을 가진 스킬들이 공용으로 사용.
    /// </summary>
    public static List<Enemy> GetValidTargets(IEnumerable<Enemy> exclude = null)
    {
        List<Enemy> allEnemies = ObjectPool.Instance.GetEnemy();
        HashSet<Enemy> excludeSet = exclude != null ? new HashSet<Enemy>(exclude) : null;

        List<Enemy> validEnemies = new List<Enemy>();
        foreach (var enemy in allEnemies)
        {
            if (!enemy.gameObject.activeInHierarchy || enemy.IsDead) continue;
            if (excludeSet != null && excludeSet.Contains(enemy)) continue;

            validEnemies.Add(enemy);
        }

        return validEnemies;
    }

    /// <summary>
    /// 유효한 적들 중 무작위로 1명 반환. 없으면 null.
    /// </summary>
    public static Enemy GetRandomValidTarget(IEnumerable<Enemy> exclude = null)
    {
        List<Enemy> validEnemies = GetValidTargets(exclude);
        if (validEnemies.Count == 0) return null;

        return validEnemies[Random.Range(0, validEnemies.Count)];
    }

    /// <summary>
    /// 유효한 적들을 무작위로 섞어서 최대 maxCount명까지 반환. (Revolver의 다중 조준용)
    /// </summary>
    public static List<Enemy> GetRandomValidTargets(int maxCount, IEnumerable<Enemy> exclude = null)
    {
        List<Enemy> validEnemies = GetValidTargets(exclude).OrderBy(x => Random.value).ToList();

        if (validEnemies.Count > maxCount)
            validEnemies = validEnemies.GetRange(0, maxCount);

        return validEnemies;
    }
}