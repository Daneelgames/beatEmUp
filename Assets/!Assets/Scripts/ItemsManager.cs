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
}

