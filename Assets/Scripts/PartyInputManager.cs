using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PartyInputManager : MonoBehaviour
{
    public static PartyInputManager Instance;

    enum UnitOrder
    {
        Move, Attack 
    }

    private UnitOrder _unitOrder = UnitOrder.Move;

    [SerializeField] private float maxDistanceToClosestUnit = 3;
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

        if (Input.GetButtonDown("AttackHotkey"))
        {
            _unitOrder = UnitOrder.Attack;
        }

        if (Input.GetButtonDown("ToggleAggroModeHotkey"))
        {
            AiInput.AggroMode newMode = AiInput.AggroMode.AggroOnSight; 
            for (int i = 0; i < selectedAllyUnits.Count; i++)
            {
                if (selectedAllyUnits[i])
                {
                    if (i == 0)
                    {
                        newMode = selectedAllyUnits[i].AiInput.aggroMode;
                        if (newMode == AiInput.AggroMode.AggroOnSight)
                            newMode = AiInput.AggroMode.AttackIfAttacked;
                        else
                            newMode = AiInput.AggroMode.AggroOnSight;
                    }
                    
                    selectedAllyUnits[i].AiInput.SetAggroMode(newMode);   
                }
            }

            PartyUi.Instance.UpdatePartyAggroMode();
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (selectedAllyUnits.Count <= 0)
                return;
            
            Vector3 newPos = GameManager.Instance.MouseWorldGroundPosition();
            newPos = GameManager.Instance.GetClosestNavmeshPoint(newPos);

            switch (_unitOrder)
            {
                case UnitOrder.Move: 
                    for (int i = 0; i < selectedAllyUnits.Count; i++)
                    {
                        if (selectedAllyUnits[i])
                            selectedAllyUnits[i].AiInput.OrderMove(newPos);
                    }   
                    break;
                case UnitOrder.Attack:
                    float distance = 1000;
                    float newDistance = 0;
                    HealthController closestUnitToAttack = null;
                    for (int i = 0; i < GameManager.Instance.Units.Count; i++)
                    {
                        var unit = GameManager.Instance.Units[i];
                        if (unit.AiInput && unit.AiInput.inParty == false)
                        {
                            newDistance = Vector3.Distance(unit.transform.position, newPos);
                            if (newDistance <= maxDistanceToClosestUnit && newDistance <= distance)
                            {
                                distance = newDistance;
                                closestUnitToAttack = unit;
                            }
                        }
                    }
                    for (int i = 0; i < selectedAllyUnits.Count; i++)
                    {
                        if (selectedAllyUnits[i])
                            selectedAllyUnits[i].AiInput.OrderAttack(newPos, closestUnitToAttack);
                    }   
                    break;
            }

            _unitOrder = UnitOrder.Move;
        }
    }

    void SelectUnit(int index)
    {
        if (Party.Count <= 0)
            return;
        
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