using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionsDropDownMenu : MonoBehaviour
{
    public static ActionsDropDownMenu Instance;

    enum ActionWithItem
    {
        Equip, Consume, Throw, Drop
    }

    private List<ActionWithItem> actionWithItemsCurrent = new List<ActionWithItem>();
    private int currentItemDatabaseIndex = -1;
    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] private List<Text> actionTexts = new List<Text>();
    public void OpenInventoryItemDropDownMenu(int itemDatabaseIndex, Vector3 newPos)
    {
        bool equipItem = false;
        bool consumeItem = false; 
        bool throwItem = false;
        bool dropItem = false;
        
        actionWithItemsCurrent.Clear();
        transform.position = newPos;
        currentItemDatabaseIndex = itemDatabaseIndex;
        
        var item = ItemsManager.Instance.ItemsDatabase.Items[itemDatabaseIndex];
        for (int i = 0; i < actionTexts.Count; i++)
        {
            if (!equipItem)
            {
                if (item.itemTypes.Contains(ItemInDatabase.ItemType.RangedWeapon) ||
                    item.itemTypes.Contains(ItemInDatabase.ItemType.MeleeWeapon))
                {
                    // EQUIP
                    equipItem = true;
                    actionWithItemsCurrent.Add(ActionWithItem.Equip);
                    actionTexts[i].transform.parent.gameObject.SetActive(true);
                    actionTexts[i].text = item.dropDownActionEquip;
                    continue;
                }   
            }

            if (!consumeItem)
            {
                if (item.itemTypes.Contains(ItemInDatabase.ItemType.Consumable))
                {
                    // CONSUME
                    consumeItem = true;
                    actionWithItemsCurrent.Add(ActionWithItem.Consume);
                    actionTexts[i].transform.parent.gameObject.SetActive(true);
                    actionTexts[i].text = item.dropDownActionConsume;
                    continue;
                }    
            }
            
            if (!throwItem)
            {
                if (item.itemTypes.Contains(ItemInDatabase.ItemType.Throwable))
                {
                    // THROW
                    throwItem = true;
                    actionWithItemsCurrent.Add(ActionWithItem.Throw);
                    actionTexts[i].transform.parent.gameObject.SetActive(true);
                    actionTexts[i].text = item.dropDownActionThrow;
                    continue;
                }  
            }
            if (!dropItem)
            {
                // DROP
                dropItem = true;
                actionWithItemsCurrent.Add(ActionWithItem.Drop);
                actionTexts[i].transform.parent.gameObject.SetActive(true);
                actionTexts[i].text = item.dropDownActionDrop;
                continue;
            }
            
            actionTexts[i].transform.parent.gameObject.SetActive(false);
        }
    }

    public void CloseDropDownMenu()
    {
        for (int i = 0; i < actionTexts.Count; i++)
            actionTexts[i].transform.parent.gameObject.SetActive(false);
        
        currentItemDatabaseIndex = -1;
    }

    public void ActionClicked(int actionIndex)
    {
        
        if (actionWithItemsCurrent.Count > actionIndex)
        {
            switch (actionWithItemsCurrent[actionIndex])
            {
                case ActionWithItem.Equip:
                    ItemsManager.Instance.EquipItemFromInventory(PartyInputManager.Instance.SelectedAllyUnits[0], currentItemDatabaseIndex);
                    break;
                
                case ActionWithItem.Consume:
                    
                    break;
                
                case ActionWithItem.Throw:
                    
                    break;
                
                case ActionWithItem.Drop:
                    ItemsManager.Instance.DropItemFromInventory(PartyInputManager.Instance.SelectedAllyUnits[0], currentItemDatabaseIndex);
                    break;
            }
        }
        
        CloseDropDownMenu();
    }
}