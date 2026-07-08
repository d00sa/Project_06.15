using UnityEngine;

public class SafeArea : MonoBehaviour
{
    private Rect _lastSafeArea;
    private Vector2Int _lastResolution;

    void Start()
    {
        ApplySafeArea();
    }

    #if UNITY_EDITOR
    private void Update()
    {
        if (_lastSafeArea != Screen.safeArea ||
            _lastResolution.x != Screen.width ||
            _lastResolution.y != Screen.height) {
            ApplySafeArea();
        }
    }
    #endif

    private void ApplySafeArea()
    {
        _lastSafeArea = Screen.safeArea;
        _lastResolution = new Vector2Int(Screen.width, Screen.height);

        RectTransform rt = GetComponent<RectTransform>();

        rt.anchorMin = new Vector2(
            Screen.safeArea.xMin / Screen.width,
            Screen.safeArea.yMin / Screen.height);

        rt.anchorMax = new Vector2(
            Screen.safeArea.xMax / Screen.width,
            Screen.safeArea.yMax / Screen.height);

        Debug.Log("SafeArea Apply");
    }
}
