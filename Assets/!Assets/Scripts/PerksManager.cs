using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerksManager : MonoBehaviour
{
    public static PerksManager Instance;
    
    [SerializeField] private PerksInfoData _perksInfoData;
    public PerksInfoData PerksInfoData => _perksInfoData;

    private void Awake()
    {
        Instance = this;
    }
}

[Serializable]
public class Perk
{
    public enum PerkType
    {
        Null, RandomShouts, BadFighter, BadShooter, GoodFighter, GoodShooter, 
        Fast, Slow, WeaponLover, HateMale, HateFemale, MeleeDamageResistBufff, 
        CantBeFoundBySixSense, RangedDamageResistBuff
    }

    public PerkType perkType = PerkType.Null;
    public int perkCost = 1;
    [Tooltip("Perk can't be used in pair with it's Antagonist")]
    public PerkType perkAntagonist = PerkType.Null;
    public string perkName;
    public string perkDescription;
}