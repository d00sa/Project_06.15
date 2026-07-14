using System.Collections.Generic;
using UnityEngine;

public class AoeEffect : MonoBehaviour, ISkillEffect
{
    private SkillLevelStat myStat;
    private StatType damageBonusType;

    // 지속시간 보정(StatType.AoeDuration)이 적용된 실제 지속시간
    private float effectiveDuration;

    private float currentDuration;
    private float tickTimer = 0f;

    // 범위 안에 들어와 있는 적들
    private List<Enemy> enemiesInside = new List<Enemy>();

    [Header("장판 설정")]
    [SerializeField] private Animator animator;
    [Tooltip("이 장판이 적을 밀쳐내는 힘")]
    [SerializeField] private float knockbackPower = 0.2f;

    [Header("회전 설정")]
    [Tooltip("체크 시 장판이 회전합니다.")]
    [SerializeField] private bool isRotating = false;
    [Tooltip("회전 속도 (양수: 반시계, 음수: 시계 방향)")]
    [SerializeField] private float rotationSpeed = 90f;

    [Header("상태 이상 (독/화상)")]
    [Tooltip("장판을 벗어난 후에도 데미지가 지속되는 시간 (0이면 없음)")]
    [SerializeField] private float lingerDuration = 3f;

    [Header("특수 옵션")]
    [Tooltip("체크 시 데미지 스탯을 무시하고, 맵 전체의 '일반(Normal)' 몹만 999999 데미지로 즉사시킵니다.")]
    [SerializeField] private bool instaKillNormalOnly = false;

    [Header("핵폭탄 (융합) 옵션")]
    [Tooltip("체크 시 일반몹 즉사 + 보스/엘리트 즉발 데미지 후 맵 전체 남은 적에게 방사능(DoT) 부여")]
    [SerializeField] private bool isNukeFusion = false;

    public void Initialize(SkillEffectContext ctx)
    {
        myStat = ctx.stat;
        damageBonusType = ctx.damageBonusType;
        effectiveDuration = myStat.speed * (1f + Player.Instance.Stat.GetStat(StatType.AoeDuration));

        if (isNukeFusion)
        {
            List<Enemy> allEnemies = ObjectPool.Instance.GetEnemy();
            float tickRate = myStat.fireRate > 0f ? 1f / myStat.fireRate : 1f;
            float finalDamage = myStat.damage + Player.Instance.Stat.GetStat(damageBonusType);

            // 리스트에서 삭제될 것을 대비해 역순으로 순회
            for (int i = allEnemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = allEnemies[i];
                if (enemy.gameObject.activeInHierarchy)
                {
                    if (enemy.priority == EnemyPriority.Normal)
                    {
                        // 일반 몹은 즉사
                        enemy.TakeDamage(999999f, transform.position, 0f);
                    }
                    else
                    {
                        // 엘리트나 보스는 핵 데미지 적용
                        enemy.TakeDamage(finalDamage, transform.position, 0f);

                        // 즉발 데미지를 맞고도 살아남았다면 DOT데미지 적용
                        if (!enemy.IsDead)
                        {
                            // effectiveDuration(speed) 시간 동안, tickRate 주기로 도트딜 부여
                            enemy.ApplyDotDamage(finalDamage * 0.5f, effectiveDuration, tickRate, 0f);
                        }
                    }
                }
            }
        }
        else if (instaKillNormalOnly)
        {
            List<Enemy> allEnemies = ObjectPool.Instance.GetEnemy();

            for (int i = allEnemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = allEnemies[i];
                if (enemy.gameObject.activeInHierarchy && enemy.priority == EnemyPriority.Normal)
                {
                    enemy.TakeDamage(999999f, transform.position, 0f);
                }
            }
        }

        SoundManager.Instance.PlaySFX("Trap");
    }

    public void OnSpawn()
    {
        currentDuration = 0f;
        tickTimer = 0f;
        enemiesInside.Clear();

        if (animator != null) animator.SetBool("isDead", false);
    }

    public void OnDespawn() { }

    private void Update()
    {
        if (myStat == null) return;

        currentDuration += Time.deltaTime;
        tickTimer += Time.deltaTime;

        if (currentDuration >= effectiveDuration)
        {
            if (lingerDuration > 0f && !instaKillNormalOnly)
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

    private void EnemyAttack()
    {
        if (instaKillNormalOnly || isNukeFusion) return;

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 즉사기일 때는 콜라이더 판정 무시
        if (instaKillNormalOnly) return;

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
        if (instaKillNormalOnly) return; // 즉사기 무시

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