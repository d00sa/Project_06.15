using System.Collections.Generic;
using UnityEngine;

public class LevelUpUIManager : MonoBehaviour
{
    public static LevelUpUIManager Instance { get; private set; }

    [Header("UI 연결")]
    [SerializeField] private GameObject levelUpPanel;
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
        levelUpPanel.SetActive(false);
    }


    [ContextMenu("테스트: 레벨업 창 띄우기")]
    public void ShowLevelUpUI()
    {
        Time.timeScale = 0f; // 게임 일시 정지
        levelUpPanel.SetActive(true);

        List<SkillData> allSkills = Player.Instance.GetAllSkillData();

        // 임시 앞에서부터 3개 뽑음
        // 스킬이 많아지면 셔플로 뽑는 걸로 바꿀 예정

        for (int i = 0; i < 3; i++)
        {
            if (i < allSkills.Count)
            {
                SkillData pickedSkill = allSkills[i];

                int currentLevel = Player.Instance.GetSkillLevel(pickedSkill.skillName);

                // 카드에 데이터 주입 후 OnCardSelected 함수가 실행되도록 연결
                skillCards[i].SetupCard(pickedSkill, currentLevel, OnCardSelected);
                skillCards[i].gameObject.SetActive(true);
            }
            else
            {
                // 스킬이 3개가 안 될 경우 남는 카드는 숨김
                skillCards[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnCardSelected(SkillData selectedData)
    {
        Debug.Log($"<color=cyan>[UI]</color> 플레이어가 '{selectedData.skillName}' 스킬을 선택했습니다");

        Player.Instance.LevelUpSkill(selectedData.skillName);

        levelUpPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
