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
        AssetSpawner.Instance.Spawn(ItemsDatabase.Items[itemDatabaseIndex].itemPickUpReference, unit.transform.position, Quaternion.identity, AssetSpawner.ObjectType.Item, unit, null, Vector3.zero);
        
        // pick new weapon up in method ProceedNewItem
    }
    
    public void DropItemFromInventory(HealthController unit, int itemDatabaseIndex)
    {
        AssetSpawner.Instance.Spawn(ItemsDatabase.Items[itemDatabaseIndex].itemPickUpReference, 
            unit.transform.position + Vector3.up * 1.5f, Quaternion.identity, 
            AssetSpawner.ObjectType.Item, null, null, Vector3.zero);
        RemoveItemFromCharacter(unit, itemDatabaseIndex);
    }

    public void ThrowItemFromInventory(HealthController unit, int itemDatabaseIndex, Vector3 throwTargetPos)
    {
        AssetSpawner.Instance.Spawn(ItemsDatabase.Items[itemDatabaseIndex].itemPickUpReference, 
            unit.transform.position + Vector3.up * 1.5f, Quaternion.identity, 
            AssetSpawner.ObjectType.Item, null, unit, throwTargetPos);
        RemoveItemFromCharacter(unit, itemDatabaseIndex);
    }           

    void RemoveItemFromCharacter(HealthController unit, int itemDatabaseIndex)
    {
        // if unis is holding weapon - Destroy it
        if (unit.AttackManager.WeaponInHands && unit.AttackManager.WeaponInHands.Interactable.IndexInDatabase == itemDatabaseIndex)
        {
            for (int i = 0; i < unit.Inventory.ItemsInInventory.Count; i++)
            {
                if (unit.Inventory.ItemsInInventory[i].itemIndex == itemDatabaseIndex &&
                    unit.Inventory.ItemsInInventory[i].amount == 1)
                {
                    // single weapon
                    unit.AttackManager.DestroyWeaponInHands(unit.AttackManager.WeaponInHands, true);
                    return;
                }
            }
        }
        // lose item if it wasnt that single weapon in hands
        unit.Inventory.CharacterLosesItem(itemDatabaseIndex);
    }

    public void ProceedNewItem(GameObject spawnedGO, HealthController interactorHc, HealthController throwerHC, Vector3 throwTargetPos)
    {
        var newInteractable = spawnedGO.GetComponent<Interactable>();
        
        if (newInteractable == null)
            return;
        
        if (interactorHc)
            interactorHc.InteractionController.Interact(newInteractable);

        if (throwerHC)
        {
            if (newInteractable.WeaponPickUp)
                newInteractable.WeaponPickUp.SetNewOwner(throwerHC.AttackManager);
            
            newInteractable.Throw(throwerHC.transform.position + Vector3.up * 1.5f, throwTargetPos);
        }
    }
}