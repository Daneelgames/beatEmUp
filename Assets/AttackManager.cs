using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AttackManager : MonoBehaviour
{
    [Header("Attack Stats")] 
    [SerializeField] private int baseAttackDamage = 10;
    [SerializeField] [Range(0,1)] private float critChange = 0.1f;
    [SerializeField] [Range(1,10)] private float critDamageScaler = 3;

    [SerializeField] private List<Attack> attackList = new List<Attack>();

    private Coroutine attackSwingCoroutine;
    private Coroutine attackDangerCoroutine;
    private Coroutine attackReturnCoroutine;

    [Header("Links")]
    [SerializeField] private Animator anim;
    private Attack currentAttack;

    private bool canMove = true;
    private bool canRotate = true;

    public bool CanMove => canMove;
    public bool CanRotate => canRotate;

    public void TryToAttack()
    {
        // MELEE
        if (currentAttack != null && !currentAttack.CanSkipReturn)
        {
            return;
        }
        
        if (attackReturnCoroutine != null)
        {
            // Stops return coroutine
            StopCoroutine(attackReturnCoroutine);
            attackReturnCoroutine = null;
            canMove = true;
            canRotate = true;
        }
        
        if (attackDangerCoroutine != null)
        {
            // Already attacking, do nothing and exit
            return;
        }
        
        if (attackSwingCoroutine != null)
        {
            // Already attacking, do nothing and exit
            return;
        }
        
        // If not attacking, start new attack
        ChooseAttack();
        attackSwingCoroutine = StartCoroutine(AttackSwing());
    }

    void ChooseAttack()
    {
        List<Attack> tempAttackList = new List<Attack>(attackList);
        int newIndex = -1;
        
        // GET RANDOM ATTACK BY WEIGHT
        // Get the total sum of all the attacks
        float weightSum = 0f;
        for (int i = 0; i < tempAttackList.Count; ++i)
        {
            weightSum += tempAttackList[i].AttackWeightCurrent;
        }
        // Step through all the possibilities, one by one, checking to see if each one is selected.
        int index = 0;
        int lastIndex = tempAttackList.Count - 1;
        while (index < lastIndex)
        {
            // Do a probability check with a likelihood of weights[index] / weightSum.
            if (Random.Range(0, weightSum) < tempAttackList[index].AttackWeightCurrent)
            {
                newIndex = index;
            }
 
            // Remove the last item from the sum of total untested weights and try again.
            weightSum -= tempAttackList[index++].AttackWeightCurrent;
        }
 
        // No other item was selected, so return very last index.
        if (newIndex == -1)
            newIndex = index;
        
            
        var newAttack = tempAttackList[newIndex];

        if (newAttack != currentAttack)
        {
            // IF ATTACK IS NEW - RESETS OLD ATTACK'S WEIGHT
            if (currentAttack != null)
                currentAttack.ResetCurrentWeight();
        }
        
        // lower new attack's current Weight   
        newAttack.AttackWeightCurrent -= newAttack.ReduceAttackWeightOnRepeat;
        
        currentAttack = newAttack;
    }
    
    IEnumerator AttackSwing()
    {
        canMove = currentAttack.CanMoveOnSwing;
        canRotate = currentAttack.CanRotateOnSwing;
        
        anim.SetTrigger(currentAttack.AttackAnimationTriggerName);
        yield return new WaitForSeconds(currentAttack.AttackSwingTime);
        attackDangerCoroutine = StartCoroutine(AttackDanger());
        attackSwingCoroutine = null;
    }

    IEnumerator AttackDanger()
    {
        canMove = currentAttack.CanMoveOnDanger;
        canRotate = currentAttack.CanRotateOnDanger;

        yield return new WaitForSeconds(currentAttack.AttackDangerTime);
        attackReturnCoroutine = StartCoroutine(AttackReturn());
        attackDangerCoroutine = null;
    }

    IEnumerator AttackReturn()
    {
        canMove = currentAttack.CanMoveOnReturn;
        canRotate = currentAttack.CanRotateOnReturn;
        
        yield return new WaitForSeconds(currentAttack.AttackReturnTime);
        attackReturnCoroutine = null;
        currentAttack = null;
        canMove = true;
        canRotate = true;
    }
}

[Serializable]
public class Attack
{
    [Tooltip("This controls how often character uses this attack")]
    [SerializeField] [Range(0f, 1f)] private float attackWeight = 1f; 
    [SerializeField] [Range(0f, 1f)] private float reduceAttackWeightOnRepeat = 0.5f; 
    private float attackWeightCurrent = 1f;

    [SerializeField] private bool canAttackMidAir = false;
    [SerializeField] [Range(0.1f,5f)] private float attackDamageScaler = 1f;
    [SerializeField] [Range(0f,5f)] private float attackCritChanceScaler = 1f;
    [SerializeField] private string attackAnimationTriggerName = "Attack";
    [Header("Time")]
    [SerializeField] private float attackSwingTime = 0.5f;
    [SerializeField] private float attackDangerTime = 0.5f;
    [SerializeField] private float attackReturnTime = 0.5f;
    [SerializeField] private bool canMoveOnSwing = true;
    [SerializeField] private bool canMoveOnDanger = false;
    [SerializeField] private bool canMoveOnReturn = true;
    [SerializeField] private bool canRotateOnSwing = true;
    [SerializeField] private bool canRotateOnDanger = false;
    [SerializeField] private bool canRotateOnReturn = true;
    [SerializeField] private bool canSkipReturn = true;
    

    public float AttackWeightCurrent
    {
        get => attackWeightCurrent;
        set => attackWeightCurrent = value;
    }

    public float ReduceAttackWeightOnRepeat
    {
        get => reduceAttackWeightOnRepeat;
        set => reduceAttackWeightOnRepeat = value;
    }

    public void ResetCurrentWeight()
    {
        attackWeightCurrent = attackWeight;
    }

    public float AttackSwingTime => attackSwingTime;

    public float AttackDangerTime => attackDangerTime;
    public float AttackReturnTime => attackReturnTime;
    public string AttackAnimationTriggerName => attackAnimationTriggerName;

    public bool CanAttackMidAir => canAttackMidAir;

    public bool CanMoveOnSwing => canMoveOnSwing;
    public bool CanMoveOnDanger => canMoveOnDanger;
    public bool CanMoveOnReturn => canMoveOnReturn;
    public bool CanRotateOnSwing => canRotateOnSwing;
    public bool CanRotateOnDanger => canRotateOnDanger;
    public bool CanRotateOnReturn => canRotateOnReturn;
    public bool CanSkipReturn => canSkipReturn;
}