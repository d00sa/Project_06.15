using UnityEngine;

public class Projectile : MonoBehaviour, ISkillEffect
{
    // 캐싱해 둘 최종 데미지
    protected float calculatedFinalDamage;

    private Transform target;
    private SkillLevelStat myStat;

    private Vector2 currentDir;

    [Header("기본 설정")]
    [Tooltip("피격 시 적이 밀려나는 힘")]
    [SerializeField] private float knockbackPower = 0.1f;

    [Header("상태 이상 옵션 (0이면 발동 안함)")]
    [Tooltip("느려지는 비율 (예: 0.9 = 90% 슬로우)")]
    [Range(0f, 1f)] public float slowPercentage = 0f;
    [Tooltip("슬로우 지속 시간 (초)")]
    public float slowDuration = 0f;
    [Tooltip("기절(스턴) 지속 시간 (초)")]
    public float stunDuration = 0f;

    [Header("특수 이동 옵션")]
    [Tooltip("체크 시 타겟을 무시하고 랜덤한 방향으로 직선 비행합니다")]
    public bool isRandomDirection = false;

    [Header("이미지 회전 보정값")]
    [SerializeField] private float rotationOffset = -45f; // 보통 45나 -45? 이미지 따라 알잘딱

    [Header("타겟 재탐색 설정")]
    [SerializeField] private float searchRadius = 3f; // 탐색 반경
    [SerializeField] private LayerMask enemyLayer; // 인스펙터에서 Enemy 레이어 선택

    public void Initialize(SkillEffectContext ctx)
    {
        target = ctx.target;
        myStat = ctx.stat;

        calculatedFinalDamage = Player.Instance.Stat.CalculateFinalDamage(
            myStat.damage,
            myStat.coolTime,
            myStat.fireRate
        );

        if (isRandomDirection)
        {
            currentDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            UpdateRotation();
        }
        else if (target != null)
        {
            currentDir = ((Vector2)target.position - (Vector2)transform.position).normalized;
            UpdateRotation();
        }

        SoundManager.Instance.PlaySFX("Arrow");
    }

    public void OnSpawn()
    {

    }

    public void OnDespawn()
    {
        currentDir = Vector2.zero;
    }

    void Update()
    {
        if (myStat == null) return;

        if (!isRandomDirection)
        {
            if (target != null && target.gameObject.activeInHierarchy && target.CompareTag("Enemy"))
            {
                currentDir = ((Vector2)target.position - (Vector2)transform.position).normalized;
                UpdateRotation();
            }
            else
            {
                target = FindClosestEnemy();
                if (target == null) ObjectPool.Instance.ReturnObj(gameObject);
            }
        }

        transform.Translate(currentDir * myStat.speed * Time.deltaTime, UnityEngine.Space.World);

    }

    private void UpdateRotation()
    {
        float angle = Mathf.Atan2(currentDir.y, currentDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle + rotationOffset, Vector3.forward);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && collision.TryGetComponent<Enemy>(out var enemy))
        {
            // 데미지 및 넉백
            enemy.TakeDamage(calculatedFinalDamage, transform.position, knockbackPower);

            // 상태 이상
            if (slowPercentage > 0f && slowDuration > 0f)
            {
                enemy.ApplySlow(slowPercentage, slowDuration);
            }
            if (stunDuration > 0f)
            {
                enemy.ApplyStun(stunDuration);
            }

            ObjectPool.Instance.ReturnObj(gameObject);
        }
    }

    // 최적화된 근거리 탐색 로직
    private Transform FindClosestEnemy()
    {

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, searchRadius, enemyLayer);

        Transform closestEnemy = null;
        float minSqrDist = Mathf.Infinity;
        Vector2 myPos = transform.position;


        foreach (Collider2D col in colliders)
        {
            if (!col.CompareTag("Enemy")) continue;

            float sqrDist = ((Vector2)col.transform.position - myPos).sqrMagnitude;

            if (sqrDist < minSqrDist)
            {
                minSqrDist = sqrDist;
                closestEnemy = col.transform;
            }
        }

        return closestEnemy;
    }

}