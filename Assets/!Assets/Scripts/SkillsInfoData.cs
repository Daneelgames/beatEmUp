using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
    public float actionTimeMin = 0.1f;
    public int cooldownTime = 15;
    public int energyCost = 10;

    public float minDistance = 2;
    public float maxDistance = 10;

    
    [Header("This value means differentThing for different Skills")]
    [Header("RN it's added to attackManager base dmg as an additional weapon dmg")]
    public float skillPower = 200;
}