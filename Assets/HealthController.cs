using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HealthController : MonoBehaviour
{
    [SerializeField] private int health = 100;
    [SerializeField] private Animator anim;
    private static readonly int DamagedString = Animator.StringToHash("Damaged");
    private static readonly int Alive = Animator.StringToHash("Alive");
    [SerializeField] private AttackManager _attackManager;
    [SerializeField] private float damagedAnimationTime = 0.5f;

    private bool damaged = false;

    public bool Damaged => damaged;

    private Coroutine damageAnimCoroutine;

    [ContextMenu("FastInit")]
    public void FastInit()
    {
        _attackManager = GetComponent<AttackManager>();
        anim = GetComponentInChildren<Animator>();
    }
    public void Damage(int dmg)
    {
        if (health <= 0)
            return;
        
        
        health -= dmg;

        if (health <= 0)
        {
            anim.SetBool(Alive, false);
            Death();
        }
        else
        {
            _attackManager.Damaged();
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
        yield return new WaitForSeconds(damagedAnimationTime);
        damaged = false;
        anim.SetBool(DamagedString, false);
        _attackManager.RestoredFromDamage();
    }
    

    void Death()
    {
        Destroy(gameObject, 3);
    }
}
