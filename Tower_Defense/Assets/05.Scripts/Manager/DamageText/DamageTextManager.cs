using UnityEngine;

public class DamageTextManager : MonoBehaviour
{
    public static DamageTextManager Instance { get; private set; }

    [Header("프리팹 설정")]
    [SerializeField] private GameObject damageTextPrefab;
    [SerializeField] private int poolCount = 50;

    [Header("위치 조정")]
    [Tooltip("텍스트가 뜨는 기본 높이를 조절 (음수면 아래 양수면 위)")]
    [SerializeField] private float spawnOffsetY = -0.5f;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 시작하자마자 오브젝트 풀에 데미지 텍스트 50개를 미리 짱박아둡니다.
        if (damageTextPrefab != null)
        {
            ObjectPool.Instance.RegisterPoolElement(damageTextPrefab, poolCount);
        }
    }

    /// <summary>
    /// 적이 맞았을 때 호출
    /// </summary>
    public void ShowDamage(float damage, Vector2 position)
    {
        if (damageTextPrefab == null) return;

        // 텍스트가 완전 겹치는 문제 방지용
        float randomX = Random.Range(-0.3f, 0.3f);
        float randomY = Random.Range(-0.1f, 0.3f);
        Vector2 spawnPos = new Vector2(position.x + randomX, position.y + randomY + spawnOffsetY);

        GameObject textObj = ObjectPool.Instance.GetObj(damageTextPrefab.name, spawnPos, null, true);

        if (textObj.TryGetComponent<DamageText>(out var damageText))
            damageText.Setup(damage);
    }
}