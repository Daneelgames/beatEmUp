using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class PartyUi : MonoBehaviour
{
    public static PartyUi Instance;

    void Awake()
    {
        Instance = this;
    }
    
    [Header("Selected Unit Info")]
    [SerializeField] private Text selectedUnitNameField;
    [SerializeField] private Text selectedUnitInfoField;
    [SerializeField] private Image selectedUnitHpBar;
    [SerializeField] private Image selectedUnitHpBarBack;
    [SerializeField] private Image selectedUnitEnergyBar;
    [SerializeField] private Image selectedUnitEnergyBarBack;
    
    [Header("Misc")]
    [SerializeField] private List<Text> partyNumbersToSelect = new List<Text>();
    [SerializeField] private Text medKitsAmountFeedback;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private Text actionFeedbackText;
    [SerializeField] private Animator actionFeedbackAnim;
    [SerializeField] private Text observableInfoText;
    [SerializeField] private Text observableInfoText2;
    [SerializeField] private Animator observableInfoAnim;
    [SerializeField] private Animator moveOrderFeedbackAnim;
    
    [Header("Cursor")]
    [SerializeField] private Image cursor;
    [SerializeField] private Sprite arrowSprite;
    [SerializeField] private Sprite pickItemSprite;
    [SerializeField] private List<SpriteRenderer> moveOrderFeedbackImages;
    public Image Cursor => cursor;

    [Header("ActiveSkills")] 
    [SerializeField] private List<Button> activeSkillsButtons = new List<Button>();

    [Header("Prefabs")] 
    [SerializeField] private CharacterInventoryUi characterInventoryUiPrefab;
    private List<CharacterInventoryUi> spawnedInventoryUIs = new List<CharacterInventoryUi>();

    public List<CharacterInventoryUi> SpawnedInventoryUIs
    {
        get{return spawnedInventoryUIs;}
        set{spawnedInventoryUIs = value;}
    }

    private static readonly int MoveString = Animator.StringToHash("Move");
    private static readonly int AttackString = Animator.StringToHash("Attack");
    private static readonly int Active = Animator.StringToHash("Active");
    private static readonly int Update = Animator.StringToHash("Update");

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < partyNumbersToSelect.Count; i++)
        {
            partyNumbersToSelect[i].text = (i + 1).ToString();
        }
        
        UpdatePartyAggroMode();
        while (true)
        {
            for (int i = 0; i < PartyInputManager.Instance.Party.Count; i++)
            {
                Vector2 viewportPosition = GameManager.Instance.mainCamera.WorldToViewportPoint(PartyInputManager.Instance.Party[i].transform.position);
                Vector2 worldObject_ScreenPosition = new Vector2(((viewportPosition.x*canvasRect.sizeDelta.x)-(canvasRect.sizeDelta.x*0.5f)), ((viewportPosition.y*canvasRect.sizeDelta.y)-(canvasRect.sizeDelta.y*0.5f)));

                if (partyNumbersToSelect.Count > i && partyNumbersToSelect[i] != null)
                {
                    partyNumbersToSelect[i].rectTransform.anchoredPosition = worldObject_ScreenPosition;
                }
            }

            for (int i = 0; i < partyNumbersToSelect.Count; i++)
            {
                if (partyNumbersToSelect[i].gameObject.activeInHierarchy == false)
                    continue;
                
                if (partyNumbersToSelect.Count > PartyInputManager.Instance.Party.Count ||
                    PartyInputManager.Instance.Party[i].Health <= 0)
                {
                    partyNumbersToSelect[i].gameObject.SetActive(false);
                }
            }

            selectedUnitHpBar.fillAmount = (float)PartyInputManager.Instance.SelectedAllyUnits[0].Health / PartyInputManager.Instance.SelectedAllyUnits[0].HealthMax; 
            selectedUnitEnergyBar.fillAmount = PartyInputManager.Instance.SelectedAllyUnits[0].Energy / PartyInputManager.Instance.SelectedAllyUnits[0].EnergyMax; 
            
            yield return null;
        }
    }

    public void NotEnoughEnergyFeedback()
    {
        StartCoroutine(NotEnoughEnergyFeedbackOverTime());
    }

    IEnumerator NotEnoughEnergyFeedbackOverTime()
    {
        for (int i = 0; i < 5; i++)
        {
            selectedUnitEnergyBarBack.color = new Color(1f, 0f, 0.2f);
            yield return new WaitForSeconds(0.1f);
            selectedUnitEnergyBarBack.color = new Color(0.32f, 0.25f, 0f);
            yield return new WaitForSeconds(0.1f);
        }
        selectedUnitEnergyBarBack.color = new Color(0.32f, 0.25f, 0f);
    }
    public void UseEnergyFeedback()
    {
        StartCoroutine(UseEnergyFeedbackOverTime());
    }

    IEnumerator UseEnergyFeedbackOverTime()
    {
        selectedUnitEnergyBar.color = new Color(1f, 0f, 0.2f);
        yield return new WaitForSeconds(0.2f);
        selectedUnitEnergyBar.color = new Color(1f, 0.91f, 0f);
    }
    public void HpChangedFeedback()
    {
        StartCoroutine(HpChangedFeedbackOverTime());
    }

    IEnumerator HpChangedFeedbackOverTime()
    {
        selectedUnitHpBar.color = new Color(1f, 0f, 0.2f);
        yield return new WaitForSeconds(0.2f);
        selectedUnitHpBar.color = new Color(0f, 1f, 0.74f);
    }

    void LateUpdate()
    {
        cursor.transform.parent.position = Input.mousePosition;
    }
    
    public void UpdatePartyAggroMode()
    {
        return;  
    }

    void SetOrderSprite(Sprite spr)
    {
        for (int i = 0; i < moveOrderFeedbackImages.Count; i++)
        {
            moveOrderFeedbackImages[i].sprite = spr;
        }
    }
    
    public void MoveOrderFeedback(Vector3 newPos)
    {
        SetOrderSprite(arrowSprite);
        moveOrderFeedbackAnim.SetTrigger(MoveString);
        moveOrderFeedbackAnim.transform.position = newPos;
    }
    public void InteractOrderFeedback(Interactable interactable)
    {
        SetOrderSprite(pickItemSprite);
        moveOrderFeedbackAnim.SetTrigger(MoveString);
        moveOrderFeedbackAnim.transform.position = interactable.transform.position;
    }
    public void AttackOrderFeedback(Vector3 newPos)
    {
        SetOrderSprite(arrowSprite);
        moveOrderFeedbackAnim.SetTrigger(AttackString);
        moveOrderFeedbackAnim.transform.position = newPos;
    }
    public void UpdateMedKits()
    {
        medKitsAmountFeedback.text = PartyInventory.Instance.MedKitsAmount + " Medkits";
    }

    public void CharacterPicksUpInteractable(HealthController hc, Interactable interactable)
    {
        if (hc.AiInput.inParty == false)
            return;

        string resultstring = hc.ObjectInfoData.objectGetsItem + " " + interactable.ObjectInfoData.objectName;
        actionFeedbackText.text = resultstring;
        actionFeedbackAnim.SetTrigger(Update);

        if (PartyInputManager.Instance.SelectedAllyUnits.Contains(hc))
        {
            if (SpawnedInventoryUIs.Count > 0 && SpawnedInventoryUIs[0] != null && SpawnedInventoryUIs[0].gameObject.activeInHierarchy)
                SpawnedInventoryUIs[0].UpdateInventoryUI(hc);
        }
    }

    public void UpdateCharacterInventory()
    {
        if (SpawnedInventoryUIs.Count > 0 && SpawnedInventoryUIs[0] != null && SpawnedInventoryUIs[0].gameObject.activeInHierarchy)
            SpawnedInventoryUIs[0].UpdateInventoryUI(PartyInputManager.Instance.SelectedAllyUnits[0]);
    }

    public void CharacterDies(HealthController deadCharacter, HealthController damager)
    {
        /*
        if (deadCharacter.AiInput.inParty == false)
            return;
            */

        string resultstring = String.Empty;
        
        if (damager != null)
            resultstring = damager.ObjectInfoData.objectKills + " " + deadCharacter.ObjectInfoData.objectName;
        else
        {
            resultstring = deadCharacter.ObjectInfoData.objectDies;
        }

        actionFeedbackText.text = resultstring;
        actionFeedbackAnim.SetTrigger(MoveString);
    }
    
    public void CharacterDamaged(HealthController damagedCharacter, HealthController damager)
    {
        if (damagedCharacter.AiInput.inParty == false)
            return;

        string resultstring = String.Empty;
        
        if (damager != null)
            resultstring = damager.ObjectInfoData.objectAttacks + " " + damagedCharacter.ObjectInfoData.objectName;
        else
        {
            resultstring = damagedCharacter.ObjectInfoData.objectIsDamaged;
        }

        actionFeedbackText.text = resultstring;
        actionFeedbackAnim.SetTrigger(MoveString);
    }

    public string UpdateObservableInfo(Observable observable, bool selectedUnit)
    {
        if (observable == null)
        {
            observableInfoAnim.SetBool(Active, false);
        }
        else
        {
            ObjectInfoData objInfo = null;
            
            string nameString = String.Empty;
            string sexString = String.Empty;
            string healthString = String.Empty;
            string perksString = String.Empty;
            string weaponString = String.Empty;

            if (observable.Interactable)
            {
                objInfo = observable.Interactable.ObjectInfoData;   
            }
            else if (observable.HealthController)
            {
                objInfo = observable.HealthController.ObjectInfoData;

                switch (observable.HealthController._Sex)
                {
                    case HealthController.Sex.Unknown:
                        sexString = objInfo.sexUnknown;
                        break;
                    case HealthController.Sex.Male:
                        sexString = objInfo.sexMale;
                        break;
                    case HealthController.Sex.Female:
                        sexString = objInfo.sexFemale;
                        break;
                }

                float healthPercent = observable.HealthController.Health / observable.HealthController.HealthMax;
                if (healthPercent >= 0.66f)
                    healthString = objInfo.healthHigh;
                else if (healthPercent >= 0.33f)
                    healthString = objInfo.healthMid;
                else
                    healthString = objInfo.healthLow;
                
                if (observable.HealthController.CharacterPerksController.CharacterPerks.Count > 0)
                {
                    var perks = observable.HealthController.CharacterPerksController.CharacterPerks;
                    for (int i = 0; i < perks.Count; i++)
                    {
                        var perk = PerksDatabaseManager.Instance.GetPerkFromPerkType(perks[i]);
                        perksString += "\n" + perk.perkName + ": " + perk.perkDescription + ".";
                    }
                }

                var weapon = observable.HealthController.AttackManager.WeaponInHands;
                if (weapon)
                {
                    weaponString += " " + weapon.Interactable.ObjectInfoData.objectName + " " + objInfo.weaponLaysInHisHand + ". ";
                }
            }

            if (!objInfo)
                return null;
            
            nameString = objInfo.objectName + ", " + sexString + ", " + healthString;

            if (!selectedUnit)
            {
                observableInfoText.text = nameString;
                observableInfoText2.text = objInfo.objectSpecialDescription + weaponString + perksString;
                observableInfoAnim.SetBool(Active, true);   
            }
            else
            {
                return sexString + ". " + objInfo.objectSpecialDescription + weaponString + perksString;
            }
        }
        
        return null;
    }
    
    public void ToggleInventoryUI(HealthController unit)
    {
        if (unit == null || unit.Inventory == null)
        {
            for (int i = 0; i < SpawnedInventoryUIs.Count; i++)
            {
                SpawnedInventoryUIs[i].gameObject.SetActive(false);
            }
            return;
        }
        
        CharacterInventoryUi inventoryUiToOpen = null;

        for (int i = 0; i < SpawnedInventoryUIs.Count; i++)
        {
            if (SpawnedInventoryUIs[i].gameObject.activeInHierarchy == false)
            {
                inventoryUiToOpen = SpawnedInventoryUIs[i];
            }
            else if (SpawnedInventoryUIs[i].Unit == unit)
            {
                SpawnedInventoryUIs[i].gameObject.SetActive(false);
                return;
            }
        }

        if (inventoryUiToOpen == null)
        {
            inventoryUiToOpen = Instantiate(characterInventoryUiPrefab, canvasRect);
        }
        
        inventoryUiToOpen.transform.SetSiblingIndex(3);
        inventoryUiToOpen.gameObject.SetActive(true);
        inventoryUiToOpen.UpdateInventoryUI(unit);
        SpawnedInventoryUIs.Add(inventoryUiToOpen);
    }

    public void UnitSelected(HealthController unit)
    {
        if (SpawnedInventoryUIs.Count > 0 && SpawnedInventoryUIs[0] != null && SpawnedInventoryUIs[0].gameObject.activeInHierarchy)
        {
            SpawnedInventoryUIs[0].UpdateInventoryUI(unit);
            UpdateSkillsIcons(unit);

            selectedUnitNameField.text = unit.ObjectInfoData.objectName;
            selectedUnitNameField.text = UpdateObservableInfo(unit.Observable, true);
        }
    }
    
    void UpdateSkillsIcons(HealthController unit)
    {
        for (int i = 0; i < activeSkillsButtons.Count; i++)
        {
            if (i >= unit.CharacterSkillsController.CharacterSkills.Count)
            {
                activeSkillsButtons[i].gameObject.SetActive(false);
                continue;
            }

            var skill = unit.CharacterSkillsController.CharacterSkills[i];
            activeSkillsButtons[i].image.sprite = skill.skillIcon;
        }
    }

    public void SelectInventorySlot(bool active, int databaseItemIndex)
    {
        PartyInputManager.Instance.SetCursorOverUI(active);
            
        if (databaseItemIndex == -1)
            return;
        
        if (!active)
        {
            observableInfoAnim.SetBool(Active, false);
            return;
        }
        
        observableInfoText.text = ItemsDatabaseManager.Instance.ItemsDatabase.Items[databaseItemIndex].itemName;
        observableInfoText2.text = ItemsDatabaseManager.Instance.ItemsDatabase.Items[databaseItemIndex].itemDescription;
        observableInfoAnim.SetBool(Active, true);
    }
    
    public void InventorySlotClicked(int itemDatabaseIndex, Vector3 newPos)
    {
        // click shows drop down menu
        ActionsDropDownMenu.Instance.OpenInventoryItemDropDownMenu(itemDatabaseIndex, newPos);
    }
    
    
    
}