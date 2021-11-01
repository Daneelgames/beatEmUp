using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInventory : MonoBehaviour
{
    [SerializeField] private List<ItemInInventory> _itemInInventories;

    public List<ItemInInventory> ItemInInventories
    {
        get => _itemInInventories;
        set => _itemInInventories = value;
    }
}

[Serializable]
public class ItemInInventory
{
    public int itemIndex = 0;
    public int amount = 0;
}