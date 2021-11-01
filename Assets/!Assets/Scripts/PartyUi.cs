using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class PartyUi : MonoBehaviour
{
    public static PartyUi Instance;

    void Awake()
    {
        Instance = this;
    }
    
    [SerializeField] private List<Text> partyNumbersToSelect = new List<Text>();
    [SerializeField] private Text medKitsAmountFeedback;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private Text actionFeedbackText;
    [SerializeField] private Animator actionFeedbackAnim;
    [SerializeField] private Text observableInfoText;
    [SerializeField] private Text observableInfoText2;
    [SerializeField] private Animator observableInfoAnim;
    [SerializeField] private Animator moveOrderFeedbackAnim;
    [SerializeField] private Image cursor;
    
    public Image Cursor => cursor;

    private static readonly int MoveString = Animator.StringToHash("Move");
    private static readonly int AttackString = Animator.StringToHash("Attack");
    private static readonly int Active = Animator.StringToHash("Active");

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
            
            yield return null;
        }
    }

    void LateUpdate()
    {
        cursor.transform.parent.position = Input.mousePosition;
    }
    
    public void UpdatePartyAggroMode()
    {
        return;
        
        for (int i = 0; i < PartyInputManager.Instance.Party.Count; i++)
        {
            switch (PartyInputManager.Instance.Party[i].AiInput.aggroMode)
            {
                case AiInput.AggroMode.AggroOnSight:
                     partyNumbersToSelect[i].color = Color.red;
                    break;
                
                case AiInput.AggroMode.AttackIfAttacked:
                    partyNumbersToSelect[i].color = Color.blue;
                    break;
            }
            
        }   
    }

    public void MoveOrderFeedback(Vector3 newPos)
    {
        moveOrderFeedbackAnim.SetTrigger(MoveString);
        moveOrderFeedbackAnim.transform.position = newPos;
    }
    public void AttackOrderFeedback(Vector3 newPos)
    {
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
        actionFeedbackAnim.SetTrigger(MoveString);
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

    public void UpdateObservableInfo(Observable observable)
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
                        var perk = PerksManager.Instance.GetPerkFromPerkType(perks[i]);
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
                return;
            nameString = objInfo.objectName + ", " + sexString + ", " + healthString;
            
            observableInfoText.text = nameString;
            observableInfoText2.text = objInfo.objectSpecialDescription + weaponString + perksString;
            observableInfoAnim.SetBool(Active, true);
        }
    }
}