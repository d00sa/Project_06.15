using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SimpleFlash : MonoBehaviour
{
    private SpriteRenderer sr;

    [Tooltip("번쩍! 하고 밝아지는 데 걸리는 시간")]
    [SerializeField] private float flashInTime = 0.1f;
    [Tooltip("서서히 투명해지며 사라지는 데 걸리는 시간")]
    [SerializeField] private float fadeOutTime = 0.5f;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        // 처음엔 완전 투명하게 시작
        Color c = sr.color;
        c.a = 0f;
        sr.color = c;

        // 0.1초 만에 투명도를 1(완전 불투명)로 올려서 눈뽕!
        float t = 0f;
        while (t < flashInTime)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, t / flashInTime);
            sr.color = c;
            yield return null;
        }

        // 0.5초 동안 서서히 투명도를 0으로 내려서 시야 복구
        t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / fadeOutTime);
            sr.color = c;
            yield return null;
        }

        c.a = 0f;
        sr.color = c;
    }
}