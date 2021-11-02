using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiButton : MonoBehaviour
{
    [Header("SLOT")] [SerializeField]
    private int inventorySlotIndex = -1;
    
    [Header("ACTION")] [SerializeField]
    private int actionIndex = -1;
    
    public void SelectButton()
    {
        // THIS SHIT IS BAD

        int newIndex = -1;
        if (inventorySlotIndex != -1)
            newIndex = PartyInputManager.Instance.SelectedAllyUnits[0].Inventory.ItemsInInventory[inventorySlotIndex].itemIndex;
        PartyUi.Instance.SelectInventorySlot(true, newIndex);
    }
    
    public void UnselectButton()
    {
        // THIS SHIT IS BAD
        int newIndex = -1;
        if (inventorySlotIndex != -1)
            newIndex = PartyInputManager.Instance.SelectedAllyUnits[0].Inventory.ItemsInInventory[inventorySlotIndex].itemIndex;
        PartyUi.Instance.SelectInventorySlot(false, newIndex);
    }

    public void ButtonPressed()
    {
        if (inventorySlotIndex >= 0)
        {
            // inventory slot pressed
            PartyUi.Instance.InventorySlotClicked(PartyInputManager.Instance.SelectedAllyUnits[0].Inventory.ItemsInInventory[inventorySlotIndex].itemIndex, transform.position);
        }
        else if (actionIndex >= 0)
        {
            ActionsDropDownMenu.Instance.ActionClicked(actionIndex);
        }
        else
        {
            ActionsDropDownMenu.Instance.CloseDropDownMenu();
        }
    }
}