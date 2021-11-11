using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class BodyPart : MonoBehaviour
{
    [SerializeField] private HealthController hc;
    [SerializeField] private AttackManager _attackManager;
    [SerializeField] private Collider collider;
    public Collider Collider => collider;
    public HealthController HC => hc;

    bool dangerous = false;

    private List<GameObject> damagedBodyPartsGameObjects = new List<GameObject>();
    [SerializeField] private List<GameObject> ownBodyPartsGameObjects = new List<GameObject>();
    public List<GameObject> OwnBodyPartsGameObjects => ownBodyPartsGameObjects;

    [SerializeField]
    Vector3 globalScaleAfterReparenting = new Vector3(30, 30, 30);
    public Vector3 GlobalScaleAfterReparenting => globalScaleAfterReparenting;

    private List<GameObject> gameObjectsOnStay = new List<GameObject>();
    private void Awake()
    {
        SpawnController.Instance.AddBodyPartTransform(transform, HC);
    }

    public void SaveGlobalScaleAfterReparenting()
    {
        globalScaleAfterReparenting = transform.lossyScale;
    }
    public void SetOwnBodyParts(List<BodyPart> tempList, AttackManager attackManager, HealthController _hc)
    {
        ownBodyPartsGameObjects.Clear();
        _attackManager = attackManager;
        hc = _hc;
        collider = GetComponent<Collider>();
        for (int i = 0; i < tempList.Count; i++)
        {
            ownBodyPartsGameObjects.Add(tempList[i].gameObject);
        }
    }

    public void SetDangerous(bool _dangerous)
    {
        damagedBodyPartsGameObjects.Clear();
        dangerous = _dangerous;

        if (dangerous && gameObjectsOnStay.Count > 0)
        {
            // damage gameObjectsOnStay
            for (int i = gameObjectsOnStay.Count - 1; i >= 0; i--)
            {
                if (gameObjectsOnStay[i] == null)
                {
                    gameObjectsOnStay.RemoveAt(i);
                    continue;
                }
                
                DamageHcByPartTransform(gameObjectsOnStay[i].transform);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
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
        
        if (!dangerous)
        {
            // need to add this object to a list gameObjectsOnStay
            // which then will be attacked when part becomes dangerous 
            gameObjectsOnStay.Add(other.gameObject);
            return;
        }
        
        // else - damage it right away
        DamageHcByPartTransform(other.transform);
    }

    void DamageHcByPartTransform(Transform part)
    {
        HealthController hcToDamage = SpawnController.Instance.GetHcByBodyPartTransform(part.transform);
        if (hcToDamage)
        {
            _attackManager.DamageOtherBodyPart(hcToDamage.BodyPartsManager.bodyParts[Random.Range(0, hcToDamage.BodyPartsManager.bodyParts.Count)],
                0, HealthController.DamageType.Melee);
            damagedBodyPartsGameObjects.Add(part.gameObject);
        }
    }

    void OnTriggerExit(Collider coll)
    {
        if (gameObjectsOnStay.Contains(coll.gameObject))
            gameObjectsOnStay.Remove(coll.gameObject);
    }
}