using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Header("SetInRuntime")]
    [SerializeField] private HealthController interactableOwner;

    [Header("Links")] 
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider _collider;
    [SerializeField] private Weapon weaponPickUp;

    public Weapon WeaponPickUp => weaponPickUp;

    void Start()
    {
        SpawnController.Instance.AddInteractable(this);
    }

    public bool CanBeInteractedBy(HealthController hc)
    {
        bool canBeInteracted = true;

        if (interactableOwner == hc || (interactableOwner && interactableOwner.PlayerInput))
            canBeInteracted = false;

        return canBeInteracted;
    }

    public void ToggleTriggerCollider(bool trigger)
    {
        _collider.isTrigger = trigger;
    }
    public void ToggleRigidbodyKinematicAndGravity(bool kinematic, bool gravity)
    {
        rb.isKinematic = kinematic;
        rb.useGravity = gravity;
    }
}