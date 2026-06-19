using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;

    private Button button;
    private SkillData mySkillData;
    private System.Action<SkillData> onClickCallback; // 클릭됐을 때 매니저에게 콜백

    private int cachedCurrentLevel;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);
    }

    public void SetupCard(SkillData data, int currentLevel, System.Action<SkillData> callback)
    {
        mySkillData = data;
        onClickCallback = callback;
        cachedCurrentLevel = currentLevel;

        if (iconImage != null) iconImage.sprite = data.icon;
        if (nameText != null) nameText.text = data.skillName;
        UpdateLevelText(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UpdateLevelText(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UpdateLevelText(false);
    }

    // 마우스 올리면(hover) 실행
    private void UpdateLevelText(bool isHovering)
    {
        if (levelText == null) return;

        if (isHovering)
        {
            levelText.text = $"Lv.{cachedCurrentLevel} <color=green>+1</color>";
        }
        else
        {
            levelText.text = $"Lv.{cachedCurrentLevel}";
        }
    }

    private void OnButtonClicked()
    {
        onClickCallback?.Invoke(mySkillData);
    }
}
