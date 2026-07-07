using System.Collections.Generic;
using UnityEngine;

public class BoomerangProjectile : MonoBehaviour, ISkillEffect
{
    private Transform shooter;
    private SkillLevelStat myStat;
    private StatType damageBonusType;
    private float effectiveSpeed;

    // 공통: 다단히트 방지용
    private HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>();

    [Header("부메랑 설정")]
    [SerializeField] private float spinSpeed = 2000f;

    [Header("특수 이동 옵션")]
    [Tooltip("체크 시 타겟을 무시하고 무작위 360도 방향으로 날아갔다가 돌아옵니다.")]
    public bool isRandomDirection = false;
    [SerializeField]  private float actualRange = 20f;

    private Vector2 targetPoint;
    private bool isReturning = false;

    private Vector2 randomDir;
    private float distanceTraveled = 0f;
    private bool isChakramReturning = false;



    public void Initialize(SkillEffectContext ctx)
    {
        myStat = ctx.stat;
        damageBonusType = ctx.damageBonusType;
        effectiveSpeed = myStat.speed * (1f + Player.Instance.Stat.GetStat(StatType.ProjectileSpeed));
        shooter = ctx.caster;
        hitEnemies.Clear();

        // 💡 분기점: 랜덤 체크 여부에 따라 초기화도 완전히 다르게 진행
        if (isRandomDirection)
        {
            // [새로운 로직] 차크람 초기화
            randomDir = UnityEngine.Random.insideUnitCircle.normalized;
            if (randomDir == Vector2.zero) randomDir = Vector2.up;

            distanceTraveled = 0f;
            isChakramReturning = false;
            actualRange = myStat.range > 1f ? myStat.range : 5f; // 사거리 0 에러 방지
        }
        else
        {
            // [기존 로직] 일반 부메랑 초기화
            isReturning = false;
            if (ctx.target != null)
            {
                Vector2 dir = ((Vector2)ctx.target.position - (Vector2)transform.position).normalized;
                targetPoint = (Vector2)ctx.target.position + dir * myStat.range;
            }
            else
            {
                targetPoint = (Vector2)transform.position + Vector2.right * myStat.range;
            }
        }

        SoundManager.Instance.PlaySFX("Gun");
    }

    public void OnSpawn() { }
    public void OnDespawn() { }

    void Update()
    {
        if (myStat == null || shooter == null || !shooter.gameObject.activeInHierarchy)
        {
            ObjectPool.Instance.ReturnObj(gameObject);
            return;
        }

        // 빙글빙글 도는 시각 효과는 공통
        transform.Rotate(Vector3.forward, spinSpeed * Time.deltaTime);

        // 💡 분기점: 실행 함수를 완전히 쪼갬
        if (isRandomDirection)
        {
            HandleRandomChakram(); // 새 로직
        }
        else
        {
            HandleNormalBoomerang(); // 기존 로직
        }
    }

    // ==========================================
    // 💡 새 함수: 랜덤 차크람 전용 이동 로직
    // ==========================================
    private void HandleRandomChakram()
    {
        if (!isChakramReturning)
        {
            // 무조건 지정된 랜덤 방향(randomDir)으로 직진
            float step = effectiveSpeed * Time.deltaTime;
            transform.Translate(randomDir * step, Space.World);
            distanceTraveled += step;

            // 특정 거리(actualRange)만큼 날아갔으면 복귀
            if (distanceTraveled >= actualRange)
            {
                isChakramReturning = true;
                hitEnemies.Clear(); // 돌아올 때 다단히트 리셋
            }
        }
        else
        {
            // 플레이어에게 복귀
            transform.position = Vector2.MoveTowards(transform.position, shooter.position, effectiveSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, shooter.position) < 0.5f)
            {
                ObjectPool.Instance.ReturnObj(gameObject);
            }
        }
    }

    // ==========================================
    // 💡 기존 함수: 일반 부메랑 이동 로직 (원형 보존)
    // ==========================================
    private void HandleNormalBoomerang()
    {
        if (!isReturning)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPoint, effectiveSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, targetPoint) < 0.1f)
            {
                isReturning = true;
                hitEnemies.Clear();
            }
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, shooter.position, effectiveSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, shooter.position) < 0.5f)
            {
                ObjectPool.Instance.ReturnObj(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (hitEnemies.Contains(collision)) return;

            collision.GetComponent<Enemy>().TakeDamage(myStat.damage + Player.Instance.Stat.GetStat(damageBonusType), transform.position, 0.2f);
            hitEnemies.Add(collision);
        }
    }
}