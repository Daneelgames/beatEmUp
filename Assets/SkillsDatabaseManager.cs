using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillsDatabaseManager : MonoBehaviour
{
    public static SkillsDatabaseManager Instance;
    
    [SerializeField] private SkillsInfoData _skillsInfoData;
    public SkillsInfoData SkillsInfoData
    {
        get => _skillsInfoData;
    }

    private HealthController currentCaster;
    private Skill currentSkill;
    
    private void Awake()
    {
        Instance = this;
    }

    public Skill GetSkillFromPerkType(Skill.SkillType skillType)
    {
        for (int i = 0; i < SkillsInfoData.skills.Count; i++)
        {
            if (SkillsInfoData.skills[i].skill == skillType)
            {
                return SkillsInfoData.skills[i];
            }
        }

        return null;
    }

    public void UnselectSkill()
    {
        SkillsUi.Instance.StopAllAiming();
    }
    
    public void SkillSelected(HealthController caster, Skill skill)
    {
        UnselectSkill();
        currentCaster = caster;
        currentSkill = skill;
        
        switch (skill.skill)
        {
            case Skill.SkillType.DashAttack:

                if (SkillsUi.Instance.State == SkillsUi.SkillsUiState.AimDirectional)
                {
                    SkillsUi.Instance.StopAllAiming();
                    currentCaster = null;
                    currentSkill = null;
                }
                
                SkillsUi.Instance.AimDirectionalSkill(caster, skill);
                break;
        }
    }
}