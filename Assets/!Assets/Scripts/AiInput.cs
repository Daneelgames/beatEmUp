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
    public bool ally = false;

    [Header("Follow the Leader behaviour")] 
    [SerializeField] private bool leader = false;
    public bool Leader { get => leader; set => leader = value; }
    
    [SerializeField] private HealthController leaderToFollow;
    public HealthController LeaderToFollow
    {
        get => leaderToFollow;
        set => leaderToFollow = value;
    }
    
    [SerializeField] private bool canCreateGroupOnRuntime = true;

    public bool CanCreateGroupOnRuntime
    {
        get => canCreateGroupOnRuntime;
        set => canCreateGroupOnRuntime = value;
    } 

    [SerializeField] private bool canJoinGroupOnRuntime = true;
    public bool CanJoinGroupOnRuntime { get => canJoinGroupOnRuntime; set => canJoinGroupOnRuntime = value; } 
    [SerializeField] List<HealthController> followersCurrent = new List<HealthController>();
    [SerializeField] int followersAmountMax = 5;

    public List<HealthController> FollowersCurrent { get => followersCurrent; set => followersCurrent = value; }

    public int FollowersAmountMax { get => followersAmountMax; set => followersAmountMax = value; }
    

    [SerializeField] private float maxDistanceFromLeader = 50;
    [SerializeField] private float followLeaderUpdateDelay = 0.5f;
    
    public enum AggroMode
    {
        AggroOnSight, AttackIfAttacked, Flee
    }

    [SerializeField] private AggroMode _aggroMode = AggroMode.AggroOnSight;

    public AggroMode aggroMode
    {
        get => _aggroMode;
        set => _aggroMode = value;
    }

    public enum State
    {
        Wander, Idle, FollowTarget, Investigate, MovingOnOrder, MovingToThrow
    }

    [SerializeField] private State state = State.Wander;
    
    public State aiState
    {
        get => state;
        set => state = value;
    }

    [Header("Stats")] 
    [SerializeField] private float aggroDistance = 30;
    public float AggroDistance => aggroDistance;
    
    [SerializeField]
    [Range(0f, 1f)] private float kidness = 0.5f;
    public float Kidness => kidness;

    [SerializeField]
    private bool hears = true;
    public bool Hears => hears;
    float updateRate = 0.5f;
    [SerializeField] private float walkSpeed = 2;
    [SerializeField] private float runSpeed = 4;
    [SerializeField] private float stopDistance = 3;
    [SerializeField] private float stopDistanceInvestigation = 3;
    [SerializeField] private float stopDistanceAllyToPlayer = 5;
    [SerializeField] private float stopDistanceFollowTarget = 2;
    [SerializeField] private float runDistanceThreshold = 5;
    [SerializeField] private float looseTargetDistance = 20;
    [SerializeField] private float rotateToTargetMaxDistance = 10;
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

    [Header("Private logic")]
    private bool moving = false;
    private bool alive = true;
    
    private static readonly int Moving = Animator.StringToHash("Moving");

    private Coroutine wanderCoroutine;
    private Coroutine idleCoroutine;
    private Coroutine followTargetCoroutine;
    private Coroutine investigateCoroutine;
    private static readonly int Running = Animator.StringToHash("Running");

    private NavMeshPath path;
    
    private Quaternion targetRotation = Quaternion.identity;
    private Quaternion targetRotation1 = Quaternion.identity;
    private HealthController currentTargetRotationHc;
    void Start()
    {
        path = new NavMeshPath();
        Init();
        StartCoroutine(CapsuleCastAgainstUnits());
    }

    public void SetAnimatorParameterBySkill(int newStringToHash, bool active)
    {
        anim.SetBool(newStringToHash, active);
    }
    
    void SetAnimatorParameter(int newStringToHash, bool active)
    {
        if (hc.CharacterSkillsController && hc.CharacterSkillsController.PerformingSkill != null && hc.CharacterSkillsController.PerformingSkill.skill != Skill.SkillType.Null)
            return;
        
        anim.SetBool(newStringToHash, active);
    }
    public void Init()
    {
        SpawnController.Instance.AddAiInput(this);
        
        currentTargetPosition = transform.position;

        if (looseTargetDistance < hc.FieldOfView.ViewRadius)
            looseTargetDistance = hc.FieldOfView.ViewRadius;
            
        if (aiState == State.Wander)
            Wander();
        else if (aiState == State.Idle)
            Idle();
        
        if (simpleWalker)
        {
            StartCoroutine(SimpleWalker());
            StartCoroutine(BurnEnergyByMovement());
        }

        StartCoroutine(DistanceToLeader());
    }

    IEnumerator DistanceToLeader()
    {
        while (GameManager.Instance.Units.Contains(hc) == false)
        {
            yield return null;
        }

        for (int i = 0; i < GameManager.Instance.Units.IndexOf(hc); i++)
        {
            yield return null;
        }
        while (true)
        {
            if (leaderToFollow)
            {
                if (leaderToFollow.Health > 0)
                {
                    leaderToFollow = null;
                    Wander();
                    yield break;
                }
                
                float distance = Vector3.Distance(transform.position, leaderToFollow.transform.position); 
                if (distance >= maxDistanceFromLeader)
                {
                    StopBehaviourCoroutines();
                    SetNavMeshAgentSpeed(runSpeed);
                    SetAgentDestinationTarget(NewPositionNearLeader() , false);
                }
            }
            yield return new WaitForSeconds(followLeaderUpdateDelay);
        }
    }
    
    public void StopBehaviourCoroutines()
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
        if (moveTowardsOrderTargetCoroutine != null)
        {
            StopCoroutine(moveTowardsOrderTargetCoroutine);
            moveTowardsOrderTargetCoroutine = null;
        }
        
        if (throwCoroutine != null)
        {
            StopCoroutine(throwCoroutine);
            throwCoroutine = null;
        }
    }

    void SetAgentDestinationTarget(Vector3 pos, bool expensive)
    {
        if (agent == null || agent.enabled == false)
            return;

        currentTargetPosition = pos;
        
        if (Vector3.Distance(GameManager.Instance.mainCamera.transform.position, currentTargetPosition) < 75)
            expensive = true;
        
        if (!expensive)
            agent.SetDestination(currentTargetPosition);
        else
        {
            NavMesh.CalculatePath(transform.position, currentTargetPosition, NavMesh.AllAreas, path);
            agent.SetPath(path);
        }
    }

    public void SetAggroMode(AggroMode newMode)
    {
        aggroMode = newMode;
        
    }
    
    public void SeeEnemy(HealthController closestVisibleEnemy, float distanceToEnemy)
    {
        if (aggroMode == AggroMode.AggroOnSight)
        {
            if (distanceToEnemy <= aggroDistance)
            {
                SetAggro(closestVisibleEnemy);
                RotateTowardsClosestEnemy(closestVisibleEnemy);   
            }
            else
            {
                StartCoroutine(HeardNoise(closestVisibleEnemy.transform.position, distanceToEnemy));
            }   
        }
        else if (aggroMode == AggroMode.Flee)
        {
            RunAway(closestVisibleEnemy.transform.position);
        }
    }

    void RunAway(Vector3 pointRunAwayFrom)
    {
        StopBehaviourCoroutines();
    }

    public void DamagedByEnemy(HealthController enemy)
    {
        SetAggro(enemy); 
        RotateTowardsClosestEnemy(enemy);   
    }

    public void OrderThrow(Vector3 newPos, int itemIndex)
    {
        StopBehaviourCoroutines();
        if (throwCoroutine != null)
            StopCoroutine(throwCoroutine);
        
        throwCoroutine = StartCoroutine(ThrowOrder(newPos, itemIndex));
    }

    private Coroutine throwCoroutine;
    IEnumerator ThrowOrder(Vector3 targetPos, int itemIndex)
    {
        state = State.MovingToThrow;
        // get selected item index in inventory
        // get throw distance
        float throwDistance = hc.AttackManager.ThrowDistance;
        float newDistance = Vector3.Distance(hc.transform.position, targetPos);
                
        while (newDistance > throwDistance)
        {
            StopAgent(false);
            //print("UNIT WALKS CLOSER TO THROW!");
            SetNavMeshAgentSpeed(runSpeed);
            SetAgentDestinationTarget(targetPos, inParty);
            yield return new WaitForSeconds(0.5f);
            
            newDistance = Vector3.Distance(hc.transform.position, targetPos);
        }
        
        SetAgentDestinationTarget(transform.position, false);
        
        ItemsDatabaseManager.Instance.ThrowItemFromInventory(hc, itemIndex, targetPos);
        throwCoroutine = null;
        Idle();
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
        if (ally && !inParty)
            print("OrderMove");

        StopBehaviourCoroutines();
        
        if (rotateTowardsClosestEnemyCoroutine != null)
        {
            StopCoroutine(rotateTowardsClosestEnemyCoroutine);
            rotateTowardsClosestEnemyCoroutine = null;
            currentTargetRotationHc = null;
        }
        
        if (moveTowardsOrderTargetCoroutine != null)
            StopCoroutine(moveTowardsOrderTargetCoroutine);
        
        moveTowardsOrderTargetCoroutine = StartCoroutine(MoveToOrderTarget(newPos));
    }

    private Coroutine moveTowardsOrderTargetCoroutine;
    
    void SetAggro(HealthController damager)
    {
        if (debugLogs && ally && !inParty)
            print("SetAggro");
        
        if (hc.Friends.Contains(damager) && Random.value < Kidness)
            return;

        if (aggroMode == AggroMode.Flee)
        {
            // SET FLEE MODE
            return;
        }
        
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
            alertCoroutine = StartCoroutine(AlertOverTime()); 
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
        
        StopBehaviourCoroutines();
        investigateCoroutine = StartCoroutine(Investigate(noiseMakerPos));
        
        
        if (alertCoroutine != null)
            yield break;
        
        alertCoroutine = StartCoroutine(AlertOverTime());
    }

    public void Alert()
    {
        if (alertCoroutine != null)
            return;
        
        alertCoroutine = StartCoroutine(AlertOverTime());
    }
    
    private Coroutine alertCoroutine;
    IEnumerator AlertOverTime()
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
        while (hc.Health > 0)
        {
            // IDLING
            if (Vector3.Distance(previousPos, transform.position) < 0.2f)
            {
                moving = false;
                SetAnimatorParameter(Moving, false);
            }
            else // MOVING
            {
                moving = true;
                SetAnimatorParameter(Moving, true);
            }
            previousPos = transform.position;
            
            if (attackCooldownCurrent > 0)
                attackCooldownCurrent -= 0.25f;
            else
                attackCooldownCurrent = 0;
            
            if (!inParty)
                yield return new WaitForSeconds(0.25f);
            else
                yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator BurnEnergyByMovement()
    {
        while (hc.Health > 0)
        {
            if (moving)
                hc.BurnEnergyByMovement();
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    void Wander()
    {
        if (debugLogs && ally && !inParty)
            print("Wander");
        
        StopBehaviourCoroutines();
        
        if (inParty == false)
            wanderCoroutine = StartCoroutine(WanderOverTime());
    }
    
    IEnumerator WanderOverTime()
    {
        aiState = State.Wander;
        StopAgent(false);
        currentTargetPosition = NewPositionNearPointOfInterest();

        while (true)
        {
            float distance = Vector3.Distance(transform.position, currentTargetPosition); 
            if (debugLogs)
                print ("WanderOverTime; distance " + distance);
            
            if (distance < stopDistance)
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
                {
                    if (distance < runDistanceThreshold)
                    {
                        SetNavMeshAgentSpeed(walkSpeed);
                        SetAnimatorParameter(Running, false);
                    }
                    else
                    {
                        SetNavMeshAgentSpeed(runSpeed);
                        SetAnimatorParameter(Running, true);
                    }
                    
                    SetAgentDestinationTarget(currentTargetPosition, false);
                }
                else
                    yield break;
            }

            yield return new WaitForSeconds(updateRate);
        }
    }

    IEnumerator MoveToOrderTarget(Vector3 newPos)
    {
        StopAgent(false);
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
                    if (newDistance > runDistanceThreshold)
                    {
                        // RUN  
                        SetAnimatorParameter(Running, true);
                        SetNavMeshAgentSpeed(runSpeed);
                    }
                    else
                    {
                        // WALK
                        SetAnimatorParameter(Running, false);
                        SetNavMeshAgentSpeed(walkSpeed);
                        
                    }
                
                    agent.isStopped = false;
                    SetAgentDestinationTarget(currentTargetPosition, true);   
                }
                else
                    yield break;
            }
            else
            {
                SetAnimatorParameter(Running, false);
                SetNavMeshAgentSpeed(walkSpeed);
            }

            yield return new WaitForSeconds(updateRate);
        }
    }

    public void Idle()
    {
        if (debugLogs && ally && !inParty)
            print("Idle");

        StopBehaviourCoroutines();
        
        if (idleCoroutine != null)
            StopCoroutine(idleCoroutine);
                    
        idleCoroutine = StartCoroutine(Ideling());
    }

    IEnumerator Investigate(Vector3 investigationPoint)
    {
        StopAgent(false);
        aiState = State.Investigate;
        // WALK
        SetAnimatorParameter(Running, false);
        SetNavMeshAgentSpeed(walkSpeed);

        while (true)
        {
            if (alive == false)
                yield break;
            
            if (_attackManager.CanMove)
            {
                if (agent.enabled)
                    agent.isStopped = false;
                
                SetAgentDestinationTarget(investigationPoint,false);   
            }
            else
            {
                if (agent.enabled)
                    agent.isStopped = true;
            }

            yield return new WaitForSeconds(updateRate);
            
            if (alive == false)
                yield break;
            
            if (Vector3.Distance(transform.position, investigationPoint) <= stopDistanceInvestigation)
            {
                if (Random.value > 0.3f)
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
        if (debugLogs)
            print ("FollowTarget");
        aiState = State.FollowTarget;

        StopAgent(false);
        while (true)
        {
            if (alive == false)
                yield break;
            
            currentTargetPosition = GetPositionOfClosestEnemy(true);
            
            if (_attackManager.CanMove)
            {
                float currentDistance = Vector3.Distance(transform.position, currentTargetPosition); 
                if (currentDistance > runDistanceThreshold)
                {
                    // RUN
                    SetAnimatorParameter(Running, true);
                    SetNavMeshAgentSpeed(runSpeed);
                }
                else
                {
                    // WALK
                    SetAnimatorParameter(Running, false);
                    SetNavMeshAgentSpeed(walkSpeed);
                }
                
                if (currentDistance > stopDistanceFollowTarget)
                {
                    agent.isStopped = false;
                    SetAgentDestinationTarget(currentTargetPosition, false);
                }   
            }
            else
            {
                agent.isStopped = true;
                SetNavMeshAgentSpeed(walkSpeed);
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
    Vector3 NewPositionNearLeader()
    {
        Vector3 newPos = LeaderToFollow.transform.position + Random.insideUnitSphere * 5; 
        
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
        if (currentTargetRotationHc != null && currentTargetRotationHc.Health > 0)
            newPos = currentTargetRotationHc.transform.position;
        else if (hc.VisibleEnemiesHCs.Count > 0)
            newPos = hc.VisibleEnemiesHCs[Random.Range(0, hc.VisibleEnemiesHCs.Count)].transform.position;
        
        float distance = 1000;
        float newDistance = 0;
        HealthController closestEnemy = null;
        for (int i = hc.Enemies.Count - 1; i >= 0; i--)
        {
            if (hc.Enemies[i].Health <= 0)
            {
                hc.RemoveEnemyAt(i);
                continue;
            }
            
            if (hc.Enemies == null || (onlyVisible && hc.VisibleHCs.Contains(hc.Enemies[i]) == false))
                continue;
            
            newDistance = Vector3.Distance(transform.position, hc.Enemies[i].transform.position);
            if (newDistance < looseTargetDistance && newDistance < distance)
            {
                distance = newDistance;
                closestEnemy = hc.Enemies[i];
            }
        }
        
        if (closestEnemy != null)
        {
            newPos = closestEnemy.transform.position;   
        }
        else
        {
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
            
            if (tries <= 0 && debugLogs)
                print("CAN'T FIND POSITION NEAR CLOSEST ENEMY");
        }
        return newPos;
    }

    public void StopAgent(bool isStopped)
    {
        if (agent && agent.enabled)
            agent.isStopped = isStopped;
    }
    
    IEnumerator Ideling()
    {
        if (debugLogs)
            print ("Ideling");
        aiState = State.Idle;
        StopAgent(true);
        
        yield return new WaitForSeconds(Random.Range(idleTimeMinMax.x, idleTimeMinMax.y));
        
        if (inParty == false)
            Wander();
    }

    private Coroutine rotateTowardsClosestEnemyCoroutine;
    public void RotateTowardsClosestEnemy(HealthController newRotationTargetHc)
    {
        if (currentTargetRotationHc == newRotationTargetHc)
            return;

        currentTargetRotationHc = newRotationTargetHc;
        
        if (rotateTowardsClosestEnemyCoroutine != null)
        {
            StopCoroutine(rotateTowardsClosestEnemyCoroutine);
        }

        rotateTowardsClosestEnemyCoroutine = StartCoroutine(RotateTowardsClosestEnemyCoroutine());
    }

    IEnumerator RotateTowardsClosestEnemyCoroutine()
    {
        bool canRotate = false;
        float t = 0;
        while (true)
        {
            if (currentTargetRotationHc == null || currentTargetRotationHc.Health <= 0)
            {
                rotateTowardsClosestEnemyCoroutine = null;
                yield break;
            }

            t += Time.deltaTime;
            if (t >= 1)
            {
                t = 0;
                float distance = Vector3.Distance(transform.position, currentTargetRotationHc.transform.position); 
                if (distance > looseTargetDistance)
                {
                    rotateTowardsClosestEnemyCoroutine = null;
                    yield break;
                }
                
                if (distance <= rotateToTargetMaxDistance)
                    canRotate = true;
                else
                    canRotate = false;
            }
            
            if (debugLogs)
                print("RotateTowardsClosestEnemyCoroutine; canRotate " + canRotate);
            
            targetRotation1.SetLookRotation(currentTargetRotationHc.transform.position - transform.position); 
            targetRotation = Quaternion.Slerp(transform.rotation, targetRotation1, 10 * Time.deltaTime);
            
            if (canRotate)
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

    void SetNavMeshAgentSpeed(float newSpeed)
    {
        if (hc.CharacterPerksController.Fast)
            newSpeed *= 1.5f;
        else if (hc.CharacterPerksController.Slow)
            newSpeed *= 0.75f;

        if (hc.CharacterPerksController.CurrentDiscomfort >= 3)
        {
            newSpeed *= 0.5f;
        }
        else if (hc.CharacterPerksController.CurrentDiscomfort >= 2)
        {
            newSpeed *= 0.75f;
        }
        else if (hc.CharacterPerksController.CurrentDiscomfort > 0)
        {
            newSpeed *= 0.9f;
        }
        
        agent.speed = newSpeed;
    }


    public void Death()
    {
        StopAgent(true);
        if (rotateTowardsClosestEnemyCoroutine != null)
        {
            StopCoroutine(rotateTowardsClosestEnemyCoroutine);
            rotateTowardsClosestEnemyCoroutine = null;
        }
        
        StopBehaviourCoroutines();
        alive = false;
    }

    public void Ressurect()
    {
        alive = true;
        Init();
    }
}