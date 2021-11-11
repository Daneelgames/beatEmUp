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
        if (currentCaster)
            currentCaster.CharacterSkillsController.SetSelectedSkill(null);
    }
    
    public void SkillSelected(HealthController caster, Skill skill)
    {
        //UnselectSkill();
        currentCaster = caster;
        currentSkill = skill;
        
        if (caster == null)
            return;
        
        
        // check if skill is on cooldown
        if (skill.OnCooldown)
        {
            return;
        }
        
        // check if unit has enough energy
        if (caster.Energy < skill.energyCost)
        {
            PartyUi.Instance.NotEnoughEnergyFeedback();
            return;
        }
        
        currentCaster.CharacterSkillsController.SetSelectedSkill(skill);
        
        switch (skill.skill)
        {
            case Skill.SkillType.DashAttack:
                if (SkillsUi.Instance.State == SkillsUi.SkillsUiState.AimDirectional)
                {
                    UnselectSkill();
                    currentCaster.CharacterSkillsController.SetSelectedSkill(null);
                    currentCaster = null;
                    currentSkill = null;
                    
                    SkillsUi.Instance.AimDirectionalSkill(null, null);
                }
                else
                {
                    SkillsUi.Instance.AimDirectionalSkill(caster, skill);
                }
                break;
        }
    }
}