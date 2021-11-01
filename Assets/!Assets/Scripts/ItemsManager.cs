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
}

