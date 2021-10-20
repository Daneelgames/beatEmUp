using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class InteractionController : MonoBehaviour
{
    public bool HighlightClosestInteractable = false;
    [SerializeField] private HealthController hc;
    public LayerMask layerMask;
    public float interactionDistance = 2;

    public ParticleSystem closestInteractableFeedbackPrefab;
    private ParticleSystem closestInteractableFeedback;
    private ParticleSystem.EmissionModule closestInteractableFeedbackEmissionModule;
    [SerializeField]
    private Collider[] interactableColliders;
    void Start()
    {
        if (HighlightClosestInteractable)
        {
            closestInteractableFeedback = Instantiate(closestInteractableFeedbackPrefab, transform.position, Quaternion.identity);
            closestInteractableFeedbackEmissionModule = closestInteractableFeedback.emission;
        }
        
        StartCoroutine(UpdateInteraction());
    }

    IEnumerator UpdateInteraction()
    {
        // pick up the item and put it in a weapon BodyPartsManager.WeaponParentTransform
        while (true)
        {
            interactableColliders = Physics.OverlapSphere(transform.position, interactionDistance, layerMask);
            Vector3 closestPoint = transform.position;
            Interactable closestInteractable = null;
            float distance = 100;
            for (int i = 0; i < interactableColliders.Length; i++)
            {
                var tempClosestPoint = interactableColliders[i].ClosestPoint(transform.position);
                print(tempClosestPoint);
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
            }
            else
            {
                if (closestInteractableFeedback)
                    closestInteractableFeedbackEmissionModule.rateOverTime = 0;
            }
            
            yield return null;
        }
    }

    void Interact(Interactable interactable)
    {
        if (interactable.WeaponPickUp)
        {
            interactable.ToggleTriggerCollider(true);
            interactable.ToggleRigidbodyKinematicAndGravity(true, false);
            hc.AttackManager.PickWeapon(interactable);
            GameManager.Instance.SetLayerRecursively(interactable.gameObject, 7);
            
            SpawnController.Instance.Interactables.Remove(interactable);
            SpawnController.Instance.InteractablesGameObjects.Remove(interactable.gameObject);
        }
    }
}