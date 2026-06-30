using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BouncyBall : Pet
{
    private Vector2 moveDirection;

    [Header("탱탱볼 전용 설정")]
    [Tooltip("적에게 맞았을 때 밀쳐내는 힘")]
    [SerializeField] private float knockbackPower = 0.5f;

    private Vector2 minBounds;
    private Vector2 maxBounds;

    // Initialize 함수 오버라이드 (기존 펫 로직 + 탱탱볼 전용 로직)
    public override void Initialize(SkillLevelStat stat)
    {
        base.Initialize(stat); 

        // 랜덤한 방향으로 튕기기
        moveDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        UpdateScreenBounds();
    }

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
        if (currentPetStat == null) return;

        // 스탯 매니저의 currentPetStat.speed 적용
        transform.Translate(moveDirection * currentPetStat.speed * Time.deltaTime, Space.World);
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

        if (bounced)
            transform.position = pos;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && collision.TryGetComponent<Enemy>(out var enemy))
        {
            // currentPetStat.damage 사용
            enemy.TakeDamage(currentPetStat.damage + StatManager.Instance.projectileDamageBonus, transform.position, knockbackPower);

            Vector2 normal = ((Vector2)transform.position - (Vector2)enemy.transform.position).normalized;
            moveDirection = Vector2.Reflect(moveDirection, normal).normalized;
        }
    }
}