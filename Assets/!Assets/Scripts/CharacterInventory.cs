using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInventory : MonoBehaviour
{
    [SerializeField] private List<ItemInInventory> itemsInInventory;

    public List<ItemInInventory> ItemsInInventory
    {
        get => itemsInInventory;
        set => itemsInInventory = value;
    }

    public void CharacterPicksUpItem(int itemIndex)
    {
        for (int i = 0; i < ItemsInInventory.Count; i++)
        {
            if (ItemsInInventory[i].itemIndex == itemIndex && ItemsManager.Instance.CanAddAnotherOne(ItemsInInventory[i].itemIndex, ItemsInInventory[i].amount))
            {
                ItemsInInventory[i].amount++;
                return;
            }
        }
        
        ItemsInInventory.Add(new ItemInInventory(itemIndex, 1));
    }

    public void CharacterLosesItem(int itemIndex)
    {
        for (int i = 0; i < ItemsInInventory.Count; i++)
        {
            if (ItemsInInventory[i].itemIndex == itemIndex && ItemsManager.Instance.CanAddAnotherOne(ItemsInInventory[i].itemIndex, ItemsInInventory[i].amount))
            {
                ItemsInInventory[i].amount--;
                if (ItemsInInventory[i].amount <= 0)
                    ItemsInInventory.RemoveAt(i);
                return;
            }
        }
    }

    public void InventorySlotClicked(int inventorySlotIndex)
    {
        
    }
}

[Serializable]
public class ItemInInventory
{
    public int itemIndex = 0;
    public int amount = 0;

    public ItemInInventory (int newIndex, int newAmount)
    {
        itemIndex = newIndex;
        amount = newAmount;
    }
}