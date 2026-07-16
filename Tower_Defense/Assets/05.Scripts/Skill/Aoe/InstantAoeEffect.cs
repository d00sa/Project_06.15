using UnityEngine;

public class InstantAoeEffect : MonoBehaviour, ISkillEffect
{
    // 캐싱해 둘 최종 데미지
    protected float calculatedFinalDamage;

    private SkillLevelStat myStat;

    [Header("번개 트랩 설정")]
    [SerializeField] private Animator animator;
    [Tooltip("적들을 마비시킬 시간 (초)")]
    [SerializeField] private float stunDuration = 2f;
    [Tooltip("적을 탐색할 레이어")]
    [SerializeField] private LayerMask enemyLayer;
    [Header("시각 효과")]
    [SerializeField] private Transform rangeIndicator;
    [Tooltip("위치로부터 미세 조정")]
    [SerializeField] private Vector3 positionOffset = Vector3.zero;

    public void Initialize(SkillEffectContext ctx)
    {
        myStat = ctx.stat;

        calculatedFinalDamage = Player.Instance.Stat.CalculateFinalDamage(
            myStat.damage,
            myStat.coolTime,
            myStat.fireRate
        );

        transform.position += positionOffset;

        if (rangeIndicator != null)
        {
            rangeIndicator.localScale = new Vector3(myStat.range, myStat.range, 1f);
        }

        Explode();
    }

    public void OnSpawn()
    {
        if (animator != null) animator.SetBool("isDead", false);
    }

    public void OnDespawn() { }

    private void Explode()
    {

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, myStat.range / 2f, enemyLayer);

        foreach (Collider2D col in hitEnemies)
        {
            if (col.CompareTag("Enemy") && col.TryGetComponent<Enemy>(out var enemy))
            {
                enemy.TakeDamage(calculatedFinalDamage, transform.position, 0f);
                enemy.ApplyStun(stunDuration);
            }
        }

        if (animator != null) animator.SetBool("isDead", true);

        ObjectPool.Instance.ReturnObj(gameObject, 0.5f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        float radius = (myStat != null && myStat.range > 0) ? myStat.range / 2f : 1.5f;

        Gizmos.DrawWireSphere(transform.position, radius);
    }
}