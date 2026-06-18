using System.Collections.Generic;
using UnityEngine;

public class AoeEffect : MonoBehaviour, IPoolable
{
    private SkillLevelStat myStat;
    private float currentDuration;

    private float tickTimer = 0f;

    // 범위 안에 들어와 있는 적들
    private List<Enemy> enemiesInside = new List<Enemy>();

    public void Initialize(SkillLevelStat stat)
    {
        myStat = stat;
    }

    public void OnSpawn()
    {
        currentDuration = 0f;
        tickTimer = 0f;
        enemiesInside.Clear(); // 초기화
    }
    public void OnDespawn()
    {
        // 구현은 해야되는데 아직은 딱히 넣을 코드가 없네 ◝(⁰▿<)
    }

    private void Update()
    {
        

        currentDuration += Time.deltaTime;
        tickTimer += Time.deltaTime;

        if (currentDuration >= myStat.speed) // AOE는 speed를 지속시간으로 사용
        {
            enemiesInside.Clear();
            ObjectPool.Instance.ReturnObj(gameObject);
            return;
        }

        if (myStat == null) return;

        EnemyAttack();

    }

    private void EnemyAttack()
    {
        bool isTickTime = tickTimer >= (1f / myStat.fireRate);
        if (isTickTime) tickTimer = 0f;

        for (int i = enemiesInside.Count - 1; i >= 0; i--)
        {
            // OnTriggerEnter2D에서 추가된 적들 -> enemiesInside
            Enemy enemy = enemiesInside[i];

            if (enemy == null || !enemy.gameObject.activeInHierarchy || !enemy.CompareTag("Enemy"))
            {
                enemiesInside.RemoveAt(i);
                continue;
            }

            if (isTickTime)
            {
                enemy.TakeDamage(myStat.damage, transform.position, 0.2f);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (collision.TryGetComponent<Enemy>(out var enemy))
            {
                if (!enemiesInside.Contains(enemy))
                    enemiesInside.Add(enemy);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (collision.TryGetComponent<Enemy>(out var enemy))
            {
                enemiesInside.Remove(enemy);
            }
        }
    }
}
