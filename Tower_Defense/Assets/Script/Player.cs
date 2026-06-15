using System.Collections.Generic;
using UnityEngine;


public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    // 스킬 컴포넌트들 (Inspector에서 연결)
    [Header("스킬 컴포넌트 연결")]
    //[SerializeField] private SkillShooter skillShooter;
    //[SerializeField] private SkillAoe skillAoe;
    // 새 스킬 종류 추가 시 여기에 필드 추가

    public event System.Action<string, int> OnSkillLevelChanged;

    public void Awake()
    {
        if (instance != null && instance != this){ Destroy(gameObject); return;}
        instance = this;

        // SO 목록 순회 컴포넌트 동적 생성
        foreach (var data in skillDataList)
        {
            if (data == null) continue;

            // SO가 직접 컴포넌트를 생성 (팩토리 패턴)
            SkillBase skill = data.CreateSkill(gameObject);

            if (skills.ContainsKey(data.skillName))
            {
                Debug.LogWarning($"[PlayerTower] 중복 스킬 이름 '{data.skillName}' — 무시됨");
                Destroy(skill);
                continue;
            }

            skills[data.skillName] = skill;
        }

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
