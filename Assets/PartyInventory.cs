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

    public void PickUpConsumable(Interactable consumableInteractable)
    {
        if (consumableInteractable.ConsumablePickUp.heal)
        {
            MedKitsAmount++;   
            PartyUi.Instance.UpdateMedKits();
        }
    }
}