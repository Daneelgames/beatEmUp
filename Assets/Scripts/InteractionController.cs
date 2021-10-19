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

    public GameObject closestInteractableFeedbackPrefab;
    private GameObject closestInteractableFeedback;
    void Start()
    {
        if (HighlightClosestInteractable)
            closestInteractableFeedback = Instantiate(closestInteractableFeedbackPrefab, transform.position, Quaternion.identity);
        
        StartCoroutine(UpdateInteraction());
    }

    IEnumerator UpdateInteraction()
    {
        // pick up the item and put it in a weapon BodyPartsManager.WeaponParentTransform
        while (true)
        {
            Collider[] interactablesInRadius = Physics.OverlapSphere(transform.position, interactionDistance, layerMask);
            Vector3 closestPoint = transform.position;
            Interactable closestInteractable = null;
            float distance = 100;
            for (int i = 0; i < interactablesInRadius.Length; i++)
            {
                var tempClosestPoint = interactablesInRadius[i].ClosestPoint(transform.position);
                float newDistance = Vector3.Distance(transform.position, tempClosestPoint);
                if (newDistance < distance)
                {
                    closestPoint = tempClosestPoint;
                    var foundInteractable = SpawnController.Instance.Interactables[SpawnController.Instance.InteractablesGameObjects.IndexOf(interactablesInRadius[i].gameObject)];

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
                    if (closestInteractableFeedback.activeInHierarchy == false)
                        closestInteractableFeedback.SetActive(true);
                }
                
                if (Input.GetButtonDown("Interact"))
                    Interact(closestInteractable);
            }
            else
            {
                if (closestInteractableFeedback && closestInteractableFeedback.activeInHierarchy)
                    closestInteractableFeedback.SetActive(false);
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
        }
    }
}