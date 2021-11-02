using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class InteractionController : MonoBehaviour
{
    public bool HighlightClosestInteractable = false;
    [SerializeField] private HealthController hc;
    public LayerMask layerMask;
    public float interactionDistance = 2;

    public ParticleSystem closestInteractableFeedbackPrefab;

    private ParticleSystem closestInteractableFeedback;
    public  ParticleSystem ClosestInteractableFeedback => closestInteractableFeedback;
    
    private ParticleSystem.EmissionModule closestInteractableFeedbackEmissionModule;
    public ParticleSystem.EmissionModule ClosestInteractableFeedbackEmissionModule => closestInteractableFeedbackEmissionModule;

    private Collider[] interactableColliders;

    private Interactable interactableToInteract;
    private bool aiming = false;
    void Start()
    {
        if (HighlightClosestInteractable)
        {
            closestInteractableFeedback = Instantiate(closestInteractableFeedbackPrefab, transform.position, Quaternion.identity);
            closestInteractableFeedbackEmissionModule = closestInteractableFeedback.emission;
        }
        
        StartCoroutine(UpdateInteraction());
    }

    public void StartAiming()
    {
        aiming = true;
        closestInteractableFeedbackEmissionModule.rateOverTime = 10;
    }
    
    public void StopAiming()
    {
        closestInteractableFeedbackEmissionModule.rateOverTime = 0;
        aiming = false;
    }

    public void SetInteractableToInteract(Interactable newInteractable)
    {
        interactableToInteract = newInteractable;
    }

    IEnumerator UpdateInteraction()
    {
        // pick up the item and put it in a weapon BodyPartsManager.WeaponParentTransform
        float t = Random.Range(0.001f, 0.1f * GameManager.Instance.Units.IndexOf(hc));
        yield return new WaitForSeconds(t);
        while (true)
        {
            yield return null;
            
            if (aiming)
                continue;
            
            interactableColliders = Physics.OverlapSphere(transform.position, interactionDistance, layerMask);
            Vector3 closestPoint = transform.position;
            Interactable closestInteractable = null;
            float distance = 100;
            for (int i = 0; i < interactableColliders.Length; i++)
            {
                var tempClosestPoint = interactableColliders[i].ClosestPoint(transform.position);
                float newDistance = Vector3.Distance(transform.position, tempClosestPoint);
                
                if (newDistance < distance)
                {
                    closestPoint = tempClosestPoint;
                    int newInt = SpawnController.Instance.InteractablesGameObjects.IndexOf(interactableColliders[i].gameObject);
                    
                    if (newInt == -1 || SpawnController.Instance.Interactables.Count <= newInt)
                        break; 
                            
                    var foundInteractable = SpawnController.Instance.Interactables[newInt];

                    if (foundInteractable != null && foundInteractable.CanBeInteractedBy(hc))
                    {
                        closestInteractable = foundInteractable;
                        distance = newDistance;
                    }
                }
            }

            if (closestInteractable)
            {
                if (HighlightClosestInteractable)
                {
                    closestInteractableFeedback.transform.position = closestPoint;
                    closestInteractableFeedbackEmissionModule.rateOverTime = 10;
                }

                if (Input.GetButtonDown("Interact") && hc.PlayerInput)
                    Interact(closestInteractable);

                if (interactableToInteract != null && interactableToInteract == closestInteractable)
                {
                    // picks up weapon if has none and if weapon has no owner
                    if (closestInteractable.WeaponPickUp &&
                        closestInteractable.WeaponPickUp.AttackManager == null)
                    {
                        Interact(closestInteractable);
                    }
                    else if (closestInteractable.ConsumablePickUp && closestInteractable.ConsumablePickUp.heal)
                    {
                        Interact(closestInteractable);
                    }   
                }
            }
            else
            {
                if (closestInteractableFeedback)
                    closestInteractableFeedbackEmissionModule.rateOverTime = 0;
            }
        }
    }

    void Interact(Interactable interactable)
    {
        if(interactable.CanInteract == false)
            return;
        
        if (interactable.WeaponPickUp)
        {
            interactable.CanInteract = false;
            interactable.ToggleTriggerCollider(true);
            interactable.ToggleRigidbodyKinematicAndGravity(true, false);
            
            GameManager.Instance.SetLayerRecursively(interactable.gameObject, 7);
            
            SpawnController.Instance.Interactables.Remove(interactable);
            SpawnController.Instance.InteractablesGameObjects.Remove(interactable.gameObject);

            hc.Inventory.CharacterPicksUpItem(interactable.IndexInDatabase);
            PartyUi.Instance.CharacterPicksUpInteractable(hc,interactable);
            
            if (hc.AttackManager.WeaponInHands == null)
                hc.AttackManager.TakeWeaponInHands(interactable);
            else
            {
                Destroy(interactable.gameObject);
            }
        }
        else if (interactable.ConsumablePickUp)
        {
            interactable.CanInteract = false;
            PartyInventory.Instance.PickUpInteractable(hc, interactable);
            hc.Inventory.CharacterPicksUpItem(interactable.IndexInDatabase);
            PartyUi.Instance.CharacterPicksUpInteractable(hc,interactable);
        }
    }
}