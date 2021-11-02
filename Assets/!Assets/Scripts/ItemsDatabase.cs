using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "NewItemsDatabase", menuName = "ScriptableObjects/ItemsDatabase", order = 1)]
public class ItemsDatabase : ScriptableObject
{
    [SerializeField] private List<ItemInDatabase> items;
    public List<ItemInDatabase> Items => items;
}

[Serializable]
public class ItemInDatabase
{
    public int itemIndexInDatabase = -1;
    public string itemName;
    public string itemDescription;
    public AssetReference itemPickUpReference;
    public int maxAmountPerInventory = -1;
    public Sprite itemIcon;
}
