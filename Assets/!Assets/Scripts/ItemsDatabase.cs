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
    [SerializeField] int itemIndexInDatabase = -1;
    [SerializeField] private AssetReference itemPickUpReference;
}
