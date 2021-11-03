using System;
using System.Collections;
using System.Collections.Generic;
using ExternalPropertyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public class Interactable : MonoBehaviour
{
    [Header("Index Should Be Static")] 
    [SerializeField] int indexInDatabase = -1;
    public int IndexInDatabase => indexInDatabase;
    
    [Header("SetInRuntime")]


    [SerializeField] private HealthController interactableOwner;

    [Header("Links")] 
    [SerializeField] private ObjectInfoData _objectInfoData;

    public ObjectInfoData ObjectInfoData => _objectInfoData;
    
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider _collider;
    [SerializeField] private Weapon weaponPickUp;
    [SerializeField] private Consumable consumablePickUp;

    public Weapon WeaponPickUp => weaponPickUp;
    public Consumable ConsumablePickUp => consumablePickUp;
    [SerializeField] private GameObject light;
    [SerializeField] private Vector3 lightPositionOffset = new Vector3(0, 2, 0);

    private bool canInteract = true;
        public bool CanInteract
    {
        get { return canInteract; }
        set { canInteract = value; }
    }
    IEnumerator Start()
    {
        SpawnController.Instance.AddInteractable(this);
        if (light == null)
            yield break;
        
        yield return new WaitForSeconds(Random.Range(0, 2f));

        while (true)
        {
            light.transform.position = transform.position + lightPositionOffset;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void ToggleLight(bool active)
    {
        if (light)
            light.SetActive(active);
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

    public void Throw(Vector3 throwOrigin, Vector3 throwTargetPos)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        //rb.AddForceAtPosition((throwTargetPos - throwOrigin) * 5, throwOrigin, ForceMode.Impulse);
        
        if (WeaponPickUp)
            WeaponPickUp.Throw(throwTargetPos);
    }
    
    private void OnDestroy()
    {
        SpawnController.Instance.InteractableDestroyed(this);
        if (light)
            Destroy(light);
    }
}