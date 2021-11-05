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

    public void SkillSelected(int index)
    {
        print("SELECTED SKILL " + index);
    }
}