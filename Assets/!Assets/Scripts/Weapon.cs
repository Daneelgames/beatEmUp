using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Ranged")] 
    [SerializeField] private float shotRaycastDistance = 100;
    [SerializeField] private float shotNoiseDistance = 100;
    [SerializeField] private List<Attack> rangedWeaponAttacks = new List<Attack>();
    public List<Attack> RangedWeaponAttacks => rangedWeaponAttacks;
    [SerializeField] private int rangedWeaponDamage = 1000;
    [SerializeField] private int ammo = 0;
    [SerializeField] private ParticleSystem shotParticles;
    [SerializeField] private LayerMask shotLayerMask;
    public int Ammo
    {
        get { return ammo; }
        set { ammo = value; }
    }

    [Header("Universal")]

    [SerializeField] private List<Attack> weaponAttacks = new List<Attack>();
    public List<Attack> WeaponAttacks => weaponAttacks;
    [SerializeField] private Vector3 eulearsOnThrow = Vector3.zero;
    [SerializeField] private int weaponDamage = 500;
    [SerializeField] private int throwDamage = 500;
    [SerializeField] private int impactNoiseDistance = 20;
    [SerializeField] private int meleeNoiseDistance = 20;
    [SerializeField] private int throwPower = 10;
    [SerializeField] private float minDistanceToStopThrowFly = 2;
    public int ThrowPower => throwPower;
    [SerializeField] private int attacksLeft = 3;
    
    private bool dangerous = false;

    [Header("Links")] 
    [SerializeField] private AudioSource shotAu;
    [SerializeField] Interactable interactable;
    [SerializeField] Rigidbody rb;
    public Rigidbody Rigidbody => rb;

    public Interactable Interactable => interactable;

    private AttackManager attackManager;
    private List<GameObject> ownBodyPartsGameObjects = new List<GameObject>();
    private List<GameObject> damagedBodyPartsGameObjects = new List<GameObject>();

    private Coroutine thrownCoroutine;
    public AttackManager AttackManager
    {
        get => attackManager;
        set => attackManager = value;
    }
   
    public void SetNewOwner(AttackManager _attackManager)
    {
        AttackManager = _attackManager;
        ownBodyPartsGameObjects = AttackManager.Hc.BodyPartsManager.bodyParts[0].OwnBodyPartsGameObjects;
        Interactable.ToggleLight(!attackManager);
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
        GameManager.Instance.SetLayerRecursively(gameObject,9);
        SetDangerous(true);
        
        StopThrowFly();
        
        thrownCoroutine = StartCoroutine(Thrown(throwTargetPoint));
    }

    IEnumerator Thrown(Vector3 targetPoint)
    {
        Rigidbody.useGravity = false;
        
        while (true)
        {
            Rigidbody.AddTorque(0,90,0);
            Rigidbody.AddForce((targetPoint - transform.position).normalized * ThrowPower, ForceMode.Force);
            yield return null;
            if (Vector3.Distance(transform.position, targetPoint) < minDistanceToStopThrowFly)
                StopThrowFly();
        }
    }

    void StopThrowFly()
    {
        if (thrownCoroutine != null)
            StopCoroutine(thrownCoroutine);

        Rigidbody.useGravity = true;
        
        if (attackManager == null)
            SetDangerous(false);
    }

    void OnCollisionEnter(Collision other)
    {
        if (thrownCoroutine != null && other.collider.gameObject.layer == 6)
        {
            StopThrowFly();
            SpawnController.Instance.MakeNoise(transform.position, impactNoiseDistance);
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (thrownCoroutine != null && other.gameObject.layer == 6)
        {
            StopThrowFly();
        }
        
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
                StopThrowFly();
                
                if (AttackManager == null)
                {
                    attacksLeft = 0;
                    SpawnController.Instance.MakeNoise(transform.position, impactNoiseDistance);
                    DamageOnThrow(newPartToDamage, throwDamage);
                    AfterAttack(newPartToDamage);
                }
                else if (AttackManager.DamageOtherBodyPart(newPartToDamage, weaponDamage, HealthController.DamageType.Melee))
                {
                    SpawnController.Instance.MakeNoise(transform.position, meleeNoiseDistance);
                    
                    if (attackManager.Hc.CharacterPerksController.WeaponLover != null || Random.value > 0.66f)
                        attacksLeft--;
                    AfterAttack(newPartToDamage);
                }
            }
        }
    }

    void DamageOnThrow(BodyPart partToDamage, int dmg)
    {
        partToDamage.HC.Damage(dmg, null, HealthController.DamageType.Throw);
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
                      
            if (AttackManager)
                AttackManager.DestroyWeaponInHands(this, true);
        }
    }

    public void ShotFX(BodyPart boneToAim)
    {
        if (boneToAim)
        {
            if (!ShotMissed())
            {
                // HIT
                shotParticles.transform.LookAt(boneToAim.transform.position);
                
                if (attackManager)
                    attackManager.DamageOtherBodyPart(boneToAim, rangedWeaponDamage, HealthController.DamageType.Ranged);
                else
                    boneToAim.HC.Damage(rangedWeaponDamage, AttackManager.Hc, HealthController.DamageType.Ranged);
            }
            else // SHOT MISSED TARGET PART
            {
                // miss
                Vector3 missPosition = GetMissShot(boneToAim.transform.position);
                
                // RAYCAST
                RaycastHit hit;
                if (Physics.Raycast(shotParticles.transform.position, missPosition - shotParticles.transform.position,
                    out hit, shotRaycastDistance, shotLayerMask))
                {
                    shotParticles.transform.LookAt(hit.point);

                    if (hit.collider.gameObject.layer == 7)
                    {
                        BodyPart part = hit.collider.gameObject.GetComponent<BodyPart>();
                        if (part)
                        {
                            
                            if (attackManager)
                                attackManager.DamageOtherBodyPart(part, rangedWeaponDamage, HealthController.DamageType.Ranged);
                            else
                                part.HC.Damage(rangedWeaponDamage, AttackManager.Hc, HealthController.DamageType.Ranged);
                        }
                    }
                }
            }
        }
        
        SpawnController.Instance.MakeNoise(transform.position, shotNoiseDistance);
        
        if (shotAu == null)
            return;
        
        shotAu.pitch = Random.Range(0.75f, 1.25f);
        shotAu.Play();
        shotParticles.Play();
    }

    Vector3 GetMissShot(Vector3 posToAim)
    {
        Vector3 newPos = posToAim;
        
        float discomfort = attackManager.Hc.CharacterPerksController.CurrentDiscomfort; 
                
        newPos += new Vector3(Random.Range(-1f * discomfort, 1f * discomfort), 
            Random.Range(-1f * discomfort, 1f * discomfort), 
            Random.Range(-1f * discomfort, 1f * discomfort));

        return newPos;
    }

    bool ShotMissed()
    {
        float hitChance = 0.75f;
        
        if (attackManager.Hc.CharacterPerksController.GoodShooter)
            hitChance = 1f;
        else if (attackManager.Hc.CharacterPerksController.BadShooter)
            hitChance = 0.33f;

        float discomfort = attackManager.Hc.CharacterPerksController.CurrentDiscomfort; 
            
        if (discomfort >= 3)
        {
            hitChance -= 0.3f;
        }
        else if (discomfort >= 2)
        {
            hitChance -= 0.2f;
        }
        else if (discomfort > 0)
        {
            hitChance -= 0.1f;
        }

        return Random.value > hitChance;
    }
}