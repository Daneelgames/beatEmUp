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
    public List<HealthController> Party
    {
        get => party;
        set => party = value;
    }

    [SerializeField]private GameObject unitSelectedFeedbackPrefab;
    private List<GameObject> spawnedUnitSelectedFeedbacks = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    public void AddPartyMember(HealthController hc)
    {
        if (Party.Contains(hc))
            return;
        
        Party.Add(hc);
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

            if (selectedAllyUnits.Count == 1)
            {
                selectedAllyUnits[0].AiInput.OrderMove(newPos);
            }
            else
            {
                int ggg = -1;
                for (int i = selectedAllyUnits.Count - 1; i >= 0; i--)
                {
                    ggg *= -1;
                    if (selectedAllyUnits[i])
                        selectedAllyUnits[i].AiInput.OrderMove(newPos + selectedAllyUnits[i].transform.forward * (i * ggg));
                }   
            }
        }
    }

    void SelectUnit(int index)
    {
        for (int i = 0; i < spawnedUnitSelectedFeedbacks.Count; i++)
        {
            FeedbackOnSelectedUnits(i, null, false);
        }
        
        if (index == -1)
        {
            // select all units
            selectedAllyUnits.Clear();
            for (int i = Party.Count - 1; i >= 0; i--)
            {
                if (i >= Party.Count)
                    break;
                
                if (Party[i] == null || Party[i].Health <= 0)
                {
                    Party.RemoveAt(i);
                    continue;
                }

                selectedAllyUnits.Add(Party[i]);
            }

            for (int i = 0; i < selectedAllyUnits.Count; i++)
            {
                FeedbackOnSelectedUnits(i, selectedAllyUnits[i].transform, true);
            }
        }
        else if (Party.Count > index)
        {
            selectedAllyUnits.Clear();
            
            for (int i = Party.Count - 1; i >= 0; i--)
            {
                if (i >= Party.Count)
                    break;
                
                if (Party[i] == null || Party[i].Health <= 0)
                {
                    Party.RemoveAt(i);
                    continue;
                }
            }
            
            if (Party[index] && Party[index].Health > 0)
                selectedAllyUnits.Add(Party[index]);
            
            FeedbackOnSelectedUnits(0, selectedAllyUnits[0].transform, true);
        }
    }

    void FeedbackOnSelectedUnits(int index, Transform parent, bool active)
    {
        while (index >= spawnedUnitSelectedFeedbacks.Count)
        {
            var newFeedback = Instantiate(unitSelectedFeedbackPrefab, Vector3.zero, Quaternion.identity);
            newFeedback.SetActive(false);
            spawnedUnitSelectedFeedbacks.Add(newFeedback);    
        }
        
        if (parent)
            spawnedUnitSelectedFeedbacks[index].transform.position = parent.position;
        spawnedUnitSelectedFeedbacks[index].transform.parent = parent;
        spawnedUnitSelectedFeedbacks[index].SetActive(active);
    }
}