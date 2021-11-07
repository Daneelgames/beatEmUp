using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSkillsController : MonoBehaviour
{
    [SerializeField] private List<Skill> characterSkills = new List<Skill>();
    private Skill currentSkill;
    
    public List<Skill> CharacterSkills
    {
        get => characterSkills;
        set => characterSkills = value;
    }

    public void SetCurrentSkill(Skill skill)
    {
        currentSkill = skill;
    }

    public void UseSkill()
    {
        if (currentSkill != null)
            print(gameObject.name + " TRIES TO USE SKILL " + currentSkill.skillName);

        SkillsDatabaseManager.Instance.UnselectSkill();
    }
}