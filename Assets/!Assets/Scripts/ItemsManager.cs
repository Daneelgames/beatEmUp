using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsManager : MonoBehaviour
{
    public static ItemsManager Instance;
    
    [SerializeField] private ItemsDatabase _itemsDatabase;

    public ItemsDatabase ItemsDatabase => _itemsDatabase;

    public void Awake()
    {
        Instance = this;
    }

    public bool CanAddAnotherOne(int index, int currentAmount)
    {
        if (index >= ItemsDatabase.Items.Count || ItemsDatabase.Items[index] == null)
        {
            Debug.LogError("ITEM DATABASE MISSING AN ITEM WITH INDEX " + index);
            return false;
        }
            
        if (ItemsDatabase.Items[index].maxAmountPerInventory == -1 || currentAmount < ItemsDatabase.Items[index].maxAmountPerInventory)
            return true;
            
        return false;
    }

    public void EquipItemFromInventory(HealthController unit, int itemDatabaseIndex)
    {
        // if unis is holding weapon already - move it to the inventory
        
        unit.AttackManager.DestroyWeaponInHands(unit.AttackManager.WeaponInHands, false);
        
        // spawn new weapon from database by index
        unit.Inventory.CharacterLosesItem(itemDatabaseIndex); // this is needed to prevent duping
        AssetSpawner.Instance.Spawn(ItemsDatabase.Items[itemDatabaseIndex].itemPickUpReference, unit.transform.position, Quaternion.identity, AssetSpawner.ObjectType.Item, unit);
        
        // pick new weapon up in method ProceedNewItem
    }

    public void ProceedNewItem(GameObject spawnedGO, HealthController interactor)
    {
        var newInteractable = spawnedGO.GetComponent<Interactable>();
        
        if (interactor && newInteractable)
            interactor.InteractionController.Interact(newInteractable);
    }
}