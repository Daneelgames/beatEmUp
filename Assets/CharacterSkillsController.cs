using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSkillsController : MonoBehaviour
{
    [SerializeField] private List<Skill.SkillType> characterSkills = new List<Skill.SkillType>();

    public List<Skill.SkillType> CharacterSkills
    {
        get => characterSkills;
        set => characterSkills = value;
    }

}
