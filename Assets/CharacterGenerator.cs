using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterGenerator : MonoBehaviour
{
    public List<HealthController> unitsToRandomize = new List<HealthController>();

    IEnumerator Start()
    {
        // GENERATE SIMPLE UNITS FOR NOW
        
        int t = 0;
        for (int i = 0; i < unitsToRandomize.Count; i++)
        {
            if (unitsToRandomize[i].AiInput.inParty == false)
            {
                // random sex
                float r = Random.value;
                if (r <= 0.4f)
                    unitsToRandomize[i]._Sex = HealthController.Sex.Female;
                else if (r <= 0.8f)
                    unitsToRandomize[i]._Sex = HealthController.Sex.Male;
                else
                    unitsToRandomize[i]._Sex = HealthController.Sex.Unknown;
            }
                
            ChoosePerksForCharacter(unitsToRandomize[i]);
            
            t++;
            if (t >= 10)
            {
                t = 0;
                yield return null;
            }
        }
    }

    void ChoosePerksForCharacter(HealthController character)
    {
        bool negativeAdded = false;
        
        for (int j = 0; j < 3; j++)
        {
            List<Perk> perksTemp = new List<Perk>();
            
            for (int i = 0; i < PerksManager.Instance.PerksInfoData.perks.Count; i++)
            {
                var perk = PerksManager.Instance.PerksInfoData.perks[i];
                
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