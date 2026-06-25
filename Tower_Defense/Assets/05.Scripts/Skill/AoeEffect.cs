using System.Collections.Generic;
using UnityEngine;

public class AoeEffect : MonoBehaviour, IPoolable
{
    private SkillLevelStat myStat;
    private float currentDuration;
    private float tickTimer = 0f;

    // 범위 안에 들어와 있는 적들
    private List<Enemy> enemiesInside = new List<Enemy>();

    [Header("장판 설정")]
    [SerializeField] private Animator animator;
    [Tooltip("이 장판이 적을 밀쳐내는 힘 (독 늪은 0, 폭발은 높게 설정)")]
    [SerializeField] private float knockbackPower = 0.2f;

    [Header("상태 이상 (독/화상)")]
    [Tooltip("장판을 벗어난 후에도 데미지가 지속되는 시간 (0이면 없음)")]
    [SerializeField] private float lingerDuration = 3f;

    public void Initialize(SkillLevelStat stat)
    {
        myStat = stat;
        // 스탯의 range 값을 이용해 장판의 실제 크기 바꾸는 로직 아직은 미사용
        //transform.localScale = new Vector3(myStat.range, myStat.range, 1f);
    }

    public void OnSpawn()
    {
        currentDuration = 0f;
        tickTimer = 0f;
        enemiesInside.Clear();

        if (animator != null) animator.SetBool("isDead", false);
    }
    public void OnDespawn()
    {
    }

    private void Update()
    {
        if (myStat == null) return;

        currentDuration += Time.deltaTime;
        tickTimer += Time.deltaTime;

        if (currentDuration >= myStat.speed) 
        {
            if (lingerDuration > 0f)
            {
                foreach (var enemy in enemiesInside)
                {
                    if (enemy != null && enemy.gameObject.activeInHierarchy)
                    {
                        enemy.ApplyDotDamage(myStat.damage, lingerDuration, 1f / myStat.fireRate, knockbackPower);
                    }
                }
            }

            enemiesInside.Clear();
            if (animator != null) animator.SetBool("isDead", true);
            ObjectPool.Instance.ReturnObj(gameObject, 0.8f);
            return;
        }

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
                enemy.TakeDamage(myStat.damage, transform.position, knockbackPower);
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
                
                if (lingerDuration > 0f)
                {
                    enemy.ApplyDotDamage(myStat.damage, lingerDuration, 1f / myStat.fireRate, knockbackPower);
                }
            }
        }
    }
}
