using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpUIManager : MonoBehaviour
{
    public static LevelUpUIManager Instance { get; private set; }

    [Header("UI 연결")]
    [SerializeField] private RectTransform levelUpPanel;
    [SerializeField] private SkillCardUI[] skillCards = new SkillCardUI[3];
    private GraphicRaycaster _graphic;

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
        _graphic = GetComponent<GraphicRaycaster>();
        _graphic.enabled = false;
    }


    [ContextMenu("테스트: 레벨업 창 띄우기")]
    public void ShowLevelUpUI()
    {
        Time.timeScale = 0f; // 게임 일시 정지
        levelUpPanel.localScale = Vector3.one;
        _graphic.enabled = true;

        List<SkillData> allSkills = Player.Instance.GetAllSkillData();

        // 이미 만렙을 찍은 스킬은 카드 후보에서 제외
        allSkills.RemoveAll(skill =>
        {
            int level = Player.Instance.GetSkillLevel(skill.skillName);
            return level > 0 && level >= skill.maxLevel;
        });

        // 대체형 융합으로 소모되어 사라진 스킬은 다시 등장하지 않도록 제외
        if (FusionSkillManager.Instance != null)
        {
            allSkills.RemoveAll(skill => FusionSkillManager.Instance.consumedFusionSkills.Contains(skill.skillName));
        }

        List<SkillData> selectedSkills = new List<SkillData>();

        if (FusionSkillManager.Instance != null && FusionSkillManager.Instance.readyFusions.Count > 0)
        {
            // 대기열의 첫 번째 융합 레시피의 결과 스킬을 선택 리스트에 추가
            FusionSkillData readyRecipe = FusionSkillManager.Instance.readyFusions[0];
            selectedSkills.Add(readyRecipe.resultFusionSkill);
        }

        for (int i = 0; i < allSkills.Count; i++)
        {
            int randomIndex = Random.Range(i, allSkills.Count);
            SkillData temp = allSkills[i];
            allSkills[i] = allSkills[randomIndex];
            allSkills[randomIndex] = temp;
        }

        foreach (var skill in allSkills)
        {
            if (selectedSkills.Count >= 3) break; // 3장이 꽉 차면 종료

            // 융합 스킬이 일반 스킬 목록에 중복으로 들어가는 것을 방지
            if (!selectedSkills.Contains(skill))
            {
                selectedSkills.Add(skill);
            }
        }

        for (int i = 0; i < 3; i++)
        {
            if (i < selectedSkills.Count)
            {
                SkillData pickedSkill = selectedSkills[i];
                int currentLevel = Player.Instance.GetSkillLevel(pickedSkill.skillName);

                skillCards[i].SetupCard(pickedSkill, currentLevel, OnCardSelected);
                skillCards[i].gameObject.SetActive(true);
            }
            else
                skillCards[i].gameObject.SetActive(false);
        }
    }

    private void OnCardSelected(SkillData selectedData)
    {
        // 유저가 선택한 스킬이 융합 스킬인지 확인
        FusionSkillData appliedRecipe = null;
        if (FusionSkillManager.Instance != null)
        {
            appliedRecipe = FusionSkillManager.Instance.readyFusions.Find(r => r.resultFusionSkill == selectedData);
        }

        // 융합 스킬을 선택했을 경우의 특수 처리
        if (appliedRecipe != null)
        {
            Debug.Log($"<color=yellow>[Fusion]</color> '{selectedData.skillName}' 융합 성공!");

            // 대기열에서 제거 (다음에 또 뜨지 않게)
            FusionSkillManager.Instance.readyFusions.Remove(appliedRecipe);

            // [대체형] 융합이라면 기존 스킬 2개를 플레이어에게서 삭제
            if (appliedRecipe.isReplacement)
            {
                Player.Instance.RemoveSkill(appliedRecipe.requiredSkill1.skillName);
                Player.Instance.RemoveSkill(appliedRecipe.requiredSkill2.skillName);

                // 소모된 스킬로 기록 -> 이후 레벨업 카드 풀에서 영구 제외
                FusionSkillManager.Instance.consumedFusionSkills.Add(appliedRecipe.requiredSkill1.skillName);
                FusionSkillManager.Instance.consumedFusionSkills.Add(appliedRecipe.requiredSkill2.skillName);
            }

            // 새로운 융합 스킬 획득 (1레벨)
            Player.Instance.LevelUpSkill(selectedData.skillName);
        }
        else
        {
            // 일반 스킬 레벨업
            Debug.Log($"<color=cyan>[UI]</color> 플레이어가 '{selectedData.skillName}' 스킬을 선택했습니다");
            Player.Instance.LevelUpSkill(selectedData.skillName);
        }

        levelUpPanel.localScale = Vector3.zero;
        Time.timeScale = GameManager.Instance.CurSpeed;
        _graphic.enabled = false;
    }
}
