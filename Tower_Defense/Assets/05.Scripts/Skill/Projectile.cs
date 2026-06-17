using UnityEngine;

public class Projectile : MonoBehaviour, IPoolable
{
    private Transform target;
    private SkillLevelStat myStat;
    private Vector2 startPos;
    private Vector2 currentDir;

    [Header("이미지 회전 보정값")]
    [SerializeField] private float rotationOffset = -45f; // 보통 45나 -45? 이미지 따라 알잘딱

    public void Initialize(Transform targetTransform, SkillLevelStat stat)
    {
        target = targetTransform;
        myStat = stat;
        startPos = transform.position;

        if (target != null)
        {
            currentDir = ((Vector2)target.position - (Vector2)transform.position).normalized;
            UpdateRotation();
        }
    }

    public void OnSpawn()
    {
        gameObject.transform.position = startPos;
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
            if (target == null ) ObjectPool.Instance.ReturnObj(gameObject);
        }

        transform.Translate(currentDir * myStat.speed * Time.deltaTime, Space.World);

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
            collision.GetComponent<Enemy>().HP -= myStat.damage;
            ObjectPool.Instance.ReturnObj(gameObject);
        }
    }

    // 기존 타겟인 적이 사라졌을 때 가장 가까운 적을 찾는 로직, 너무 무거우면 그냥 사라지게 할까 고민 중
    private Transform FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform closestEnemy = null;

        float minSqrDist = Mathf.Infinity;
        Vector2 myPos = transform.position;

        foreach (GameObject enemy in enemies)
        {
            Vector2 dirToEnemy = (Vector2)enemy.transform.position - myPos;
            float sqrDist = dirToEnemy.sqrMagnitude;

            if (sqrDist < minSqrDist)
            {
                minSqrDist = sqrDist;
                closestEnemy = enemy.transform;
            }
        }

        return closestEnemy;
    }
}
