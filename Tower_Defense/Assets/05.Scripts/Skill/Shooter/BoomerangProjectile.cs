using System.Collections.Generic;
using UnityEngine;

public class BoomerangProjectile : MonoBehaviour, IPoolable
{
    private Transform shooter;       // 돌아갈 발사자 (플레이어 or 펫)
    private Vector2 targetPoint;     // 찍고 돌아올 목표점
    private SkillLevelStat myStat;
    private bool isReturning = false;

    // 다단히트 버그 방지용
    private HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>();

    [Header("부메랑 설정")]
    [SerializeField] private float spinSpeed = 1000f; // 회전 속도 (대강 빠르게)

    /// <summary>
    /// 초기화
    /// </summary>
    public void Initialize(Transform targetTransform, SkillLevelStat stat, Transform shooterTransform)
    {
        myStat = stat;
        shooter = shooterTransform;
        isReturning = false;
        hitEnemies.Clear(); // 때린 적 초기화

        if (targetTransform != null)
        {
            // 타겟 방향으로 range 만큼 떨어진 곳을 반환점으로 설정
            Vector2 dir = ((Vector2)targetTransform.position - (Vector2)transform.position).normalized;
            targetPoint = (Vector2)targetTransform.position + dir * myStat.range;
        }
        else
        {
            // 타겟이 없으면 그냥 오른쪽으로 날아감
            targetPoint = (Vector2)transform.position + Vector2.right * myStat.range;
        }
    }

    public void OnSpawn() { }
    public void OnDespawn() { }

    void Update()
    {
        if (myStat == null || shooter == null) return;

        transform.Rotate(Vector3.forward, spinSpeed * Time.deltaTime);

        if (!isReturning)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPoint, myStat.speed * Time.deltaTime);

            // 반환점에 거의 도달했으면 복귀 모드로 전환
            if (Vector2.Distance(transform.position, targetPoint) < 0.1f)
            {
                isReturning = true;
                hitEnemies.Clear(); // 올 때 한 번 더 때릴 수 있게 초기화
            }
        }
        else
        {
            // 발사자에게 돌아가기
            transform.position = Vector2.MoveTowards(transform.position, shooter.position, myStat.speed * Time.deltaTime);

            // 발사자에게 도착하면 풀로 반환
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
            // 다단히트 방지
            if (hitEnemies.Contains(collision)) return;

            // 데미지 주기
            collision.GetComponent<Enemy>().TakeDamage(myStat.damage + StatManager.Instance.projectileDamageBonus, transform.position, 0.2f);

            // 적 등록
            hitEnemies.Add(collision);

        }
    }
}