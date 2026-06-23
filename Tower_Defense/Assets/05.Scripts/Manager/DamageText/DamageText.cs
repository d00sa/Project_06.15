using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour, IPoolable
{
    private TextMeshPro textMesh; 
    private float timer;

    [Header("데미지 텍스트 설정")]
    [SerializeField] private float moveSpeed = 2f; // 떠오르는 속도
    [SerializeField] private float lifeTime = 0.8f; // 유지 시간

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    /// <summary>
    /// 매니저가 텍스트를 꺼낼 때 호출하여 데미지 값과 초기화를 진행합니다.
    /// </summary>
    public void Setup(float damage)
    {
        textMesh.text = damage.ToString("F0"); // 소수점 제거하고 정수로 표시

        // 투명도 100%로 초기화
        Color color = textMesh.color;
        color.a = 1f;
        textMesh.color = color;

        timer = 0f;
    }

    public void OnSpawn() { }

    public void OnDespawn() { }

    private void Update()
    {
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime, UnityEngine.Space.World);

        timer += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, timer / lifeTime);

        Color color = textMesh.color;
        color.a = alpha;
        textMesh.color = color;

        if (timer >= lifeTime)
        {
            ObjectPool.Instance.ReturnObj(gameObject);
        }
    }
}