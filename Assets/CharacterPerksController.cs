using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPerksController : MonoBehaviour
{
    [SerializeField] private int perkPoints = 1;
    public int PerkPoints
    {
        get => perkPoints;
        set => perkPoints = value;
    }

    [SerializeField] List<Perk.PerkType> characterPerks = new List<Perk.PerkType>();

    public List<Perk.PerkType> CharacterPerks
    {
        get => characterPerks;
        set => characterPerks = value;
    }


    [Header("Links")]

    [SerializeField] private HealthController hc;

    public void AddPerk(Perk newPerk)
    {
        characterPerks.Add(newPerk.perkType);
        PerkPoints -= newPerk.perkCost;
        
        StartPerkEffect(newPerk);
    }

    private Coroutine randomShoutsCoroutine;
    IEnumerator RandomShoutsOnRepeat()
    {
        hc.AiInput.Alert();
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(5, 30));
        }
    }
    
    void StartPerkEffect(Perk newPerk)
    {
        switch (newPerk.perkType)
        {
         case Perk.PerkType.RandomShouts:
             if (randomShoutsCoroutine == null)
                 randomShoutsCoroutine = StartCoroutine(RandomShoutsOnRepeat());
             
             RandomShouts = true;
             break;
         case Perk.PerkType.BadFighter:
             BadFighter = true;
             break;
         case Perk.PerkType.BadShooter:
             BadShooter = true;
             break;
         case Perk.PerkType.GoodFighter:
             GoodFighter = true;
             break;
         case Perk.PerkType.GoodShooter:
             GoodShooter = true;
             break;
         case Perk.PerkType.Fast:
             Fast = true;
             break;
         case Perk.PerkType.Slow:
             Slow = true;
             break;
         
         // here
         case Perk.PerkType.WeaponLover:
             WeaponLover = true;
             break;
         case Perk.PerkType.HateMale:
             HateMale = true;
             break;
         case Perk.PerkType.HateFemale:
             HateFemale = true;
             break;
         case Perk.PerkType.MeleeDamageResistBufff:
             MeleeDamageResistBuff = true;
             break;
         case Perk.PerkType.CantBeFoundBySixSense:
             CantBeFoundBySixSense = true;
             break;
         case Perk.PerkType.RangedDamageResistBuff:
             RangedDamageResistBuff = true;
             break;
        }
    }
    
    #region perk bools

    private bool randomShouts = false;
    public bool RandomShouts
    {
        get => randomShouts;
        set => randomShouts = value;
    }
    
    private bool badFighter = false;
    public bool BadFighter
    {
        get => badFighter;
        set => badFighter = value;
    }
    
    private bool badShooter = false;
    public bool BadShooter
    {
        get => badShooter;
        set => badShooter = value;
    }
    
    private bool goodFighter = false;
    public bool GoodFighter
    {
        get => goodFighter;
        set => goodFighter = value;
    }
    
    private bool goodShooter = false;
    public bool GoodShooter
    {
        get => goodShooter;
        set => goodShooter = value;
    }
    
    private bool fast = false;
    public bool Fast
    {
        get => fast;
        set => fast = value;
    }
    
    private bool slow = false;
    public bool Slow
    {
        get => slow;
        set => slow = value;
    }
    
    private bool weaponLover = false;
    public bool WeaponLover
    {
        get => weaponLover;
        set => weaponLover = value;
    }
    
    private bool hateMale = false;
    public bool HateMale
    {
        get => hateMale;
        set => hateMale = value;
    }
    
    private bool hateFemale = false;
    public bool HateFemale
    {
        get => hateFemale;
        set => hateFemale = value;
    }
    
    private bool meleeDamageResistBuff = false;
    public bool MeleeDamageResistBuff
    {
        get => meleeDamageResistBuff;
        set => meleeDamageResistBuff = value;
    }
    
    private bool cantBeFoundBySixSense = false;
    public bool CantBeFoundBySixSense
    {
        get => cantBeFoundBySixSense;
        set => cantBeFoundBySixSense = value;
    }
    
    private bool rangedDamageResistBuff = false;
    public bool RangedDamageResistBuff
    {
        get => rangedDamageResistBuff;
        set => rangedDamageResistBuff = value;
    }
    #endregion
}