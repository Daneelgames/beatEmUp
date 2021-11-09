using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillsDatabase", menuName = "ScriptableObjects/SkillsDatabase", order = 1)]
public class SkillsInfoData : ScriptableObject
{
    [Header("ПРОБЕЛЫ И ТОЧКИ НА КОНЦЕ ФРАЗЫ В КОДЕ")]
    public List<Skill> skills;
}

[Serializable]
public class Skill
{
    public enum SkillType
    {
        Null, DashAttack, SlowAoe, RegenAoe
    }

    public SkillType skill;
    
    public string skillName = "Skill";
    public string skillDescription = "Unknown";
    public Sprite skillIcon;
    public float actionTime = 3;
    public int cooldownTime = 15;
    public int energyCost = 10;

    public float maxDistance = 10;

    [Header("This value means differentThing for different Skills")]
    public float skillPower = 10;
}