using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CharacterGenerator : MonoBehaviour
{
    public static CharacterGenerator Instance;

    [Header("Squads")]
    public bool generateSquads = true;

    private void Awake()
    {
        Instance = this;
    }

    public void GenerateUnit(HealthController hc)
    {
        if (hc.AiInput && hc.AiInput.inParty == false)
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

        // generate squads of allies
        if (generateSquads)
        {
            bool squadFound = false;
            for (int i = 0; i < GameManager.Instance.Units.Count; i++)
            {
                var newHc = GameManager.Instance.Units[i];
                if (newHc == hc)
                    continue;
                
                if (hc.AiInput == null || newHc.AiInput == null || (newHc.AiInput.ally != hc.AiInput.ally))
                    continue;

                if (hc.AiInput.CanJoinGroupOnRuntime && newHc.AiInput.LeaderToFollow == null)
                {
                    if (newHc.AiInput.Leader || newHc.AiInput.CanCreateGroupOnRuntime)
                    {
                        if (newHc.AiInput.FollowersCurrent.Count < newHc.AiInput.FollowersAmountMax)
                        {
                            squadFound = true;
                            SetLeader(newHc, hc);
                        }
                    }
                }
            }

            if (squadFound == false && hc.AiInput && hc.AiInput.CanCreateGroupOnRuntime && hc.AiInput.LeaderToFollow == null)
            {
                SetLeader(hc, null);
            }
        }
    }

    void SetLeader(HealthController leader, HealthController follower)
    {
        leader.AiInput.Leader = true;
        if (follower)
        {
            if (leader.AiInput.FollowersCurrent.Contains(follower) == false)
                leader.AiInput.FollowersCurrent.Add(follower);
            
            follower.AiInput.LeaderToFollow = leader;
        }
    }

    void ChoosePerksForCharacter(HealthController character)
    {
        bool negativeAdded = false;
        
        for (int j = 0; j < 2; j++)
        {
            List<Perk> perksTemp = new List<Perk>();
            
            for (int i = 0; i < PerksDatabaseManager.Instance.PerksInfoData.perks.Count; i++)
            {
                var perk = PerksDatabaseManager.Instance.PerksInfoData.perks[i];
                if (perk.perkType == Perk.PerkType.RandomShouts && (!character.AiInput || character.AiInput.inParty == false))
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