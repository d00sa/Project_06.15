using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ReplicatingCell : MonoBehaviour, ISkillEffect
{
    // 최종 데미지 캐싱용
    protected float calculatedFinalDamage;

    private SkillLevelStat myStat;
    private Transform caster;
    private Vector2 moveDirection;

    // 크기에 따라 변하는 현재 데미지 수치
    private float currentDamage;
    // 크기에 따라 변하는 현재 넉백 수치
    private float currentKnockbackPower;

    [Header("세포 분열 설정")]
    [Tooltip("최대 분열 횟수 (예: 3이면 1->2->4->8개까지 늘어나고 끝남)")]
    [SerializeField] private int maxSplitCount = 3;
    [SerializeField] private float knockbackPower = 0.5f;

    private int currentSplitCount = 0;
    private Enemy ignoredEnemy = null;

    private Vector2 minBounds;
    private Vector2 maxBounds;
    private float finalSpeedStat = 0f;

    public void Initialize(SkillEffectContext ctx)
    {
        myStat = ctx.stat;
        caster = ctx.caster;
        finalSpeedStat = Player.Instance.Stat.GetStat(StatType.ProjectileSpeed) + myStat.speed;

        currentSplitCount = 0;
        transform.localScale = Vector3.one;

        calculatedFinalDamage = Player.Instance.Stat.CalculateFinalDamage(
            myStat.damage,
            myStat.coolTime,
            myStat.fireRate
        );

        currentDamage = calculatedFinalDamage;

        // 초기 넉백 파워 설정
        currentKnockbackPower = knockbackPower;
        ignoredEnemy = null;

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
    /// 분열 시 부모의 데미지와 넉백 파워를 전달받아 절반으로 설정
    /// </summary>
    public void SetupAsChild(SkillLevelStat stat, Transform casterTrans, int splitGen, Vector2 dir, Vector3 parentScale, float parentDamage, float parentKnockback, Enemy enemyToIgnore)
    {
        myStat = stat;
        caster = casterTrans;

        currentSplitCount = splitGen;
        moveDirection = dir.normalized;

        transform.localScale = parentScale * 0.5f;
        currentDamage = parentDamage * 0.5f;


        currentKnockbackPower = parentKnockback * 0.5f;

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
        transform.Translate(moveDirection * Time.deltaTime * finalSpeedStat, Space.World);
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
            if (enemy == ignoredEnemy) return;

            enemy.TakeDamage(currentDamage, transform.position, currentKnockbackPower);

            if (currentSplitCount < maxSplitCount)
            {
                SplitCell(enemy);
            }

            ObjectPool.Instance.ReturnObj(gameObject);
        }
        else if (collision.TryGetComponent<ReplicatingCell>(out var otherCell))
        {
            if (this.currentSplitCount < otherCell.currentSplitCount)
                return;
            Vector2 normal = ((Vector2)transform.position - (Vector2)otherCell.transform.position).normalized;
            if (normal == Vector2.zero) return;
            moveDirection = Vector2.Reflect(moveDirection, normal).normalized;
            if (this.currentSplitCount > otherCell.currentSplitCount)
            {
                transform.position += (Vector3)normal * 0.3f;
            }
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
            rep.SetupAsChild(myStat, caster, currentSplitCount + 1, dir, transform.localScale, currentDamage, currentKnockbackPower, hitEnemy);
        }
    }
}