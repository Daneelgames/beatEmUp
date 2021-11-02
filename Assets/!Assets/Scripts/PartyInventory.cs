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

    private Coroutine pickConsumableCoroutine;
    public void PickUpConsumable(HealthController hc, Interactable consumableInteractable)
    {
        if ((hc && hc.AiInput.inParty == false) || pickConsumableCoroutine != null)
            return;

        pickConsumableCoroutine = StartCoroutine(PickConsumableWithDelay(consumableInteractable));
    }

    IEnumerator PickConsumableWithDelay(Interactable consumableInteractable)
    {
        yield return new WaitForSeconds(0.1f);
        
        if (consumableInteractable.ConsumablePickUp.heal)
        {
            MedKitsAmount++;   
            PartyUi.Instance.UpdateMedKits();
        }
        Destroy(consumableInteractable.gameObject);
        pickConsumableCoroutine = null;
    }
}