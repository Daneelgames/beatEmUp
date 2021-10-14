using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BodyPart : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private HealthController hc;
    [SerializeField] private AttackManager _attackManager;
    public HealthController HC => hc;

    bool dangerous = false;

    private List<GameObject> damagedBodyPartsGameObjects = new List<GameObject>();
    [SerializeField] private List<GameObject> ownBodyPartsGameObjects = new List<GameObject>();
    public void SetOwnBodyParts(List<BodyPart> tempList, AttackManager attackManager, HealthController _hc)
    {
        ownBodyPartsGameObjects.Clear();
        _attackManager = attackManager;
        hc = _hc;
        for (int i = 0; i < tempList.Count; i++)
        {
            ownBodyPartsGameObjects.Add(tempList[i].gameObject);
        }
    }

    public void SetDangerous(bool _dangerous)
    {
        dangerous = _dangerous;
        damagedBodyPartsGameObjects.Clear();    
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (dangerous)
        {
            if (other.gameObject.layer != 7)
                return;
            
            if (ownBodyPartsGameObjects.Contains(other.gameObject))
            {
                return;
            }
            if (damagedBodyPartsGameObjects.Contains(other.gameObject))
            {
                return;
            }
            
            var newPartToDamage = other.gameObject.GetComponent<BodyPart>();

            if (newPartToDamage)
            {
                _attackManager.DamageOtherBodyPart(newPartToDamage);
                damagedBodyPartsGameObjects.Add(newPartToDamage.gameObject);
            }
        }
    }
}
