using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ReplicatingCell : MonoBehaviour, ISkillEffect
{
    private SkillLevelStat myStat;
    private StatType damageBonusType;
    private Transform caster;

    private float effectiveSpeed;
    private Vector2 moveDirection;
    private float currentDamage;

    [Header("세포 분열 설정")]
    [Tooltip("최대 분열 횟수 (예: 3이면 1->2->4->8개까지 늘어나고 끝남)")]
    [SerializeField] private int maxSplitCount = 3;
    [SerializeField] private float knockbackPower = 0.5f;

    private int currentSplitCount = 0;

    // 연쇄 분열 방지용 변수
    private Enemy ignoredEnemy = null;

    private Vector2 minBounds;
    private Vector2 maxBounds;

    public void Initialize(SkillEffectContext ctx)
    {
        myStat = ctx.stat;
        damageBonusType = ctx.damageBonusType;
        caster = ctx.caster;
        effectiveSpeed = myStat.speed * (1f + Player.Instance.Stat.GetStat(StatType.ProjectileSpeed));

        currentSplitCount = 0;
        transform.localScale = Vector3.one;
        currentDamage = myStat.damage + Player.Instance.Stat.GetStat(damageBonusType);

        ignoredEnemy = null; // 초기화 시 초기화

        if (ctx.target != null)
            moveDirection = ((Vector2)ctx.target.position - (Vector2)transform.position).normalized;
        else
        {
            moveDirection = Random.insideUnitCircle.normalized;
            if (moveDirection == Vector2.zero) moveDirection = Vector2.up;
        }

        SoundManager.Instance.PlaySFX("Gun");
    }

    /// <summary>
    /// 분열된 자식 세포 전용 셋업 (무시할 적 추가)
    /// </summary>
    public void SetupAsChild(SkillLevelStat stat, StatType bonusType, Transform casterTrans, int splitGen, Vector2 dir, Vector3 parentScale, float parentDamage, Enemy enemyToIgnore)
    {
        myStat = stat;
        damageBonusType = bonusType;
        caster = casterTrans;
        effectiveSpeed = myStat.speed * (1f + Player.Instance.Stat.GetStat(StatType.ProjectileSpeed));

        currentSplitCount = splitGen;
        moveDirection = dir.normalized;

        transform.localScale = parentScale * 0.5f;
        currentDamage = parentDamage * 0.5f;

        ignoredEnemy = enemyToIgnore;
    }

    public void OnSpawn() { }
    public void OnDespawn() { }

    private void UpdateScreenBounds()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        float vertExtent = cam.orthographicSize;
        float horzExtent = vertExtent * Screen.width / Screen.height;
        minBounds = new Vector2(cam.transform.position.x - horzExtent, cam.transform.position.y - vertExtent);
        maxBounds = new Vector2(cam.transform.position.x + horzExtent, cam.transform.position.y + vertExtent);
    }

    private void Update()
    {
        if (myStat == null) return;

        UpdateScreenBounds();
        transform.Translate(moveDirection * effectiveSpeed * Time.deltaTime, Space.World);
        CheckBoundsAndBounce();
    }

    private void CheckBoundsAndBounce()
    {
        Vector3 pos = transform.position;
        bool bounced = false;

        if (pos.x <= minBounds.x || pos.x >= maxBounds.x)
        {
            moveDirection.x *= -1;
            pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
            bounced = true;
        }
        if (pos.y <= minBounds.y || pos.y >= maxBounds.y)
        {
            moveDirection.y *= -1;
            pos.y = Mathf.Clamp(pos.y, minBounds.y, maxBounds.y);
            bounced = true;
        }

        if (bounced) transform.position = pos;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && collision.TryGetComponent<Enemy>(out var enemy))
        {
            if (enemy.IsDead || !enemy.gameObject.activeInHierarchy) return;

            // 막 떄린 적은 그냥 통과 (연쇄 폭발 완벽 방지)
            if (enemy == ignoredEnemy) return;

            enemy.TakeDamage(currentDamage, transform.position, knockbackPower);

            if (currentSplitCount < maxSplitCount)
            {
                SplitCell(enemy);
            }

            ObjectPool.Instance.ReturnObj(gameObject);
        }
    }

    private void SplitCell(Enemy hitEnemy)
    {
        Vector2 dir1 = Quaternion.Euler(0, 0, 30) * moveDirection;
        Vector2 dir2 = Quaternion.Euler(0, 0, -30) * moveDirection;

        SpawnChild(dir1, hitEnemy);
        SpawnChild(dir2, hitEnemy);
    }

    private void SpawnChild(Vector2 dir, Enemy hitEnemy)
    {
        GameObject cell = ObjectPool.Instance.GetObj(gameObject.name, transform.position, null, true);
        if (cell.TryGetComponent<ReplicatingCell>(out var rep))
        {
            rep.SetupAsChild(myStat, damageBonusType, caster, currentSplitCount + 1, dir, transform.localScale, currentDamage, hitEnemy);
        }
    }
}