using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class HealthController : MonoBehaviour
{

    [Header("Links")] 
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator anim;
    [SerializeField] private AiInput _aiInput;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private BodyPartsManager _bodyPartsManager;

    [Header("Stats")] 
    [SerializeField] private bool invincible = false;
    [SerializeField] private int health = 100;
    public int Health => health;

    private static readonly int DamagedString = Animator.StringToHash("Damaged");
    private static readonly int Alive = Animator.StringToHash("Alive");
    [SerializeField] private AttackManager _attackManager;
    [SerializeField] private float damagedAnimationTime = 0.5f;

    private bool damaged = false;

    public bool Damaged => damaged;

    private Coroutine damageAnimCoroutine;

    private List<HealthController> enemies = new List<HealthController>();
    public List<HealthController> Enemies => enemies;
    
    [ContextMenu("FastInit")]
    public void FastInit()
    {
        _attackManager = GetComponent<AttackManager>();
        anim = GetComponentInChildren<Animator>();
    }
    public void Damage(int dmg, HealthController damager)
    {
        if (health <= 0)
            return;

        if (_aiInput)
            _aiInput.Damaged(damager);
        
        AddEnemy(damager);
        
        if (!invincible)
            health -= dmg;

        if (health <= 0)
        {
            anim.SetBool(Alive, false);
            Death();
        }
        else
        {
            if (damageAnimCoroutine != null)
            {
                StopCoroutine(damageAnimCoroutine);
            }

            damageAnimCoroutine = StartCoroutine(DamageAnim());
        }
    }

    IEnumerator DamageAnim()
    {
        damaged = true;
        anim.SetBool(DamagedString, true);
        _attackManager.Damaged();
        
        yield return new WaitForSeconds(damagedAnimationTime);
        
        damaged = false;
        anim.SetBool(DamagedString, false);
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

    void Death()
    {
        _bodyPartsManager.SetAllPartsColliders();
        
        if (_aiInput)
            _aiInput.Death();
        
        if (agent)
            agent.enabled = false;
        if (characterController)
            characterController.enabled = false;
        
        rb.isKinematic = false;
        rb.useGravity = true;
        GameManager.Instance.SetLayerRecursively(gameObject, 6);
        rb.AddExplosionForce(100,transform.position + Random.onUnitSphere, 10);

        if (Random.value > 0.5f)
        {
            StartCoroutine(_bodyPartsManager.RemovePart(false));   
        }
        
        StartCoroutine(GameManager.Instance.FreezeRigidbodyOverTime(5, rb, 5, true));
    }
}