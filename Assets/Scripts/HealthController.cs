using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

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
    [SerializeField] 
    private Image healthBar;

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
    public bool enemiesInSight = false;

    [ContextMenu("FastInit")]
    public void FastInit()
    {
        _attackManager = GetComponent<AttackManager>();
        anim = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        GameManager.Instance.AddUnit(this);
        
        if (_aiInput && _aiInput.inParty)
            PartyInputManager.Instance.AddPartyMember(this);

        if (health != null)
            StartCoroutine(UpdateHealthbar());
    }

    IEnumerator UpdateHealthbar()
    {
        healthBar.transform.parent.transform.localScale = Vector3.zero;
        while (health > 0)
        {
            float t = 0;
            while (true)
            {
                if (Vector3.Distance(transform.position, GameManager.Instance.mainCamera.transform.position) <=
                    GameManager.Instance.DrawHealthbarsDistance)
                {
                    break;
                }
                
                yield return new WaitForSeconds(1f);
            }
            
            t = 0;
            while (t < 0.5f)
            {
                t += Time.deltaTime;

                healthBar.transform.parent.transform.localScale = Vector3.one * Mathf.Lerp(0f, 1f, t / 0.5f);
                yield return null;
            }

            
            // every frame
            t = 0;
            while (true)
            {
                healthBar.fillAmount = (health * 1f) / (healthMax * 1f);
                healthBar.transform.parent.LookAt(GameManager.Instance.mainCamera.transform.position);
                
                t++;
                if (t > 30)
                {
                    t = 0;
                    if (Vector3.Distance(transform.position, GameManager.Instance.mainCamera.transform.position) >
                        GameManager.Instance.DrawHealthbarsDistance)
                    {
                        break;
                    }
                }
                
                yield return null;
            }
            
            t = 0;
            while (t < 0.5f)
            {
                t += Time.deltaTime;

                healthBar.transform.parent.transform.localScale = Vector3.one * Mathf.Lerp(1f, 0f, t / 0.5f);
                yield return null;
            }

        }
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
                _aiInput.DamagedByEnemy(damager);
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
            damager.RemoveEnemy(this);
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
        
        if (Enemies.Contains(damager) == false && Friends.Contains(damager) == false)
            Enemies.Add(damager);
    }

    public void RemoveEnemyAt(int index)
    {
        if (Enemies.Count > index)
            Enemies.RemoveAt(index);
    }

    public void RemoveEnemy(HealthController unit)
    {
        if (Enemies.Contains(unit))
            Enemies.Remove(unit);
    }

    public IEnumerator UpdateVisibleTargets(List<Transform> visibleTargets)
    {
        float t = 0;
        float distance = 100;
        float newDistance = 0;
        HealthController closestVisibleEnemy = null;
        BodyPart visibleTargetToAim = null; 
        
        enemiesInSight = false;
        print("UpdateVisibleTargets, visibleTargets " + visibleTargets.Count);

        for (int i = GameManager.Instance.Units.Count - 1; i >= 0; i--)
        {
            int visibleBonesAmount = 0;

            var unit = GameManager.Instance.Units[i];

            if (unit == this || unit.health <= 0)
                continue;

            for (int j = unit.BodyPartsManager.bodyParts.Count - 1; j >= 0; j--)
            {
                if (unit.BodyPartsManager.bodyParts[j] == null)
                    continue;
                
                if (visibleTargets.Contains(unit.BodyPartsManager.bodyParts[j].transform))
                {
                    visibleTargetToAim = unit.BodyPartsManager.bodyParts[j];
                    visibleBonesAmount++;
                }
            }

            print("visibleBonesAmount " + visibleBonesAmount);
            if (visibleBonesAmount > fieldOfView.MinVisibleBonesToSeeUnit)
            {
                if (GameManager.Instance.simpleEnemiesAllies && _aiInput && unit._aiInput)
                {
                    if (unit._aiInput.inParty == _aiInput.inParty)
                    {
                        if (Friends.Contains(unit) == false)
                            Friends.Add(unit);
                    }
                    else
                    {
                        if (Enemies.Contains(unit) == false)
                            Enemies.Add(unit);
                    }
                }
                
                if (Enemies.Contains(unit))
                {
                    newDistance = Vector3.Distance(transform.position, unit.transform.position);
                    if (newDistance < distance)
                    {
                        distance = newDistance;
                        closestVisibleEnemy = unit;
                        
                        enemiesInSight = true;
                    }
                }
                
                if (visibleHCs.Contains(unit) == false)
                {
                    visibleHCs.Add(unit);  
                }
            }

            t++;
            if (t > 50)
            {
                t = 0;
                yield return null;   
            }
        }

        if (closestVisibleEnemy != null)
        {
            _attackManager.SeeEnemy(closestVisibleEnemy, visibleTargetToAim);
            if (_aiInput)
            {
                _aiInput.SeeEnemy(closestVisibleEnemy);
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
        if (_attackManager)
            _attackManager.Death();
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