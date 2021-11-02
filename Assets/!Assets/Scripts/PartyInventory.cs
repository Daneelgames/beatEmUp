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

    private Coroutine pickInteractableCoroutine;
    public void PickUpInteractable(HealthController hc, Interactable interactable)
    {
        if ((hc && hc.AiInput.inParty == false) || pickInteractableCoroutine != null)
            return;

        pickInteractableCoroutine = StartCoroutine(PickInteractableWithDelay(interactable));
    }

    IEnumerator PickInteractableWithDelay(Interactable interactable)
    {
        yield return new WaitForSeconds(0.1f);
        
        if (interactable.IndexInDatabase == 0)
        {
            MedKitsAmount++;   
            PartyUi.Instance.UpdateMedKits();
        }
        Destroy(interactable.gameObject);
        pickInteractableCoroutine = null;
    }
}