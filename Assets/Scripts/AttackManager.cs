using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class AttackManager : MonoBehaviour
{
    [Header("Attack Stats")] 
    [SerializeField] private int baseAttackDamage = 10;
    [SerializeField] [Range(0,1)] private float critChance = 0.1f;
    [SerializeField] [Range(1,10)] private float critDamageScaler = 3;

    [SerializeField] private List<Attack> attackList = new List<Attack>();
    List<Attack> tempAttackList = new List<Attack>();
    
    [SerializeField] private List<GrabAttack> grabAttacksList = new List<GrabAttack>();
    [SerializeField] private float grabAttacksRange = 3;

    private Coroutine attackSwingCoroutine;
    private Coroutine attackDangerCoroutine;
    private Coroutine attackReturnCoroutine;

    private Weapon weaponInHands;
    
    [Header("Links")] 
    [SerializeField] private HealthController hc;
    public HealthController Hc => hc;

    [SerializeField] private Transform weaponParentTransform;
    public Transform WeaponParentTransform => weaponParentTransform;
    
    [SerializeField] private Animator anim;
    private Attack currentAttack;
    private GrabAttack currentGrabAttack;
    public GameObject HitParticle;
    
    private List<HealthController> damagedHCs = new List<HealthController>();
    public Attack CurrentAttack
    {
        get => currentAttack;
    }
    public GrabAttack CurrentGrabAttack
    {
        get => currentGrabAttack;
    }

    private Attack prevAttack;

    private bool canMove = true;
    private bool canRotate = true;

    public bool CanMove
    {
        get => canMove;
        set
        {
            canMove = value; 
        }
    }
    public bool CanRotate
    {
        get => canRotate;
        set
        {
            canRotate = value; 
        }
    }

    public void Jumped()
    {
        if (currentGrabAttack != null)
            return;
        
        if (attackSwingCoroutine != null)
        {
            StopCoroutine(attackSwingCoroutine);
            attackSwingCoroutine = null;
        }
        if (attackDangerCoroutine != null)
        {
            StopCoroutine(attackDangerCoroutine);
            attackDangerCoroutine = null;
            
            for (var index = currentAttack.dangeorusParts.Count - 1; index >= 0; index--)
            {
                var part = currentAttack.dangeorusParts[index];
                if (part)
                    part.SetDangerous(false);
            }
        }
        if (attackReturnCoroutine != null)
        {
            StopCoroutine(attackReturnCoroutine);
            attackReturnCoroutine = null;
        }

        currentAttack = null;
        CanMove = true;
        CanRotate = true;
    }

    public void Damaged()
    {
        if (currentGrabAttack != null)
            return;

        if (attackSwingCoroutine != null)
        {
            StopCoroutine(attackSwingCoroutine);
            attackSwingCoroutine = null;
        }
        if (attackDangerCoroutine != null)
        {
            StopCoroutine(attackDangerCoroutine);
            attackDangerCoroutine = null;
            
            for (var index = currentAttack.dangeorusParts.Count - 1; index >= 0; index--)
            {
                var part = currentAttack.dangeorusParts[index];
                if (part)
                    part.SetDangerous(false);
            }
        }
        if (attackReturnCoroutine != null)
        {
            StopCoroutine(attackReturnCoroutine);
            attackReturnCoroutine = null;
        }
        
        currentAttack = null;
        CanMove = false;
        CanRotate = false;
    }

    public void RestoredFromDamage()
    {
        if (currentGrabAttack != null)
            return;

        CanMove = true;
        CanRotate = true;
    }
    
    public void TryToAttack(bool playerInput)
    {
        if (currentGrabAttack != null)
        {
            print("currentGrabAttack != null");
            return;
        }

        // MELEE
        if (currentAttack != null && !currentAttack.CanSkipReturn && attackReturnCoroutine != null)
        {
            print("currentAttack != null");
            return;
        }

        // GRAB ATTACK
        if (grabAttacksList.Count > 0 && weaponInHands == null)
        {
            // IF LOW HP ENEMY NEARBY
            var nearbyEnemy = GameManager.Instance.GetClosestUnit(hc, !playerInput,grabAttacksRange); 
            if (nearbyEnemy != null)
            {
                // EXECUTE PAIRED ANIMATION
                float healthPercent = (nearbyEnemy.Health * 1f) / (nearbyEnemy.HealthMax * 1f);

                if (nearbyEnemy.PlayerInput == null && nearbyEnemy.VisibleHCs.Contains(hc) == false && nearbyEnemy.AiInput.aiState != AiInput.State.FollowTarget)
                    healthPercent = 0;
            
                currentGrabAttack = ChooseGrabAttack(healthPercent);

                if (currentGrabAttack != null)
                {
                    if (attackSwingCoroutine != null)
                    {
                        StopCoroutine(attackSwingCoroutine);
                        attackSwingCoroutine = null;
                    }
                    if (attackDangerCoroutine != null)
                    {
                        StopCoroutine(attackDangerCoroutine);
                        attackDangerCoroutine = null;
                    }
                    if (attackReturnCoroutine != null)
                    {
                        StopCoroutine(attackReturnCoroutine);
                        attackReturnCoroutine = null;
                    }
                    StartCoroutine(ExecuteGrabAttack(nearbyEnemy));
                    return;   
                }
            }   
        }
        
        if (attackReturnCoroutine != null)
        {
            // Stops return coroutine
            StopCoroutine(attackReturnCoroutine);
            attackReturnCoroutine = null;
            CanMove = true;
            CanRotate = true;   
        }
        
        if (attackDangerCoroutine != null || attackSwingCoroutine != null)
        {
            // Already attacking, do nothing and exit
            return;
        }
        // clear the list of attacked units from previous attack 
        damagedHCs.Clear();
        // If not attacking, start new attack
        ChooseAttack();
        attackSwingCoroutine = StartCoroutine(AttackSwing());
    }

    public void ThrowWeapon()
    {
        if (weaponInHands == null)
            return;
        
        
    }


    GrabAttack ChooseGrabAttack(float healthPercent)
    {
        List<GrabAttack> tempList = new List<GrabAttack>(grabAttacksList);

        for (int i = tempList.Count - 1; i >= 0; i--)
        {
            if (healthPercent > tempList[i].healthPercentMin)
                tempList.RemoveAt(i);
        }

        if (tempList.Count <= 0)
            return null;
        
        return tempList[Random.Range(0, tempList.Count)];
    }

    IEnumerator ExecuteGrabAttack(HealthController victimHc)
    {
        //move victim's hips inside our animator
        victimHc.Death(true, false, false, false);
        
        if (victimHc.BodyPartsManager.HipsBone == null)
            yield break;
        
        Transform victimsHips = victimHc.BodyPartsManager.HipsBone.transform;
        
        Vector3 localPositionInsideParent = victimsHips.localPosition;
        Vector3 localScaleInsideParent = victimsHips.localScale;
        Quaternion localRotationInsideParent = victimsHips.localRotation;
        
        victimsHips.parent = hc.BodyPartsManager.victimTargetTransform;
        hc.Anim.Rebind();
        anim.Rebind();

        anim.SetBool(currentGrabAttack.AttackAnimationTriggerName, true);
        CanMove = false;
        CanRotate = false;

        yield return StartCoroutine(MoveVictimHipsInsideAnimator(victimsHips, localPositionInsideParent, localScaleInsideParent, localRotationInsideParent));
        
        if (victimHc.BodyPartsManager.HipsBone == null)
            yield break;
        
        yield return new WaitForSeconds(currentGrabAttack.GrabAttackDuration);
        
        if (victimHc.BodyPartsManager.HipsBone == null)
            yield break;
        
        anim.SetBool(currentGrabAttack.AttackAnimationTriggerName, false);
        CanMove = true;
        CanRotate = true;

        victimHc.Death(false, false, true, false);
        
        victimHc.transform.position = victimHc.BodyPartsManager.HipsBone.transform.position; 
        victimHc.transform.rotation = victimHc.BodyPartsManager.HipsBone.transform.rotation;

        GameObject fakeCorpse = Instantiate(victimsHips.gameObject, victimsHips.transform.position, victimsHips.transform.rotation);
        fakeCorpse.transform.parent = victimHc.transform;
        
        Destroy(victimsHips.gameObject);
        
        anim.Rebind();
        
        currentGrabAttack = null;
    }

    IEnumerator MoveVictimHipsInsideAnimator(Transform victimsHips, Vector3 localPositionInsideParent,
        Vector3 localScaleInsideParent, Quaternion localRotationInsideParent)
    {
        Vector3 victimsInitLocalPos = victimsHips.localPosition;
        Quaternion victimIniLocalRot = victimsHips.localRotation;
        
        float t = 0;
        
        while (t < currentGrabAttack.prepareTime)
        {
            t += Time.deltaTime;
            victimsHips.position = Vector3.Lerp(victimsInitLocalPos, localPositionInsideParent, t / currentGrabAttack.prepareTime);
            victimsHips.rotation = Quaternion.Slerp(victimIniLocalRot, localRotationInsideParent, t / currentGrabAttack.prepareTime);
            yield return null;
        }

        victimsHips.localPosition = localPositionInsideParent;
        victimsHips.localScale = localScaleInsideParent;
        victimsHips.localRotation = localRotationInsideParent;
    }
    
    void ChooseAttack()
    {
        if (tempAttackList.Count <= 0)
            tempAttackList = new List<Attack>(attackList);
        
        if (weaponInHands == null)
        {
            int newIndex = -1;
        
            // GET RANDOM ATTACK BY WEIGHT
            // Get the total sum of all the attacks
            float weightSum = 0f;
        
            for (int i = tempAttackList.Count - 1; i >= 0; --i)
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
                if (prevAttack != null && prevAttack != newAttack)
                    prevAttack.RestoreCurrentWeight();
            }
        
            // lower new attack's current Weight   
            newAttack.ReduceCurrentWeight();
            tempAttackList.RemoveAt(newIndex);
            currentAttack = newAttack;
        }
        else
        {
            // weapon attack
            currentAttack = weaponInHands.WeaponAttacks[Random.Range(0, weaponInHands.WeaponAttacks.Count)];
        }
        
        prevAttack = currentAttack;
    }
    
    IEnumerator AttackSwing()
    {
        CanMove = currentAttack.CanMoveOnSwing;
        CanRotate = currentAttack.CanRotateOnSwing;
        
        anim.SetTrigger(currentAttack.AttackAnimationTriggerName);
        yield return new WaitForSeconds(currentAttack.AttackSwingTime);
        attackDangerCoroutine = StartCoroutine(AttackDanger());
        attackSwingCoroutine = null;
    }

    IEnumerator AttackDanger()
    {
        CanMove = currentAttack.CanMoveOnDanger;
        CanRotate = currentAttack.CanRotateOnDanger;
        
        for (var index = currentAttack.dangeorusParts.Count - 1; index >= 0; index--)
        {
            var part = currentAttack.dangeorusParts[index];
            if (part)
                part.SetDangerous(true);
        }

        if (weaponInHands)
            weaponInHands.SetDangerous(true);

        yield return new WaitForSeconds(currentAttack.AttackDangerTime);
        
        for (var index = currentAttack.dangeorusParts.Count - 1; index >= 0; index--)
        {
            var part = currentAttack.dangeorusParts[index];
            if (part)
                part.SetDangerous(false);
        }
        if (weaponInHands)
            weaponInHands.SetDangerous(true);
        
        attackReturnCoroutine = StartCoroutine(AttackReturn());
        attackDangerCoroutine = null;
    }

    IEnumerator AttackReturn()
    {
        CanMove = currentAttack.CanMoveOnReturn;
        CanRotate = currentAttack.CanRotateOnReturn;
        
        yield return new WaitForSeconds(currentAttack.AttackReturnTime);
        attackReturnCoroutine = null;
        currentAttack = null;
        CanMove = true;
        CanRotate = true;
    }

    public bool DamageOtherBodyPart(BodyPart partToDamage, int additionalWeaponDamage)
    {
        if (currentAttack == null)
            return false;
        
        if (damagedHCs.Contains(partToDamage.HC))
            return false;

        bool damagedSuccessfully = false;
        
        damagedHCs.Add(partToDamage.HC);
        var newParticle = Instantiate(HitParticle,partToDamage.Collider.bounds.center, Quaternion.identity);
        int resultDamage = Mathf.RoundToInt((baseAttackDamage + additionalWeaponDamage) * currentAttack.AttackDamageScaler);
        float randomCritChance = Random.value;
        int _criticalDamage = 0;
        
        if (randomCritChance <= critChance * currentAttack.AttackCritChanceScaler)
            resultDamage *= Mathf.RoundToInt(critDamageScaler);

        damagedSuccessfully = partToDamage.HC.Damage(resultDamage, hc);
        
        if (hc.Friends.Contains(partToDamage.HC))
        {
            if (Random.value < hc.AiInput.Kidness)
            {
                hc.RemoveEnemy(partToDamage.HC);
            }
        }

        return damagedSuccessfully;
    }

    
    public void PickWeapon(Interactable interactable)
    {
        interactable.transform.position = WeaponParentTransform.position;
        interactable.transform.rotation = WeaponParentTransform.rotation;
        interactable.transform.parent = WeaponParentTransform;
        interactable.WeaponPickUp.SetNewOwner(this);
        
        if (weaponInHands != null)
        {
            var weaponToDrop = Instantiate(weaponInHands, weaponInHands.transform.position, weaponInHands.transform.rotation);
            
            GameManager.Instance.SetLayerRecursively(weaponToDrop.gameObject, 9);
            
            weaponToDrop.transform.localScale = Vector3.one;
            weaponToDrop.Interactable.ToggleTriggerCollider(false);
            weaponToDrop.Interactable.ToggleRigidbodyKinematicAndGravity(false, true);
        }
        
        weaponInHands = interactable.WeaponPickUp;
    }

    public void RemoveWeapon(Weapon weapon)
    {
        if (weaponInHands == weapon)
            weaponInHands = null;
    }
}



# region Attack
[Serializable]
public class Attack
{
    [Tooltip("This controls how often character uses this attack")]
    [SerializeField] [Range(0f, 1f)] private float attackWeight = 1f; 
    [SerializeField] [Range(0f, 1f)] private float reduceAttackWeightOnRepeat = 0.5f; 
    [SerializeField] [Range(0f, 1f)] private float restoreAttackWeightOnAttackRest = 0.25f; 
    private float attackWeightCurrent = 1f;

    [SerializeField] private bool canAttackMidAir = false;
    [SerializeField] [Range(0.1f,5f)] private float attackDamageScaler = 1f;
    public float AttackDamageScaler { get => attackDamageScaler; }
    
    [SerializeField] [Range(0f,5f)] private float attackCritChanceScaler = 1f;
    public float AttackCritChanceScaler { get => attackCritChanceScaler; }

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
    
    public List<BodyPart> dangeorusParts;
    
    public float AttackWeightCurrent
    {
        get => attackWeightCurrent;
        set => attackWeightCurrent = value;
    }

    public void RestoreCurrentWeight()
    {
        attackWeightCurrent += restoreAttackWeightOnAttackRest;
        attackWeightCurrent = Mathf.Clamp(attackWeightCurrent, 0,attackWeight);     
    }

    public void ReduceCurrentWeight()
    {
        attackWeightCurrent -= reduceAttackWeightOnRepeat;
        if (attackWeightCurrent <= 0)
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
# endregion

#region GrabAttack

[Serializable]
public class GrabAttack
{
    public string AttackAnimationTriggerName = "PairedAttack";
    public float prepareTime = 0.5f;
    public float GrabAttackDuration = 1f;
    [Range(0.01f, 0.99f)] public float healthPercentMin = 0.2f;
}
#endregion