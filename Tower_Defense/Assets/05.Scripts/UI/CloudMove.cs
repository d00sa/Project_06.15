using UnityEngine;

public class CloudMove : MonoBehaviour
{
    [Header("랜덤 요소 설정")]
    [SerializeField] private float minSpeed = 30f;
    [SerializeField] private float maxSpeed = 100f;
    [SerializeField] private float minY = -200f;
    [SerializeField] private float maxY = 400f;

    private RectTransform rectTransform;
    private RectTransform parentRect;
    private float currentSpeed;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentRect = transform.parent as RectTransform;
    }

    void Start()
    {
        ResetCloud(true);
    }

    void Update()
    {
        rectTransform.anchoredPosition += Vector2.right * currentSpeed * Time.deltaTime;

        float parentHalfWidth = parentRect.rect.width * 0.5f;
        float cloudHalfWidth = (rectTransform.rect.width * rectTransform.localScale.x) * 0.5f;
        float rightLimit = parentHalfWidth + cloudHalfWidth;

        if (rectTransform.anchoredPosition.x > rightLimit) ResetCloud(false);
    }

    private void ResetCloud(bool isInitialSetup)
    {
        currentSpeed = Random.Range(minSpeed, maxSpeed);

        float parentHalfWidth = parentRect.rect.width * 0.5f;
        float parentHalfHeight = parentRect.rect.height * 0.5f;
        float cloudHalfWidth = (rectTransform.rect.width * rectTransform.localScale.x) * 0.5f;

        float leftLimit = -parentHalfWidth - cloudHalfWidth;
        float rightLimit = parentHalfWidth + cloudHalfWidth;
        float randomY = Random.Range(minY, maxY);

        // 처음 시작 시엔 화면 내부 랜덤 배치, 재생성 시엔 화면 왼쪽 밖(leftLimit)에서 출발
        float startX = isInitialSetup ? Random.Range(leftLimit, rightLimit) : leftLimit;
        rectTransform.anchoredPosition = new Vector2(startX, randomY);
    }
}
