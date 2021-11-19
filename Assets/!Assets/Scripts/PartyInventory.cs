using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyInventory : MonoBehaviour
{
    public static PartyInventory Instance;
    private int _medKitsAmount = 0;

    public int MedKitsAmount
    {
        get => _medKitsAmount;
        set => _medKitsAmount = value;
    }

    void Start()
    {
        Instance = this;
    }

    private Coroutine pickInteractableCoroutine;

    public void PickUpInteractable(HealthController hc, Interactable interactable)
    {
        if ((hc && hc.AiInput && hc.AiInput.inParty == false) || pickInteractableCoroutine != null)
            return;

        pickInteractableCoroutine = StartCoroutine(PickInteractableWithDelay(hc, interactable));
    }

    IEnumerator PickInteractableWithDelay(HealthController hc, Interactable interactable)
    {
        yield return new WaitForSeconds(0.1f);
        
        if (interactable.IndexInDatabase == 0)
        {
            MedKitsAmount++;   
            PartyUi.Instance.UpdateMedKits();
        }
        
        if (interactable.WeaponPickUp)
        {
            interactable.ToggleTriggerCollider(true);
            interactable.ToggleRigidbodyKinematicAndGravity(true, false);
            
            GameManager.Instance.SetLayerRecursively(interactable.gameObject, 7);
            
            SpawnController.Instance.Interactables.Remove(interactable);
            SpawnController.Instance.InteractablesGameObjects.Remove(interactable.gameObject);

            var weaponInDatabase = ItemsDatabaseManager.Instance.ItemsDatabase.Items[interactable.IndexInDatabase];
            if (hc && hc.AttackManager.WeaponInHands == null && (weaponInDatabase.itemTypes.Contains(ItemInDatabase.ItemType.MeleeWeapon) || weaponInDatabase.itemTypes.Contains(ItemInDatabase.ItemType.RangedWeapon)))
                hc.AttackManager.TakeWeaponInHands(interactable);
            else
            {
                Destroy(interactable.gameObject);
            }
        }
        else
        {
            Destroy(interactable.gameObject);   
        }
        pickInteractableCoroutine = null;
    }
}