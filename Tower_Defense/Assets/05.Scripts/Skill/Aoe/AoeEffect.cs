using System.Collections.Generic;
using UnityEngine;

public class AoeEffect : MonoBehaviour, ISkillEffect
{
    protected SkillLevelStat myStat;
    protected StatType damageBonusType;

    // 지속시간 보정(StatType.AoeDuration)이 적용된 실제 지속시간
    protected float effectiveDuration;
    private float currentDuration;
    private float tickTimer = 0f;

    // 범위 안에 들어와 있는 적들
    private List<Enemy> enemiesInside = new List<Enemy>();

    [Header("장판 설정")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected float knockbackPower = 0.2f;

    [Header("회전 설정")]
    [SerializeField] protected bool isRotating = false;
    [SerializeField] protected float rotationSpeed = 90f;

    [Header("상태 이상 (독/화상)")]
    [SerializeField] protected float lingerDuration = 3f;

    public virtual void Initialize(SkillEffectContext ctx)
    {
        myStat = ctx.stat;
        damageBonusType = ctx.damageBonusType;
        effectiveDuration = myStat.speed * (1f + Player.Instance.Stat.GetStat(StatType.AoeDuration));

        SoundManager.Instance.PlaySFX("Trap");
    }

    public virtual void OnSpawn()
    {
        currentDuration = 0f;
        tickTimer = 0f;
        enemiesInside.Clear();

        if (animator != null) animator.SetBool("isDead", false);
    }

    public virtual void OnDespawn() { }

    protected virtual void Update()
    {
        if (myStat == null) return;

        currentDuration += Time.deltaTime;
        tickTimer += Time.deltaTime;

        if (currentDuration >= effectiveDuration)
        {
            if (lingerDuration > 0f)
            {
                foreach (var enemy in enemiesInside)
                {
                    if (enemy != null && enemy.gameObject.activeInHierarchy)
                    {
                        float tickRate = myStat.fireRate > 0f ? 1f / myStat.fireRate : 1f;
                        enemy.ApplyDotDamage(myStat.damage, lingerDuration, tickRate, knockbackPower);
                    }
                }
            }

            enemiesInside.Clear();
            if (animator != null) animator.SetBool("isDead", true);
            ObjectPool.Instance.ReturnObj(gameObject, 0.8f);
            return;
        }

        if (isRotating)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }

        EnemyAttack();
    }

    protected virtual void EnemyAttack()
    {
        bool isTickTime = myStat.fireRate > 0f && tickTimer >= (1f / myStat.fireRate);
        if (isTickTime) tickTimer = 0f;

        for (int i = enemiesInside.Count - 1; i >= 0; i--)
        {
            Enemy enemy = enemiesInside[i];

            if (enemy == null || !enemy.gameObject.activeInHierarchy || !enemy.CompareTag("Enemy"))
            {
                enemiesInside.RemoveAt(i);
                continue;
            }

            if (isTickTime)
            {
                enemy.TakeDamage(myStat.damage + Player.Instance.Stat.GetStat(damageBonusType), transform.position, knockbackPower);
            }
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
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

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (collision.TryGetComponent<Enemy>(out var enemy))
            {
                enemiesInside.Remove(enemy);

                if (lingerDuration > 0f)
                {
                    float tickRate = myStat.fireRate > 0f ? 1f / myStat.fireRate : 1f;
                    enemy.ApplyDotDamage(myStat.damage, lingerDuration, tickRate, knockbackPower);
                }
            }
        }
    }
}