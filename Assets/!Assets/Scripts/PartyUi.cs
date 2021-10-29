using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private Image cursor;
    public Image Cursor => cursor;

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
            
            yield return null;
        }
    }

    void LateUpdate()
    {
        cursor.transform.position = Input.mousePosition;
    }
    
    public void UpdatePartyAggroMode()
    {
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
        actionFeedbackAnim.SetTrigger(Update);
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
        actionFeedbackAnim.SetTrigger(Update);
    }
}