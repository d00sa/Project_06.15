using System.Collections.Generic;
using UnityEngine;

public class CopyPet : Pet
{
    private float timer = 0f;
    private int currentPetLevel = 1;

    public override void Initialize(SkillLevelStat stat)
    {
        base.Initialize(stat);
        currentPetLevel = 1;
        timer = 0f;
        SwapRandomPlayerSkill();
    }

    public override void UpgradePet(int level, SkillLevelStat stat)
    {
        base.UpgradePet(level, stat);
        currentPetLevel = level;

        // 레벨업시 새로운 스킬로 교체
        SwapRandomPlayerSkill();
        timer = 0f;
    }

    private void Update()
    {
        if (currentPetStat == null) return;

        timer += Time.deltaTime;

        float currentDuration = currentPetStat.speed;

        if (timer >= currentDuration)
        {
            SwapRandomPlayerSkill();
            timer = 0f;
        }
    }

    private void SwapRandomPlayerSkill()
    {
        List<string> playerSkills = GetPlayerActiveSkills();

        if (playerSkills == null || playerSkills.Count == 0) return;

        string randomSkillName = playerSkills[Random.Range(0, playerSkills.Count)];

        ClearPetSkills();

        for (int i = 0; i < currentPetLevel; i++)
        {
            LevelUpPetSkill(randomSkillName);
        }

        Debug.Log($"카피캣 펫 플레이어의 {randomSkillName} 스킬 카피 (적용 레벨: {currentPetLevel})");

    }

    private List<string> GetPlayerActiveSkills()
    {
        List<string> activeNames = new List<string>();

        if (Player.Instance != null)
        {
            SkillShooter shooter = Player.Instance.GetComponent<SkillShooter>();
            if (shooter != null) activeNames.AddRange(shooter.GetActiveSkillNames());

            SkillAOE aoe = Player.Instance.GetComponent<SkillAOE>();
            if (aoe != null) activeNames.AddRange(aoe.GetActiveSkillNames());
        }

        return activeNames;
    }
}