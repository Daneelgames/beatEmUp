using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSkillsController : MonoBehaviour
{
    [SerializeField] private List<Skill> characterSkills = new List<Skill>();

    public List<Skill> CharacterSkills
    {
        get => characterSkills;
        set => characterSkills = value;
    }

}
