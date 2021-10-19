using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionController : MonoBehaviour
{
    void Start()
    {

        StartCoroutine(UpdateInteraction());
    }

    IEnumerator UpdateInteraction()
    {
        // pick up the item and put it in a weapon BodyPartsManager.WeaponParentTransform
        while (true)
        {
            
            yield return null;
        }
    }
}