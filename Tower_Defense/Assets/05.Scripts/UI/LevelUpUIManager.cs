using System.Collections.Generic;
using UnityEngine;

public class LevelUpUIManager : MonoBehaviour
{
    public static LevelUpUIManager Instance { get; private set; }

    [Header("UI 연결")]
    [SerializeField] private RectTransform levelUpPanel;
    [SerializeField] private SkillCardUI[] skillCards = new SkillCardUI[3];

    private void Awake()
    {
        // 싱글톤 세팅
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        levelUpPanel = GetComponent<RectTransform>();
    }


    [ContextMenu("테스트: 레벨업 창 띄우기")]
    public void ShowLevelUpUI()
    {
        Time.timeScale = 0f; // 게임 일시 정지
        levelUpPanel.localScale = Vector3.one;

        List<SkillData> allSkills = Player.Instance.GetAllSkillData();

        for (int i = 0; i < allSkills.Count; i++)
        {
            int randomIndex = Random.Range(i, allSkills.Count);
            SkillData temp = allSkills[i];
            allSkills[i] = allSkills[randomIndex];
            allSkills[randomIndex] = temp;
        }

        for (int i = 0; i < 3; i++)
        {
            if (i < allSkills.Count)
            {
                SkillData pickedSkill = allSkills[i];

                int currentLevel = Player.Instance.GetSkillLevel(pickedSkill.skillName);

                skillCards[i].SetupCard(pickedSkill, currentLevel, OnCardSelected);
                skillCards[i].gameObject.SetActive(true);
            }
            else
            {
                skillCards[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnCardSelected(SkillData selectedData)
    {
        Debug.Log($"<color=cyan>[UI]</color> 플레이어가 '{selectedData.skillName}' 스킬을 선택했습니다");

        Player.Instance.LevelUpSkill(selectedData.skillName);

        levelUpPanel.localScale = Vector3.zero;
        Time.timeScale = 1f;
    }
}
