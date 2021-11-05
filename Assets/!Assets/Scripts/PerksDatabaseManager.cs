using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerksDatabaseManager : MonoBehaviour
{
    public static PerksDatabaseManager Instance;
    
    [SerializeField] private PerksInfoData _perksInfoData;
    public PerksInfoData PerksInfoData => _perksInfoData;

    private void Awake()
    {
        Instance = this;
    }

    public Perk GetPerkFromPerkType(Perk.PerkType perkType)
    {
        for (int i = 0; i < PerksInfoData.perks.Count; i++)
        {
            if (PerksInfoData.perks[i].perkType == perkType)
            {
                return PerksInfoData.perks[i];
            }
        }
        return null;
    }
}

[Serializable]
public class Perk
{
    public enum PerkType
    {
        Null, RandomShouts, BadFighter, BadShooter, GoodFighter, GoodShooter, 
        Fast, Slow, WeaponLover, ScaredByMen, ScaredByLadies, MeleeDamageResistBuff, 
        StealthMaster, RangedDamageResistBuff
    }

    public PerkType perkType = PerkType.Null;
    public int perkCost = 1;
    [Tooltip("Perk can't be used in pair with it's Antagonist")]
    public PerkType perkAntagonist = PerkType.Null;
    public string perkName;
    public string perkDescription;
}