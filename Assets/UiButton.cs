using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiButton : MonoBehaviour
{
    [Header("Inventory Slot Index")] [SerializeField]
    private int inventorySlotIndex = -1;
    
    public void SelectButton()
    {
        // THIS SHIT IS BAD
        int newIndex = PartyInputManager.Instance.SelectedAllyUnits[0].Inventory.ItemsInInventory[inventorySlotIndex].itemIndex;
        PartyUi.Instance.SelectInventorySlot(true, newIndex);
    }
    
    public void UnselectButton()
    {
        // THIS SHIT IS BAD
        int newIndex = PartyInputManager.Instance.SelectedAllyUnits[0].Inventory.ItemsInInventory[inventorySlotIndex].itemIndex;
        PartyUi.Instance.SelectInventorySlot(false, newIndex);
    }

    public void ButtonPressed()
    {
        if (inventorySlotIndex >= 0)
        {
            // inventory slot pressed
            PartyInputManager.Instance.SelectedAllyUnits[0].Inventory.InventorySlotClicked(inventorySlotIndex);
        }
    }
}