using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class AiInput : MonoBehaviour
{
    public bool debugLogs = false;
    public bool inParty = false;

    public enum AggroMode
    {
        AggroOnSight, AttackIfAttacked
    }

    [SerializeField] private AggroMode _aggroMode = AggroMode.AggroOnSight;

    public AggroMode aggroMode
    {
        get => _aggroMode;
        set => _aggroMode = value;
    }

    public enum State
    {
        Wander, Idle, FollowTarget, Investigate, MovingOnOrder
    }

    [SerializeField] private State state = State.Wander;
    
    public State aiState
    {
        get => state;
        set => state = value;
    }
    
    [Header("Stats")] 
    [SerializeField] [Range(0f, 1f)] private float kidness = 0.5f;
    public float Kidness => kidness;

    private bool hears = true;
    public bool Hears => hears;
    float updateRate = 0.5f;
    [SerializeField] private float walkSpeed = 2;
    [SerializeField] private float runSpeed = 4;
    [SerializeField] private float stopDistance = 3;
    [SerializeField] private float stopDistanceAllyToPlayer = 5;
    [SerializeField] private float runDistanceThreshold = 5;
    [SerializeField] private float looseTargetDistance = 20;
    [SerializeField] private bool simpleWalker = true;
    [SerializeField] private Vector2 idleTimeMinMax = new Vector2(5, 30);
    [SerializeField] private Vector2 attackCooldownMinMax = new Vector2(0.5f, 3f);
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask unitsMask;
    private float attackCooldownCurrent = 0;

    private Vector3 currentTargetPosition;

    [Header("Links")] 
    [SerializeField] private AudioSource alert;
    [SerializeField] private NoiseMaker noiseMaker;
    [SerializeField] private HealthController hc;
    [SerializeField] private AttackManager _attackManager;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator anim;

    private bool moving = false;
    private bool alive = true;
    
    private static readonly int Moving = Animator.StringToHash("Moving");

    private Coroutine wanderCoroutine;
    private Coroutine idleCoroutine;
    private Coroutine followTargetCoroutine;
    private Coroutine investigateCoroutine;
    private static readonly int Running = Animator.StringToHash("Running");

    private NavMeshPath path;
    
    void Start()
    {
        path = new NavMeshPath();
        Init();
        StartCoroutine(CapsuleCastAgainstUnits());
    }

    public void Init()
    {
        SpawnController.Instance.AddAiInput(this);
        
        currentTargetPosition = transform.position;
        
        if (aiState == State.Wander)
            Wander();
        else if (aiState == State.Idle)
            Idle();
        
        if (simpleWalker)
            StartCoroutine(SimpleWalker());   
    }

    void StopBehaviourCoroutines()
    {
        if (wanderCoroutine != null)
        {
            StopCoroutine(wanderCoroutine);
            wanderCoroutine = null;
        }
        if (idleCoroutine != null)
        {
            StopCoroutine(idleCoroutine);
            idleCoroutine = null;
        }
        
        if (followTargetCoroutine != null)
        {
            StopCoroutine(followTargetCoroutine);
            followTargetCoroutine = null;
        }
        
        if (investigateCoroutine != null)
        {
            StopCoroutine(investigateCoroutine);
            investigateCoroutine = null;
        }
        
        if (alertCoroutine != null)
        {
            StopCoroutine(alertCoroutine);
            alertCoroutine = null;
        }
    }


    void SetAgentDestinationTarget(Vector3 pos, bool expensive)
    {
        if (agent == null || agent.enabled == false)
            return;

        if (Vector3.Distance(GameManager.Instance.mainCamera.transform.position, pos) < 75)
            expensive = true;
        
        if (!expensive)
            agent.SetDestination(pos);
        else
        {
            NavMesh.CalculatePath(transform.position, pos, NavMesh.AllAreas, path);
            agent.SetPath(path);
        }
    }

    public void SetAggroMode(AggroMode newMode)
    {
        aggroMode = newMode;
        
        /*
        switch (aggroMode)
        {
            case AggroMode.AggroOnSight:
                aggroMode = AggroMode.AttackIfAttacked;                
                break;
            case AggroMode.AttackIfAttacked:
                aggroMode = AggroMode.AggroOnSight;                
                break;
        }*/
    }
    
    public void SeeEnemy(HealthController closestVisibleEnemy)
    {
        if (aggroMode == AggroMode.AggroOnSight)
        {
            SetAggro(closestVisibleEnemy); 
            RotateTowardsClosestEnemy(closestVisibleEnemy);   
        }
    }

    public void DamagedByEnemy(HealthController enemy)
    {
        SetAggro(enemy); 
        RotateTowardsClosestEnemy(enemy);   
        
        /*
        if (aggroMode == AggroMode.AttackIfAttacked)
        {
        }*/
    }

    public void OrderAttack(Vector3 newPos, HealthController unit)
    {
        StopBehaviourCoroutines();
        
        SetAggro(unit);
        RotateTowardsClosestEnemy(unit);   
        OrderMove(newPos);
    }
    
    public void OrderMove(Vector3 newPos)
    {
        StopBehaviourCoroutines();
        
        if (moveTowardsOrderTargetCoroutine != null)
            StopCoroutine(moveTowardsOrderTargetCoroutine);
        
        moveTowardsOrderTargetCoroutine = StartCoroutine(MoveToOrderTarget(newPos));
    }

    private Coroutine moveTowardsOrderTargetCoroutine;
    
    void SetAggro(HealthController damager)
    {
        if (debugLogs)
            print ("SetAggro");
        
        if (hc.Friends.Contains(damager) && Random.value < kidness)
            return;
        
        if (followTargetCoroutine == null && aiState != State.FollowTarget)
        {
            StopBehaviourCoroutines();
            
            if (PlayerInput.Instance && PlayerInput.Instance.gameObject == damager.gameObject)
            {
                PlayerInput.Instance.HC.AddEnemy(hc);
            }
            else if (damager.AiInput && damager.AiInput.inParty)
                damager.AddEnemy(hc);

            SetAgentDestinationTarget(damager.transform.position, false);
            
            if (followTargetCoroutine == null)
                followTargetCoroutine = StartCoroutine(FollowTarget());
            
            if (alertCoroutine != null)
                return;
            alertCoroutine = StartCoroutine(Alert()); 
        }
    }

    public IEnumerator HeardNoise(Vector3 noiseMakerPos, float distance)
    {
        if (inParty)
            yield break;
        
        if (!alive)
            yield break;
        
        if (aiState == State.FollowTarget || aiState == State.Investigate)
            yield break;
        
        StopBehaviourCoroutines();

        yield return new WaitForSeconds(distance / 20f);
        
        if (agent && agent.enabled)
            SetAgentDestinationTarget(noiseMakerPos, false);
        else
            yield break;
        
        investigateCoroutine = StartCoroutine(Investigate(noiseMakerPos));
        
        
        if (alertCoroutine != null)
            yield break;
        
        alertCoroutine = StartCoroutine(Alert());
    }


    private Coroutine alertCoroutine;
    IEnumerator Alert()
    {
        if (debugLogs)
            print("Alert");
        alert.gameObject.SetActive(false);
        yield return null;
        
        if (noiseMaker)
            noiseMaker.ShoutNoise();
        
        alert.gameObject.SetActive(true);
        alert.pitch = Random.Range(0.75f, 1.1f);
        alert.Play();
        yield return new WaitForSeconds(1f);
        alert.gameObject.SetActive(false);
        alertCoroutine = null;
    }
    
    IEnumerator SimpleWalker()
    {
        Vector3 previousPos = transform.position;
        while (true)
        {
            // IDLING
            if (Vector3.Distance(previousPos, transform.position) < 0.2f)
            {
                moving = false;
                anim.SetBool(Moving, false);
            }
            else // MOVING
            {
                moving = true;
                anim.SetBool(Moving, true);
            }
            previousPos = transform.position;
            
            if (attackCooldownCurrent > 0)
                attackCooldownCurrent -= 0.25f;
            else
                attackCooldownCurrent = 0;
            
            yield return new WaitForSeconds(0.25f);
        }
    }

    void Wander()
    {
        if (inParty == false)
            wanderCoroutine = StartCoroutine(WanderOverTime());
    }
    
    IEnumerator WanderOverTime()
    {
        aiState = State.Wander;
        currentTargetPosition = NewPositionNearPointOfInterest();

        if (debugLogs)
            print ("WanderOverTime");
        while (true)
        {
            if (Vector3.Distance(transform.position, currentTargetPosition) < stopDistance)
            {
                if (Random.value < 0.5)
                {
                    // GO TO A NEW POINT
                    Vector3 newTargetPosition = transform.position;
                    newTargetPosition =  NewPositionNearPointOfInterest();

                    currentTargetPosition = newTargetPosition;
                    if (agent && agent.enabled)
                        SetAgentDestinationTarget(newTargetPosition,false);   
                    else
                        yield break;
                }
                else
                {
                    // REST A BIT
                    Idle();
                    wanderCoroutine = null;
                    yield break;
                }
            }
            else
            {
                if (agent && agent.enabled)
                    SetAgentDestinationTarget(currentTargetPosition, false);
                else
                    yield break;
            }

            yield return new WaitForSeconds(updateRate);
        }
    }

    IEnumerator MoveToOrderTarget(Vector3 newPos)
    {
        aiState = State.MovingOnOrder;

        if (debugLogs)
            print ("WanderByPlayer");

        currentTargetPosition = newPos;
        
        while (true)
        {
            float newDistance = Vector3.Distance(transform.position, currentTargetPosition); 
            if (newDistance > stopDistanceAllyToPlayer)
            {
                if (agent && agent.enabled)
                {
                    if (newDistance > runDistanceThreshold && hc.enemiesInSight)
                    {
                        // RUN
                        anim.SetBool(Running, true);
                        agent.speed = runSpeed;
                    }
                    else
                    {
                        // WALK
                        anim.SetBool(Running, false);
                        agent.speed = walkSpeed;
                    }
                
                    agent.isStopped = false;
                    SetAgentDestinationTarget(currentTargetPosition, true);   
                }
                else
                    yield break;
            }
            else
            {
                anim.SetBool(Running, false);
                agent.speed = walkSpeed;
            }

            yield return new WaitForSeconds(updateRate);
        }
    }

    void Idle()
    {
        if (idleCoroutine != null)
            StopCoroutine(idleCoroutine);
                    
        idleCoroutine = StartCoroutine(Ideling());
    }

    IEnumerator Investigate(Vector3 investigationPoint)
    {
        aiState = State.Investigate;
        // WALK
        anim.SetBool(Running, false);
        agent.speed = walkSpeed;

        while (true)
        {
            if (alive == false)
                yield break;
            
            if (_attackManager.CanMove)
            {
                agent.isStopped = false;
                SetAgentDestinationTarget(investigationPoint,false);   
            }
            else
            {
                agent.isStopped = true;
            }

            yield return new WaitForSeconds(updateRate);
            
            if (alive == false)
                yield break;
            
            if (Vector3.Distance(transform.position, investigationPoint) <= stopDistance)
            {
                if (Random.value > 0.5f)
                    Idle();
                else
                    Wander();
                
                investigateCoroutine = null;
                yield break;
            }
        }
    }
    IEnumerator FollowTarget()
    {
        aiState = State.FollowTarget;

        while (true)
        {
            if (alive == false)
                yield break;
            
            currentTargetPosition = GetPositionOfClosestEnemy(true);
            
            if (_attackManager.CanMove)
            {
                if (Vector3.Distance(transform.position, currentTargetPosition) > runDistanceThreshold)
                {
                    // RUN
                    anim.SetBool(Running, true);
                    agent.speed = runSpeed;
                }
                else
                {
                    // WALK
                    anim.SetBool(Running, false);
                    agent.speed = walkSpeed;
                }
                
                agent.isStopped = false;
                SetAgentDestinationTarget(currentTargetPosition, false);   
            }
            else
            {
                agent.isStopped = true;
                agent.speed = walkSpeed;
            }

            yield return new WaitForSeconds(updateRate);
        }
    }

    Vector3 NewPositionNearPointOfInterest()
    {
        Vector3 newPos = AiNavigationManager.instance.GetPointOfInterestForUnit(hc); 
        
        NavMeshHit hit;
        float tries = 5;
        while (tries > 0)
        {
            NavMesh.SamplePosition(newPos, out hit, 10, groundMask);
            if (hit.hit)
            {
                newPos = hit.position;
                break;
            }
            tries--;
        }
        return newPos;
    }
    
    Vector3 GetPositionOfClosestEnemy(bool onlyVisible)
    {
        Vector3 newPos = transform.position;
        
        float distance = 1000;
        float newDistance = 0;
        HealthController closestEnemy = null;
        for (int i = 0; i < hc.Enemies.Count; i++)
        {
            if (hc.Enemies == null || (onlyVisible && hc.VisibleHCs.Contains(hc.Enemies[i]) == false) || hc.Enemies[i].Health <= 0)
                continue;
            
            newDistance = Vector3.Distance(transform.position, hc.Enemies[i].transform.position);
            if (newDistance < looseTargetDistance && newDistance < distance)
            {
                distance = newDistance;
                closestEnemy = hc.Enemies[i];
            }
        }

        if (closestEnemy != null)
            newPos = closestEnemy.transform.position;
        else
        {
            if (followTargetCoroutine != null)
            {
                StopCoroutine(followTargetCoroutine);
                followTargetCoroutine = null;
            }
            if (investigateCoroutine != null)
            {
                StopCoroutine(investigateCoroutine);
                investigateCoroutine = null;
            }
            Wander();
        }
        
        NavMeshHit hit;
        float tries = 5;
        while (tries > 0)
        {
            NavMesh.SamplePosition(newPos, out hit, 10, groundMask);
            if (hit.hit)
            {
                newPos = hit.position;
                break;
            }
            tries--;
        }
        return newPos;
    }
    
    IEnumerator Ideling()
    {
        if (debugLogs)
            print ("Ideling");
        aiState = State.Idle;
        yield return new WaitForSeconds(Random.Range(idleTimeMinMax.x, idleTimeMinMax.y));
        
        if (inParty == false)
            Wander();
    }

    private HealthController currentRotationTargetTransform;
    private Coroutine rotateTowardsClosestEnemyCoroutine;
    public void RotateTowardsClosestEnemy(HealthController newRotationTargetHc)
    {
        if (currentRotationTargetTransform == newRotationTargetHc)
            return;

        currentRotationTargetTransform = newRotationTargetHc;
        
        if (rotateTowardsClosestEnemyCoroutine != null)
        {
            StopCoroutine(rotateTowardsClosestEnemyCoroutine);
        }

        rotateTowardsClosestEnemyCoroutine = StartCoroutine(RotateTowardsClosestEnemyCoroutine(newRotationTargetHc));
    }

    private Quaternion targetRotation = Quaternion.identity;
    private Quaternion targetRotation1 = Quaternion.identity;
    IEnumerator RotateTowardsClosestEnemyCoroutine(HealthController targetHc)
    {
        float t = 0;
        while (true)
        {
            if (targetHc == null || targetHc.Health <= 0)
                yield break;

            t += Time.deltaTime;
            if (t >= 1)
            {
                t = 0;
                if (Vector3.Distance(transform.position, targetHc.transform.position) > looseTargetDistance)
                    yield break;
            }
            targetRotation1.SetLookRotation(targetHc.transform.position - transform.position); 
            targetRotation = Quaternion.Lerp(transform.rotation, targetRotation1, 10 * Time.deltaTime);
            transform.localEulerAngles = new Vector3(0, targetRotation.eulerAngles.y, 0);
            yield return null;
        }
    }
    IEnumerator CapsuleCastAgainstUnits()
    {
        while (hc.Health > 0)
        {
            if (attackCooldownCurrent <= 0)
            {
                Collider[] targetsInViewRadius = Physics.OverlapCapsule(transform.position + transform.forward,transform.position + transform.forward + Vector3.up * 2f ,1.5f, unitsMask);

                for (int i = 0; i < targetsInViewRadius.Length; i++)
                {
                    if (hc.EnemiesGameObjects.Contains(targetsInViewRadius[i].gameObject))
                    {
                        attackCooldownCurrent = Random.Range(attackCooldownMinMax.x, attackCooldownMinMax.y);
                        _attackManager.TryToAttack(false, null);
                    }
                }   
            }
            
            if (inParty)
                yield return null;
            else
                yield return new WaitForSeconds(0.1f);
        }
        
    }
    
    /*
    private void OnTriggerStay(Collider other)
    {
        if (!alive)
            return;
        
        if (attackCooldownCurrent > 0)
            return;
        
        if (other.gameObject.layer != 7)
            return;

        for (int i = hc.Enemies.Count - 1; i >= 0; i--)
        {
            if (hc.Enemies[i] == null || hc.Enemies[i].gameObject == null || hc.Enemies[i].Health <= 0 || hc.Friends.Contains(hc.Enemies[i]))
            {
                hc.RemoveEnemyAt(i);
                return;   
            }
            
            if (hc.Enemies[i].gameObject == other.gameObject)
            {
                attackCooldownCurrent = Random.Range(attackCooldownMinMax.x, attackCooldownMinMax.y);
                _attackManager.TryToAttack(false, null);
                
                StopBehaviourCoroutines();
                followTargetCoroutine = StartCoroutine(FollowTarget());
                return;
            }
        }
    }*/


    public void Death()
    {
        StopBehaviourCoroutines();
        alive = false;
    }

    public void Ressurect()
    {
        alive = true;
        Init();
    }
}