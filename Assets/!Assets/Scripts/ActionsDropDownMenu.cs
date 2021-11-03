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

    public int CurrentItemDatabaseIndex
    {
        get => currentItemDatabaseIndex;
        set => currentItemDatabaseIndex = value;
    }

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
        CurrentItemDatabaseIndex = itemDatabaseIndex;
        
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
        
        CurrentItemDatabaseIndex = -1;
    }

    public void ActionClicked(int actionIndex)
    {
        if (actionWithItemsCurrent.Count > actionIndex)
        {
            switch (actionWithItemsCurrent[actionIndex])
            {
                case ActionWithItem.Equip:
                    PartyInputManager.Instance.SelectedAllyUnits[0].AiInput.StopBehaviourCoroutines();
                    PartyInputManager.Instance.SelectedAllyUnits[0].AiInput.Idle();

                    ItemsManager.Instance.EquipItemFromInventory(PartyInputManager.Instance.SelectedAllyUnits[0], CurrentItemDatabaseIndex);
                    break;
                
                case ActionWithItem.Consume:
                    PartyInputManager.Instance.SelectedAllyUnits[0].AiInput.StopBehaviourCoroutines();
                    PartyInputManager.Instance.SelectedAllyUnits[0].AiInput.Idle();
                    PartyInputManager.Instance.ConsumeItem(currentItemDatabaseIndex);
                    break;
                
                case ActionWithItem.Throw:
                    /*
                    PartyInputManager.Instance.SelectedAllyUnits[0].AiInput.StopBehaviourCoroutines();
                    PartyInputManager.Instance.SelectedAllyUnits[0].AiInput.Idle();
                    */
                    
                    PartyInputManager.Instance.ThrowMode(true, currentItemDatabaseIndex);
                    break;
                
                case ActionWithItem.Drop:
                    PartyInputManager.Instance.SelectedAllyUnits[0].AiInput.StopBehaviourCoroutines();
                    PartyInputManager.Instance.SelectedAllyUnits[0].AiInput.Idle();
                    
                    ItemsManager.Instance.DropItemFromInventory(PartyInputManager.Instance.SelectedAllyUnits[0], CurrentItemDatabaseIndex);
                    break;
            }
        }
        
        CloseDropDownMenu();
    }
}