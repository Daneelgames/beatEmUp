using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class HealthController : MonoBehaviour
{
    [Header("Links")] 
    [SerializeField] private PlayerInput playerInput;
    public PlayerInput PlayerInput => playerInput;

    [SerializeField] private CharacterController characterController;
    [SerializeField] private AttackManager _attackManager;
    public AttackManager AttackManager => _attackManager;

    [SerializeField] private Animator anim;
    public Animator Anim => anim;

    [SerializeField] private AiInput _aiInput;
    public AiInput AiInput => _aiInput;

    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private BodyPartsManager _bodyPartsManager;
    public BodyPartsManager BodyPartsManager => _bodyPartsManager;
    [SerializeField] private FieldOfView fieldOfView;
    public FieldOfView FieldOfView => fieldOfView;

    [Header("Stats")] 
    [SerializeField] private bool invincible = false;
    [SerializeField] private int health = 1000;
    [SerializeField] private int healthMax = 1000;
    [SerializeField] private float damagedAnimationTime = 0.5f;
    private bool damageOnCooldown = false;
    public int Health => health;
    public int HealthMax => healthMax;

    private static readonly int DamagedString = Animator.StringToHash("Damaged");
    private static readonly int Alive = Animator.StringToHash("Alive");

    private bool damaged = false;
    public bool Damaged => damaged;

    private Coroutine damageAnimCoroutine;

    [SerializeField] private List<HealthController> enemies = new List<HealthController>();
    public List<HealthController> Enemies => enemies;
    
    [SerializeField] private List<HealthController> friends = new List<HealthController>();
    public List<HealthController> Friends => friends;
    
    private List<HealthController> visibleHCs = new List<HealthController>();
    public List<HealthController> VisibleHCs => visibleHCs;

    [ContextMenu("FastInit")]
    public void FastInit()
    {
        _attackManager = GetComponent<AttackManager>();
        anim = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        GameManager.Instance.AddUnit(this);
    }
    
    public bool Damage(int dmg, HealthController damager)
    {
        bool damaged = false;
        
        if (damageOnCooldown)
            return damaged;
        
        if (health <= 0)
            return damaged;

        if (damager)
        {
            if (visibleHCs.Contains(damager) == false &&
                Vector3.Distance(transform.position, damager.transform.position) < 10)
            {
                visibleHCs.Add(damager);
            }
        
            AddEnemy(damager);
        
            if (_aiInput)
                _aiInput.SetAggro(damager);
        }
        
        if (FieldOfView)
            FieldOfView.DelayCooldown(5);
        
        if (!invincible)
        {
            damaged = true;
            health -= dmg;
        }

        if (health <= 0)
        {
            Death(false, true, false, true);
        }
        else
        {
            if (damageAnimCoroutine != null)
            {
                StopCoroutine(damageAnimCoroutine);
            }
            damageAnimCoroutine = StartCoroutine(DamageAnim());
        }

        return damaged;
    }

    IEnumerator DamageAnim()
    {
        damaged = true;
        if (_attackManager == null || _attackManager.CurrentAttack == null)
        {
            anim.SetBool(DamagedString, true);   
        }
        _attackManager.Damaged();
        damageOnCooldown = true;
        yield return new WaitForSeconds(damagedAnimationTime);
        
        damaged = false;
        
        anim.SetBool(DamagedString, false); 
        
        damageOnCooldown = false;
        _attackManager.RestoredFromDamage();
    }

    public void AddEnemy(HealthController damager)
    {
        if (damager == null)
            return;
        
        if (enemies.Contains(damager) == false)
            enemies.Add(damager);
    }

    public void RemoveEnemyAt(int index)
    {
        if (enemies.Count > index)
            enemies.RemoveAt(index);
    }

    public void RemoveEnemy(HealthController unit)
    {
        if (enemies.Contains(unit))
            enemies.Remove(unit);
    }

    public IEnumerator UpdateVisibleTargets(List<Transform> visibleTargets)
    {
        float t = 0;
        for (int i = GameManager.Instance.Units.Count - 1; i >= 0; i--)
        {
            int visibleBonesAmount = 0;
 
            var unit = GameManager.Instance.Units[i];
            if (unit == this)
                continue;

            for (int j = unit.BodyPartsManager.bodyParts.Count - 1; j >= 0; j--)
            {
                if (unit.BodyPartsManager.bodyParts[j] == null)
                    continue;
                
                if (visibleTargets.Contains(unit.BodyPartsManager.bodyParts[j].transform))
                {
                    visibleBonesAmount++;
                }
            }
            
            /*
            if (unit.playerInput)
                print("Sees " + visibleBonesAmount + " of player's bones!");*/

            if (visibleBonesAmount > fieldOfView.MinVisibleBonesToSeeUnit)
            {
                if (visibleHCs.Contains(unit) == false)
                {
                    visibleHCs.Add(unit);  
                    
                    if (enemies.Contains(unit) && _aiInput)
                        _aiInput.SetAggro(unit);   
                }
            }

            t++;
            if (t > 20)
            {
                t = 0;
                yield return null;   
            }
        }
    }
    
    public void ResetVisibleUnits()
    {
        visibleHCs.Clear();
    }
    
    public void Death(bool onlyDisableAllSystems, bool explode, bool removeRbInstantly, bool removePart)
    {
        health = 0;
        
        if (playerInput)
            playerInput.Death();
        if (_aiInput)
            _aiInput.Death();
        if (agent)
            agent.enabled = false;
        if (characterController)
            characterController.enabled = false;
        if (fieldOfView)
            fieldOfView.Death();

        if (onlyDisableAllSystems)
        {
            anim.enabled = false;
            return;   
        }
        
        _bodyPartsManager.SetAllPartsColliders();
        
        anim.SetBool(Alive, false);

        GameManager.Instance.SetLayerRecursively(gameObject, 6);

        if (removePart)
        {
            StartCoroutine(_bodyPartsManager.RemovePart(false));   
        }
        
        if (rb)
        {
            rb.isKinematic = false;
            rb.useGravity = true;   
            
            if (explode)
                rb.AddExplosionForce(100,transform.position + Random.onUnitSphere, 10);
            
            if (!removeRbInstantly)
                StartCoroutine(GameManager.Instance.FreezeRigidbodyOverTime(5, rb, 5, true));
            else
                Destroy(rb);
        }
    }
}