using UnityEngine;

public class BlackHoleEffect : MonoBehaviour, ISkillEffect
{
    private SkillLevelStat myStat;
    private StatType damageBonusType;

    private float effectiveDuration;
    private float currentDuration;
    private float tickTimer = 0f;

    [Header("블랙홀 설정")]
    [Tooltip("적을 중심부로 끌어당기는 힘 (속도)")]
    [SerializeField] private float pullSpeed = 3f;
    [Tooltip("블랙홀 범위 (0이면 SkillData의 Range 스탯을 따라감)")]
    [SerializeField] private float pullRadius = 0f;
    [Tooltip("적이 느려지는 비율 (예: 0.9 = 90% 느려짐)")]
    [SerializeField] private float slowPercentage = 0.9f;

    [Header("애니메이션 설정")]
    [SerializeField] private Animator animator;
    [Tooltip("End 애니메이션이 재생되는 시간 (이 시간이 지나면 풀로 반환)")]
    [SerializeField] private float endAnimationDuration = 0.5f;

    private bool isEnding = false;

    public void Initialize(SkillEffectContext ctx)
    {
        myStat = ctx.stat;
        damageBonusType = ctx.damageBonusType;
        effectiveDuration = myStat.speed * (1f + Player.Instance.Stat.GetStat(StatType.AoeDuration));

        currentDuration = 0f;
        tickTimer = 999f;

        // 사거리(Range) 적용
        if (myStat.range > 0f) pullRadius = myStat.range;
        if (pullRadius <= 0f) pullRadius = 3f;

        SoundManager.Instance.PlaySFX("Trap");
    }

    public void OnSpawn()
    {
        isEnding = false;
        currentDuration = 0f;
        tickTimer = 0f;

        if (animator != null)
            animator.SetBool("isDead", false);
    }

    public void OnDespawn() { }

    private void Update()
    {
        if (myStat == null) return;

        if (isEnding) return;

        currentDuration += Time.deltaTime;
        tickTimer += Time.deltaTime;

        // 지속 시간이 다 끝났을 때
        if (currentDuration >= effectiveDuration)
        {
            isEnding = true; // 다시 이 블록에 들어오지 못하게 막음

            if (animator != null)
                animator.SetBool("isDead", true);

            ObjectPool.Instance.ReturnObj(gameObject, endAnimationDuration);
            return;
        }

        HandleGravityAndDamage();
    }

    private void HandleGravityAndDamage()
    {
        float actualFireRate = myStat.fireRate > 0f ? myStat.fireRate : 1f;
        float tickInterval = 1f / actualFireRate;
        bool isTickTime = tickTimer >= tickInterval;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, pullRadius);

        foreach (Collider2D col in hitColliders)
        {
            if (col.CompareTag("Enemy") && col.TryGetComponent<Enemy>(out var enemy))
            {
                if (enemy.IsDead || !enemy.gameObject.activeInHierarchy) continue;

                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance > 0.1f)
                {
                    Vector3 pullDir = (transform.position - enemy.transform.position).normalized;
                    enemy.transform.position += pullDir * pullSpeed * Time.deltaTime;
                }

                enemy.ApplySlow(slowPercentage, 0.2f);

                if (isTickTime)
                {
                    enemy.TakeDamage(myStat.damage + Player.Instance.Stat.GetStat(damageBonusType), transform.position, 0f);
                }
            }
            else if (col.CompareTag("Projectile") || col.TryGetComponent<ReplicatingCell>(out _) || col.TryGetComponent<BouncyBall>(out _))
            {
                if (col.gameObject == this.gameObject) continue;

                float distance = Vector2.Distance(transform.position, col.transform.position);
                if (distance > 0.1f)
                {
                    Vector3 pullDir = (transform.position - col.transform.position).normalized;
                    float gravityMultiplier = Mathf.Lerp(10f, 2f, distance / pullRadius);
                    col.transform.position += pullDir * (pullSpeed * gravityMultiplier) * Time.deltaTime;
                }
            }
        }

        if (isTickTime)
        {
            tickTimer = 0f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0f, 0.5f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, pullRadius);
    }
}