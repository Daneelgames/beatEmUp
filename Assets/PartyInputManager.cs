using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PartyInputManager : MonoBehaviour
{
    public static PartyInputManager Instance;
    
    [SerializeField] List<HealthController> selectedAllyUnits = new List<HealthController>();
    [SerializeField] List<HealthController> party = new List<HealthController>();

    private void Awake()
    {
        Instance = this;
    }

    public void AddPartyMember(HealthController hc)
    {
        if (party.Contains(hc))
            return;
        
        party.Add(hc);
    }

    private void Update()
    {
        GetInput();
    }

    void GetInput()
    {
        if (Input.GetButtonDown("SelectFirstAlly"))
        {
            SelectUnit(0);
        }
        if (Input.GetButtonDown("SelectSecondAlly"))
        {
            SelectUnit(1);
        }
        if (Input.GetButtonDown("SelectThirdAlly"))
        {
            SelectUnit(2);
        }
        if (Input.GetButtonDown("SelectAllAllies"))
        {
            SelectUnit(-1);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (selectedAllyUnits.Count <= 0)
                return;
            
            Vector3 newPos = GameManager.Instance.MouseWorldGroundPosition();
            newPos = GameManager.Instance.GetClosestNavmeshPoint(newPos);
            
            for (int i = selectedAllyUnits.Count - 1; i >= 0; i--)
            {
                if (selectedAllyUnits[i])
                    selectedAllyUnits[i].AiInput.OrderMove(newPos);
            }
        }
    }

    void SelectUnit(int index)
    {
        if (index == -1)
        {
            // select all units
            selectedAllyUnits.Clear();
            for (int i = party.Count - 1; i >= 0; i--)
            {
                if (i >= party.Count)
                    break;
                
                if (party[i] == null || party[i].Health <= 0)
                {
                    party.RemoveAt(i);
                    continue;
                }

                selectedAllyUnits.Add(party[i]);
            }
        }
        else if (party.Count > index)
        {
            selectedAllyUnits.Clear();
            
            for (int i = party.Count - 1; i >= 0; i--)
            {
                
                if (i >= party.Count)
                    break;
                
                if (party[i] == null || party[i].Health <= 0)
                {
                    party.RemoveAt(i);
                    continue;
                }
            }
            
            if (party[index] && party[index].Health > 0)
                selectedAllyUnits.Add(party[index]);
        }
    }
}