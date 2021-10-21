using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private List<Attack> weaponAttacks = new List<Attack>();
    public List<Attack> WeaponAttacks => weaponAttacks;
    [SerializeField] private Vector3 eulearsOnThrow = Vector3.zero;
    
    [SerializeField] private int weaponDamage = 500;
    [SerializeField] private int throwDamage = 500;
    [SerializeField] private int impactNoiseDistance = 10;
    [SerializeField] private int throwPower = 10;
    public int ThrowPower => throwPower;
    [SerializeField] private int attacksLeft = 3;
    
    private bool dangerous = false;

    [Header("Links")] 
    [SerializeField] Interactable interactable;
    [SerializeField] Rigidbody rb;
    public Rigidbody Rigidbody => rb;

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

    public void Throw(Vector3 throwTargetPoint)
    {
        AttackManager = null;
        transform.localEulerAngles = eulearsOnThrow;
        Interactable.ToggleTriggerCollider(false);
        Interactable.ToggleRigidbodyKinematicAndGravity(false, true);
        Rigidbody.AddForce((throwTargetPoint - transform.position).normalized * ThrowPower, ForceMode.Impulse); 
        GameManager.Instance.SetLayerRecursively(gameObject,9);
        SetDangerous(true);
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
                if (AttackManager == null)
                {
                    attacksLeft = 0;
                    DamageDirectly(newPartToDamage);
                    AfterAttack(newPartToDamage);
                }
                else if (AttackManager.DamageOtherBodyPart(newPartToDamage, weaponDamage))
                {
                    attacksLeft--;
                    AfterAttack(newPartToDamage);
                }
            }
        }
    }

    void DamageDirectly(BodyPart partToDamage)
    {
        SpawnController.Instance.MakeNoise(transform.position, impactNoiseDistance);
        partToDamage.HC.Damage(throwDamage, null);
    }

    void AfterAttack(BodyPart newPartToDamage)
    {
        damagedBodyPartsGameObjects.Add(newPartToDamage.gameObject);

        if (attacksLeft <= 0)
        {
            if (SpawnController.Instance.Interactables.Contains(interactable))
                SpawnController.Instance.Interactables.Remove(interactable);
            if (SpawnController.Instance.InteractablesGameObjects.Contains(interactable.gameObject))
                SpawnController.Instance.InteractablesGameObjects.Remove(interactable.gameObject);
                      
            if (attackManager)
                AttackManager.RemoveWeapon(this);
            
            Destroy(gameObject);
        }
    }
}
