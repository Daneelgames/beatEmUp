using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private List<Attack> weaponAttacks = new List<Attack>();
    public List<Attack> WeaponAttacks => weaponAttacks;

    private int weaponDamage = 500;
    [SerializeField] private int attacksLeft = 3;
    
    private bool dangerous = false;

    [Header("Links")] 
    [SerializeField] Interactable interactable;

    public Interactable Interactable => interactable;

    private AttackManager attackManager;
    private List<GameObject> ownBodyPartsGameObjects = new List<GameObject>();
    private List<GameObject> damagedBodyPartsGameObjects = new List<GameObject>();

    public AttackManager AttackManager
    {
        get => attackManager;
        set => attackManager = value;
    }

    public void SetNewOwner(AttackManager _attackManager)
    {
        AttackManager = _attackManager;
        ownBodyPartsGameObjects = AttackManager.Hc.BodyPartsManager.bodyParts[0].OwnBodyPartsGameObjects;
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
                if (AttackManager.DamageOtherBodyPart(newPartToDamage, weaponDamage))
                {
                    attacksLeft--;
                    damagedBodyPartsGameObjects.Add(newPartToDamage.gameObject);

                    if (attacksLeft <= 0)
                    {
                        AttackManager.RemoveWeapon(this);
                        Destroy(gameObject);
                    }
                }
            }
        }
    }
}
