using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CharacterGenerator : MonoBehaviour
{
    public static CharacterGenerator Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void GenerateUnit(HealthController hc)
    {
        if (hc.AiInput.inParty == false)
        {
            // random sex
            float r = Random.value;
            if (r <= 0.4f)
                hc._Sex = HealthController.Sex.Female;
            else if (r <= 0.8f)
                hc._Sex = HealthController.Sex.Male;
            else
                hc._Sex = HealthController.Sex.Unknown;
        }
                
        ChoosePerksForCharacter(hc);
    }

    void ChoosePerksForCharacter(HealthController character)
    {
        bool negativeAdded = false;
        
        for (int j = 0; j < 2; j++)
        {
            List<Perk> perksTemp = new List<Perk>();
            
            for (int i = 0; i < PerksManager.Instance.PerksInfoData.perks.Count; i++)
            {
                var perk = PerksManager.Instance.PerksInfoData.perks[i];
                if (perk.perkType == Perk.PerkType.RandomShouts && character.AiInput.inParty == false)
                {
                    if (Random.value > 0.1f)
                        continue;
                }
                
                if (perk.perkCost <= character.CharacterPerksController.PerkPoints)
                {
                    if (character.CharacterPerksController.CharacterPerks.Contains(perk.perkType) || 
                        character.CharacterPerksController.CharacterPerks.Contains(perk.perkAntagonist) || 
                        (negativeAdded && perk.perkCost < 0))
                        continue;
                }
                perksTemp.Add(perk);
            }

            if (perksTemp.Count <= 0)
                return;
            
            Perk resultPerk = perksTemp[Random.Range(0, perksTemp.Count)];
            if (resultPerk.perkCost < 0)
                negativeAdded = true;
            
            character.CharacterPerksController.AddPerk(resultPerk);
        }
    }
}