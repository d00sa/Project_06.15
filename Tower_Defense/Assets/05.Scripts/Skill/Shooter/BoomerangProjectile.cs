using System.Collections.Generic;
using UnityEngine;

public class BoomerangProjectile : MonoBehaviour, ISkillEffect
{
    // 캐싱해 둘 최종 데미지
    protected float calculatedFinalDamage;

    private Transform shooter;
    private SkillLevelStat myStat;

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
    private float finalSpeedStat = 0f;


    public void Initialize(SkillEffectContext ctx)
    {
        myStat = ctx.stat;
        shooter = ctx.caster;

        calculatedFinalDamage = Player.Instance.Stat.CalculateFinalDamage(
        myStat.damage,
        myStat.coolTime,
        myStat.fireRate
        );

        finalSpeedStat = Player.Instance.Stat.GetStat(StatType.ProjectileSpeed) + myStat.speed;

        hitEnemies.Clear();

        if (isRandomDirection)
        {
            randomDir = UnityEngine.Random.insideUnitCircle.normalized;
            if (randomDir == Vector2.zero) randomDir = Vector2.up;

            distanceTraveled = 0f;
            isChakramReturning = false;
            actualRange = myStat.range > 1f ? myStat.range : 5f; // 사거리 0 에러 방지
        }
        else
        {
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

        transform.Rotate(Vector3.forward, spinSpeed * Time.deltaTime);

        if (isRandomDirection)
        {
            HandleRandomChakram(); // 새 로직
        }
        else
        {
            HandleNormalBoomerang(); // 기존 로직
        }
    }

    private void HandleRandomChakram()
    {
        if (!isChakramReturning)
        {
            // 무조건 지정된 랜덤 방향(randomDir)으로 직진
            float step = finalSpeedStat * Time.deltaTime;
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
            transform.position = Vector2.MoveTowards(transform.position, shooter.position, finalSpeedStat * Time.deltaTime);

            if (Vector2.Distance(transform.position, shooter.position) < 0.5f)
            {
                ObjectPool.Instance.ReturnObj(gameObject);
            }
        }
    }

    private void HandleNormalBoomerang()
    {
        if (!isReturning)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPoint, finalSpeedStat * Time.deltaTime);

            if (Vector2.Distance(transform.position, targetPoint) < 0.1f)
            {
                isReturning = true;
                hitEnemies.Clear();
            }
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, shooter.position, finalSpeedStat * Time.deltaTime);

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

            collision.GetComponent<Enemy>().TakeDamage(calculatedFinalDamage, transform.position, 0.2f);
            hitEnemies.Add(collision);
        }
    }
}