using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInventoryUi : MonoBehaviour
{
    [SerializeField] private Text unitNameText;
    [SerializeField] private List<SlotUI> _slotUis = new List<SlotUI>();
    private HealthController unit;

    [Header("For Editor Only")] 
    [SerializeField] private int slotsAmount = 10;
    [SerializeField] private Transform slotsParent;
    [SerializeField] private Transform slotTemplate;
    
    public HealthController Unit
    {
        get => unit;
        set => unit = value;
    }
    
    [ContextMenu("InitInventoryUI")]
    public void InitInventoryUI()
    {
        for (int i = _slotUis.Count - 1; i >= 0; i--)
        {
            if (_slotUis[i].amount.transform.parent == slotTemplate)
                continue;
            
            DestroyImmediate(_slotUis[i].amount.transform.parent.gameObject);    
        }
        _slotUis.Clear();
        
        NewSlotUI(slotTemplate.gameObject);
        
        for (int i = 1; i < slotsAmount; i++)
        {
            var newSlot = Instantiate(slotTemplate, slotsParent);
            newSlot.name = "InventorySlot" + i;
            NewSlotUI(newSlot.gameObject);
        }
    }

    void NewSlotUI(GameObject slotGo)
    {
        _slotUis.Add(new SlotUI());
        _slotUis[_slotUis.Count-1].itemIcon = slotGo.GetComponentInChildren<Image>();
        _slotUis[_slotUis.Count-1].amount = slotGo.GetComponentInChildren<Text>();
    }
    
    public void UpdateInventoryUI(HealthController _unit)
    {
        var inventory = _unit.Inventory;
        Unit = _unit;
        
        unitNameText.text = Unit.ObjectInfoData.objectName;

        for (int i = 0; i < _slotUis.Count; i++)
        {
            if (i >= Unit.Inventory.ItemsInInventory.Count)
            {
                _slotUis[i].amount.transform.parent.gameObject.SetActive(false);
                continue;
            }

            var newItem = ItemsDatabaseManager.Instance.ItemsDatabase.Items[Unit.Inventory.ItemsInInventory[i].itemIndex];
            _slotUis[i].itemIcon.sprite = newItem.itemIcon;
            if (Unit.Inventory.ItemsInInventory[i].amount == 1)
                _slotUis[i].amount.text = String.Empty;
            else
                _slotUis[i].amount.text = Unit.Inventory.ItemsInInventory[i].amount.ToString();
            
            _slotUis[i].amount.transform.parent.gameObject.SetActive(true);
        }
    }
}

[Serializable]
public class SlotUI
{
    public Image itemIcon;
    public Text amount;
}