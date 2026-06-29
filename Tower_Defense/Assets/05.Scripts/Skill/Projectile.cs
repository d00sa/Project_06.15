using UnityEngine;

public class Projectile : MonoBehaviour, IPoolable
{
    private Transform target;
    private SkillLevelStat myStat;
    private Vector2 startPos;
    private Vector2 currentDir;

    [Header("이미지 회전 보정값")]
    [SerializeField] private float rotationOffset = -45f; // 보통 45나 -45? 이미지 따라 알잘딱
    [Header("타겟 재탐색 설정")]
    [SerializeField] private float searchRadius = 3f; // 탐색 반경
    [SerializeField] private LayerMask enemyLayer; // 인스펙터에서 Enemy 레이어 선택
    public void Initialize(Transform targetTransform, SkillLevelStat stat)
    {
        target = targetTransform;
        myStat = stat;
        //startPos = transform.position;

        if (target != null)
        {
            currentDir = ((Vector2)target.position - (Vector2)transform.position).normalized;
            UpdateRotation();
        }
    }

    public void OnSpawn()
    {
        //gameObject.transform.position = startPos;
    }

    public void OnDespawn()
    {
        currentDir = Vector2.zero;
    }

    void Update()
    {
        if (myStat == null) return;

        if (target != null && target.gameObject.activeInHierarchy && target.CompareTag("Enemy"))
        {
            currentDir = ((Vector2)target.position - (Vector2)transform.position).normalized;
            UpdateRotation();
        }
        else
        {
            // 타겟 재설정
            target = FindClosestEnemy();
            if (target == null) ObjectPool.Instance.ReturnObj(gameObject);
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
        if (collision.CompareTag("Enemy"))
        {
            collision.GetComponent<Enemy>().TakeDamage(myStat.damage + StatManager.Instance.projectileDamageBonus, transform.position, 0.1f);
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
