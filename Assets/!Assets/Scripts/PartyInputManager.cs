using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class PartyInputManager : MonoBehaviour
{
    public static PartyInputManager Instance;

    [SerializeField] private float maxDistanceToClosestUnit = 3;
    [SerializeField] List<HealthController> selectedAllyUnits = new List<HealthController>();
    public List<HealthController> SelectedAllyUnits
    {
        get => selectedAllyUnits;
        set => selectedAllyUnits = value;
    } 
    
    [SerializeField] List<HealthController> party = new List<HealthController>();
    public List<HealthController> Party
    {
        get => party;
        set => party = value;
    }

    [SerializeField]private GameObject unitSelectedFeedbackPrefab;
    private List<GameObject> spawnedUnitSelectedFeedbacks = new List<GameObject>();

    private bool observeMode = false;
    private bool cursorOverUI = false;
    private void Awake()
    {
        Instance = this;
    }

    IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);
        CameraController.Instance.MoveCameraToPosition(Party[0].transform.position, Party[0].transform);
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
            if (Party.Count > 0 && Party[0] != null && Party[0].Health > 0)
            {
                SelectUnit(0);
                CameraController.Instance.MoveCameraToPosition(Party[0].transform.position, Party[0].transform);
                PartyUi.Instance.UnitSelected(Party[0]);
            }
            ObserveMode(false);
        }
        if (Input.GetButtonDown("SelectSecondAlly"))
        {
            if (Party.Count > 1 && Party[1] != null && Party[1].Health > 0)
            {
                SelectUnit(1);
                CameraController.Instance.MoveCameraToPosition(Party[1].transform.position, Party[1].transform);
                PartyUi.Instance.UnitSelected(Party[1]);
            }
            ObserveMode(false);
        }
        if (Input.GetButtonDown("SelectThirdAlly"))
        {
            if (Party.Count > 2 && Party[2] != null && Party[2].Health > 0)
            {
                SelectUnit(2);
                CameraController.Instance.MoveCameraToPosition(Party[2].transform.position, Party[2].transform);
                PartyUi.Instance.UnitSelected(Party[2]);
            }
            ObserveMode(false);
        }
        
        if (Input.GetButtonDown("SelectAllAllies"))
        {
            if (Party.Count > 0)
            {
                SelectUnit(-1);
                for (int i = 0; i < Party.Count; i++)
                {
                    if (Party[i].Health > 0)
                    {
                        CameraController.Instance.MoveCameraToPosition(Party[i].transform.position, Party[i].transform);
                        break;
                    }
                }   
                PartyUi.Instance.UnitSelected(SelectedAllyUnits[0]);
            }
            ObserveMode(false);
        }

        if (Input.GetButtonDown("UseMedKitHotkey"))
        {
            if (PartyInventory.Instance.MedKitsAmount <= 0)
                return;
            
            UseMedKit();
            ObserveMode(false);
        }


        if (Input.GetButtonDown("Observe"))
        {
            ObserveMode(!observeMode);
        }
        
        if (Input.GetButtonDown("Inventory"))
        {
            if (SelectedAllyUnits.Count <= 0)
                return;
            
            PartyUi.Instance.ToggleInventoryUI(SelectedAllyUnits[0]);
        }
        
        /*
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
        }*/

        if (Input.GetMouseButtonDown(0))
        {
            if (cursorOverUI)
                return;
            
            ObserveMode(false);
            
            if (SelectedAllyUnits.Count <= 0)
                return;
            
            Vector3 newPos = GameManager.Instance.MouseWorldGroundPosition();
            newPos = GameManager.Instance.GetClosestNavmeshPoint(newPos);

            HealthController closestUnitToAttack = null;
            float distance = 1000;
            float newDistance = 0;
            for (int i = 0; i < GameManager.Instance.Units.Count; i++)
            {
                var unit = GameManager.Instance.Units[i];
                if (unit.Health > 0 && unit.AiInput && unit.AiInput.inParty == false)
                {
                    newDistance = Vector3.Distance(unit.transform.position, newPos);
                    if (newDistance <= maxDistanceToClosestUnit && newDistance <= distance)
                    {
                        distance = newDistance;
                        closestUnitToAttack = unit;
                    }
                }
            }

            if (closestUnitToAttack)
            { 
                // ATTACK ORDER
                for (int i = 0; i < SelectedAllyUnits.Count; i++)
                {
                    if (SelectedAllyUnits[i] && SelectedAllyUnits[i].AiInput)
                    {
                        SelectedAllyUnits[i].AiInput.SetAggroMode(AiInput.AggroMode.AggroOnSight);
                        SelectedAllyUnits[i].AiInput.OrderAttack(newPos, closestUnitToAttack);
                        PartyUi.Instance.AttackOrderFeedback(newPos);
                    }
                }
            }
            else
            {
                for (int i = 0; i < SelectedAllyUnits.Count; i++)
                {
                    Interactable interactableToInteract = SpawnController.Instance.GetClosestInteractable(newPos, SelectedAllyUnits[i].InteractionController.interactionDistance);

                    Vector3 tempPose = newPos;
                    if (SelectedAllyUnits[i])
                    {
                        switch (i)
                        {
                            case 0: 
                                break;
                            case 1: 
                                tempPose += Vector3.forward * 2;
                                break;
                            case 2: 
                                tempPose += Vector3.right * 2;
                                break;
                            case 3: 
                                tempPose += Vector3.forward * -2;
                                break;
                            case 4: 
                                tempPose += Vector3.right * -2;
                                break;
                            default:
                                tempPose += new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
                                break;
                        }

                        SelectedAllyUnits[i].AiInput.SetAggroMode(AiInput.AggroMode.AttackIfAttacked);
                        if (interactableToInteract == null)
                        {
                            PartyUi.Instance.MoveOrderFeedback(newPos);
                        }
                        else
                        {
                            PartyUi.Instance.InteractOrderFeedback(interactableToInteract);
                            SelectedAllyUnits[i].InteractionController.SetInteractableToInteract(interactableToInteract);
                        }
                        
                        SelectedAllyUnits[i].AiInput.OrderMove(tempPose);
                    }
                }   
            }
        }
    }
    
    public void ObserveMode(bool active)
    {
        observeMode = active;

        if (observeMode)
        {
            if (observeCoroutine == null)
                observeCoroutine = StartCoroutine(ObserveCoroutine());
            
            UniversalCursorController.Instance.SetObserveCursor();
        }
        else
        {
            if (observeCoroutine != null)
            {
                StopCoroutine(observeCoroutine);
                observeCoroutine = null;
            }
            UniversalCursorController.Instance.SetDefaultCursor();
            PartyUi.Instance.UpdateObservableInfo(null);
        }
    }

    private Coroutine observeCoroutine;
    private Observable lastUpdatedObservable;
    IEnumerator ObserveCoroutine()
    {
        Observable closestObjectWithInfo = null;
        lastUpdatedObservable = null;
        while (true)
        {
            float distance = 1000;
            float newDistance = 0;

            int t = 0;
            Vector3 mouseWorldPositionTemp = GameManager.Instance.MouseWorldGroundPosition();
            
            for (int i = GameManager.Instance.ObservablesInRunTime.Count - 1; i >= 0; i--)
            {
                if (GameManager.Instance.ObservablesInRunTime[i] == null)
                {
                    GameManager.Instance.ObservablesInRunTime.RemoveAt(i);
                    continue;
                }
                
                if (GameManager.Instance.ObservablesInRunTime[i].Interactable && 
                    GameManager.Instance.ObservablesInRunTime[i].Interactable.WeaponPickUp && 
                    GameManager.Instance.ObservablesInRunTime[i].Interactable.WeaponPickUp.AttackManager)
                    continue;
                if (GameManager.Instance.ObservablesInRunTime[i].HealthController && 
                    GameManager.Instance.ObservablesInRunTime[i].HealthController.Health <= 0)
                    continue;
                
                newDistance = Vector3.Distance(GameManager.Instance.ObservablesInRunTime[i].transform.position, mouseWorldPositionTemp);

                if (newDistance <= 5 && newDistance < distance)
                {
                    distance = newDistance;
                    closestObjectWithInfo = GameManager.Instance.ObservablesInRunTime[i];
                }
                
                t++;
                if (t >= 10)
                {
                    t = 0;
                    mouseWorldPositionTemp = GameManager.Instance.MouseWorldGroundPosition();
                    yield return null;
                }
            }
            if (lastUpdatedObservable != closestObjectWithInfo)
            {
                PartyUi.Instance.UpdateObservableInfo(closestObjectWithInfo);
                lastUpdatedObservable = closestObjectWithInfo;
            }
        }
    }
    
    void UseMedKit()
    {
        int healthLowest = 100000;
        HealthController unitWithLowestHp = null;
        for (int i = 0; i < SelectedAllyUnits.Count; i++)
        {
            if (SelectedAllyUnits[i].Health == SelectedAllyUnits[i].HealthMax)
                continue;
                
            if (SelectedAllyUnits[i].Health < healthLowest)
            {
                healthLowest = SelectedAllyUnits[i].Health;
                unitWithLowestHp = SelectedAllyUnits[i];
            }
        }

        if (unitWithLowestHp == null)
        {
            // dont waste
            return;
        }

        unitWithLowestHp.Heal(unitWithLowestHp.HealthMax * 0.66f);
        PartyInventory.Instance.MedKitsAmount--;
        PartyUi.Instance.UpdateMedKits();
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
            SelectedAllyUnits.Clear();
            for (int i = Party.Count - 1; i >= 0; i--)
            {
                if (i >= Party.Count)
                    break;
                
                if (Party[i] == null || Party[i].Health <= 0)
                {
                    continue;
                }

                SelectedAllyUnits.Add(Party[i]);
            }

            for (int i = 0; i < SelectedAllyUnits.Count; i++)
            {
                FeedbackOnSelectedUnits(i, SelectedAllyUnits[i].transform, true);
            }
        }
        else if (Party.Count > index)
        {
            SelectedAllyUnits.Clear();
            
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
                SelectedAllyUnits.Add(Party[index]);
            
            FeedbackOnSelectedUnits(0, SelectedAllyUnits[0].transform, true);
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

    public void SetCursorOverUI(bool _cursorOverUI)
    {
        cursorOverUI = _cursorOverUI;
    }
}