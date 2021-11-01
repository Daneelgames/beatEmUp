using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPerksController : MonoBehaviour
{
    [SerializeField] private int perkPoints = 1;
    [SerializeField] private float currentDiscomfort = 0;
    public float CurrentDiscomfort
    {
        get => currentDiscomfort;
        set => currentDiscomfort = value;
    }

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
        if (characterPerks.Contains(newPerk.perkType))
            return;
        
        characterPerks.Add(newPerk.perkType);
        PerkPoints -= newPerk.perkCost;
        
        StartPerkEffect(newPerk);
    }

    private Coroutine randomShoutsCoroutine;
    IEnumerator RandomShoutsOnRepeat()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(5, 30));
            hc.AiInput.Alert();
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
         case Perk.PerkType.WeaponLover:
             WeaponLover = true;
             break;
         case Perk.PerkType.ScaredByMen:
             ScaredByMen = true;
             break;
         case Perk.PerkType.ScaredByLadies:
             ScaredByLadies = true;
             break;
         case Perk.PerkType.MeleeDamageResistBuff:
             MeleeDamageResistBuff = true;
             break;
         case Perk.PerkType.StealthMaster:
             StealthMaster = true;
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
    
    private bool _scaredByMen = false;
    public bool ScaredByMen
    {
        get => _scaredByMen;
        set => _scaredByMen = value;
    }
    
    private bool _scaredByLadies = false;
    public bool ScaredByLadies
    {
        get => _scaredByLadies;
        set => _scaredByLadies = value;
    }
    
    private bool meleeDamageResistBuff = false;
    public bool MeleeDamageResistBuff
    {
        get => meleeDamageResistBuff;
        set => meleeDamageResistBuff = value;
    }
    
    private bool _stealthMaster = false;
    public bool StealthMaster
    {
        get => _stealthMaster;
        set => _stealthMaster = value;
    }
    
    private bool rangedDamageResistBuff = false;
    public bool RangedDamageResistBuff
    {
        get => rangedDamageResistBuff;
        set => rangedDamageResistBuff = value;
    }
    #endregion

    public void SetDiscomfort(float hateDiscomfort)
    {
        CurrentDiscomfort = hateDiscomfort;
    }
}