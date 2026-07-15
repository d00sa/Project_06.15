using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillPet : SkillBase
{
    [Header("펫 소환 위치 오프셋 (좌, 우, 상, 하)")]
    private Vector2[] petOffsets = new Vector2[]
    {
        new Vector2(-1.5f, 0f),
        new Vector2(1.5f, 0f),
        new Vector2(0f, 1.5f),
        new Vector2(0f, -1.5f)
    };

    // 현재 소환된 펫들을 기억 (중복 소환 방지 및 레벨업 관리용)
    private Dictionary<string, List<Pet>> spawnedPets = new Dictionary<string, List<Pet>>();

    protected override void Update()
    {
    }

    protected override void Execute(ActiveSkill skill)
    {
        // 1레벨(최초 소환)일 때만 작동
        if (spawnedPets.ContainsKey(skill.data.skillName) && spawnedPets[skill.data.skillName].Count > 0)
            return;

        SpawnPetObj(skill);
    }

    protected override void OnLevelUp(ActiveSkill skill, bool exist)
    {
        if (spawnedPets.TryGetValue(skill.data.skillName, out List<Pet> petList))
        {
            bool shouldAddQuantity = false;

            // 기존에 소환되어 있던 모든 펫의 스탯 업그레이드
            foreach (var pet in petList)
            {
                if (pet != null)
                {
                    pet.UpgradePet(skill.level, skill.CurrentStat);
                    if (pet.isQuantityType) shouldAddQuantity = true;
                }
            }

            // 만약 마리수가 늘어나는 타입이라면 1마리 추가
            if (shouldAddQuantity)
            {
                SpawnPetObj(skill);
            }
        }
    }

    private void SpawnPetObj(ActiveSkill skill)
    {
        if (skill.data.skillPrefab == null) return;

        GameObject petObj = Instantiate(skill.data.skillPrefab, transform.position, Quaternion.identity);
        Pet petComponent = petObj.GetComponent<Pet>();

        if (petComponent != null)
        {
            if (!spawnedPets.ContainsKey(skill.data.skillName))
                spawnedPets[skill.data.skillName] = new List<Pet>();

            if (!petComponent.isFreeMoving)
            {
                // 플레이어 옆에 둥둥 떠다니는 고정 펫
                petObj.transform.SetParent(this.transform);
                int offsetIndex = spawnedPets[skill.data.skillName].Count % 4;
                petObj.transform.localPosition = petOffsets[offsetIndex];
            }
            else
            {
                petObj.transform.SetParent(null);
            }

            petComponent.Initialize(skill.CurrentStat);

            spawnedPets[skill.data.skillName].Add(petComponent);
        }
    }

    protected override void OnSkillRemoved(ActiveSkill skill)
    {
        base.OnSkillRemoved(skill);

        // 이 스킬 이름으로 소환된 펫 목록이 있는지 확인
        if (spawnedPets.TryGetValue(skill.data.skillName, out List<Pet> petList))
        {

            foreach (var pet in petList)
            {
                if (pet != null)
                {
                    Destroy(pet.gameObject);
                }
            }

            spawnedPets.Remove(skill.data.skillName);
        }
    }
}