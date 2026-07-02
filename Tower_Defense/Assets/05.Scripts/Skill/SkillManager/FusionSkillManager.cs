using System.Collections.Generic;
using UnityEngine;

public class FusionSkillManager : MonoBehaviour
{
    public static FusionSkillManager Instance;

    [Header("게임 내 모든 융합 레시피 등록")]
    public List<FusionSkillData> allRecipes = new List<FusionSkillData>();

    // UI에 확정으로 띄워주기 위해 대기 중인 융합 스킬들을 모아두는 리스트
    public List<FusionSkillData> readyFusions = new List<FusionSkillData>();

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 플레이어가 스킬을 찍을 때마다(레벨업 직후) 호출해서 융합 조건을 검사
    /// </summary>
    public void CheckFusionAvailability()
    {
        foreach (var recipe in allRecipes)
        {
            if (recipe == null || recipe.resultFusionSkill == null ||
                recipe.requiredSkill1 == null || recipe.requiredSkill2 == null)
            {
                continue;
            }

            if (readyFusions.Contains(recipe)) continue;

            // 이미 배운 융합 스킬이면 패스
            if (Player.Instance.GetSkillLevel(recipe.resultFusionSkill.skillName) > 0) continue;

            // 💡 Player.Instance를 통해 직접 레벨을 가져옵니다.
            int level1 = Player.Instance.GetSkillLevel(recipe.requiredSkill1.skillName);
            int level2 = Player.Instance.GetSkillLevel(recipe.requiredSkill2.skillName);

            bool isSkill1Maxed = level1 > 0 && level1 >= recipe.requiredSkill1.maxLevel;
            bool isSkill2Maxed = level2 > 0 && level2 >= recipe.requiredSkill2.maxLevel;

            if (isSkill1Maxed && isSkill2Maxed)
            {
                Debug.Log($"<color=yellow>[Fusion]</color> {recipe.resultFusionSkill.skillName} 융합 조건 달성! 다음 레벨업에 등장합니다.");
                readyFusions.Add(recipe);
            }
        }
    }
}
