using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInventory : MonoBehaviour
{
    [SerializeField] private List<ItemInInventory> itemsInInventories;

    public List<ItemInInventory> ItemsInInventories
    {
        get => itemsInInventories;
        set => itemsInInventories = value;
    }

    public void CharacterPicksUpItem(int itemIndex)
    {
        for (int i = 0; i < ItemsInInventories.Count; i++)
        {
            if (ItemsInInventories[i].itemIndex == itemIndex && ItemsManager.Instance.CanAddAnotherOne(ItemsInInventories[i].itemIndex, ItemsInInventories[i].amount))
            {
                ItemsInInventories[i].amount++;
                return;
            }
        }
        
        ItemsInInventories.Add(new ItemInInventory(itemIndex, 1));
    }

    public void CharacterLosesItem(int itemIndex)
    {
        for (int i = 0; i < ItemsInInventories.Count; i++)
        {
            if (ItemsInInventories[i].itemIndex == itemIndex && ItemsManager.Instance.CanAddAnotherOne(ItemsInInventories[i].itemIndex, ItemsInInventories[i].amount))
            {
                ItemsInInventories[i].amount--;
                if (ItemsInInventories[i].amount <= 0)
                    ItemsInInventories.RemoveAt(i);
                return;
            }
        }
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