using System.Collections.Generic;
using UnityEngine;

public static class TargetingHelper
{
    /// <summary>
    /// 맵에 있는 모든 적 중 가장 우선순위가 높은 적들만 추려내서 반환
    /// (예: 보스가 있으면 보스만 반환, 보스가 없으면 엘리트들만 반환, 엘리트도 없으면 일반 몹 전부 반환)
    /// </summary>
    public static List<Enemy> GetHighestPriorityEnemies()
    {
        List<Enemy> allEnemies = Enemy.ActiveEnemies;

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
}
