using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillPet : SkillBase
{

    // 일단 위치는 고정
    [Header("펫 소환 위치 오프셋 (좌, 우, 상, 하)")]
    private Vector2[] petOffsets = new Vector2[]
    {
        new Vector2(-1.5f, 0f),  // 1번째 펫: 좌
        new Vector2(1.5f, 0f),   // 2번째 펫: 우
        new Vector2(0f, 1.5f),   // 3번째 펫: 상
        new Vector2(0f, -1.5f)   // 4번째 펫: 하
    };

    // 현재 소환된 펫들을 기억 (중복 소환 방지 및 레벨업 관리용)
    private Dictionary<string, Pet> spawnedPets = new Dictionary<string, Pet>();

    protected override void Update()
    {
        // update 쓸모없음 그냥 덮어쓰기 용
    }

    protected override void Execute(ActiveSkill skill)
    {
        // 이미 소환된 펫이라면 무시
        if (spawnedPets.ContainsKey(skill.data.skillName))
            return;

        // 소환되고 말거니까 옵젝풀은 생략
        GameObject petObj = Instantiate(skill.data.skillPrefab, transform.position, Quaternion.identity, this.transform);

        // 위치 배정
        int offsetIndex = spawnedPets.Count % 4;
        petObj.transform.localPosition = petOffsets[offsetIndex];

        // 펫 초기화
        Pet petComponent = petObj.GetComponent<Pet>();
        if (petComponent != null)
        {
            petComponent.Initialize(skill.CurrentStat);
        }

        // Dictionary 등록
        spawnedPets.Add(skill.data.skillName, petComponent);
    }

    protected override void OnLevelUp(ActiveSkill skill)
    {
        if (spawnedPets.TryGetValue(skill.data.skillName, out Pet pet))
        {
            pet.UpgradePet(skill.level, skill.CurrentStat);
        }
    }
}