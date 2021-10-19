using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiInput : MonoBehaviour
{
    public enum State
    {
        Wander, Idle, FollowTarget, Alert
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
    [SerializeField] private float runDistanceThreshold = 5;
    [SerializeField] private float looseTargetDistance = 20;
    [SerializeField] private bool simpleWalker = true;
    [SerializeField] private Vector2 idleTimeMinMax = new Vector2(5, 30);
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Vector2 attackCooldownMinMax = new Vector2(0.5f, 3f);
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

    void Start()
    {
        Init();
    }

    public void Init()
    {
        SpawnController.Instance.AddAiInput(this);
        
        currentTargetPosition = transform.position;
        
        if (state == State.Wander)
            Wander();
        else if (state == State.Idle)
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
        
        if (aleartCoroutine != null)
        {
            StopCoroutine(aleartCoroutine);
            aleartCoroutine = null;
        }
    }
    
    public void SetAggro(HealthController damager)
    {
        if (hc.Friends.Contains(damager) && Random.value < kidness)
            return;
        
        if (state != State.FollowTarget)
        {
            StopBehaviourCoroutines();
            
            if (PlayerInput.Instance.gameObject == damager.gameObject)
            {
                PlayerInput.Instance.HC.AddEnemy(hc);
            }

            aleartCoroutine = StartCoroutine(Alert());
            
            agent.SetDestination(damager.transform.position);
            followTargetCoroutine = StartCoroutine(FollowTarget());
        }
    }

    public IEnumerator HeardNoise(Vector3 noiseMakerPos, float distance)
    {
        if (!alive)
            yield break;
        
        if (state == State.FollowTarget || state == State.Alert)
            yield break;
        
        StopBehaviourCoroutines();

        yield return new WaitForSeconds(distance / 20f);
        
        agent.SetDestination(noiseMakerPos);
        investigateCoroutine = StartCoroutine(Investigate(noiseMakerPos));
        
        aleartCoroutine = StartCoroutine(Alert());
    }


    private Coroutine aleartCoroutine;
    IEnumerator Alert()
    {
        alert.gameObject.SetActive(false);
        yield return null;
        
        if (noiseMaker)
            noiseMaker.ShoutNoise();
        
        alert.gameObject.SetActive(true);
        alert.pitch = Random.Range(0.75f, 1.1f);
        alert.Play();
        yield return new WaitForSeconds(1f);
        alert.gameObject.SetActive(false);
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
        wanderCoroutine = StartCoroutine(WanderOverTime());
    }
    
    IEnumerator WanderOverTime()
    {
        state = State.Wander;
        currentTargetPosition = NewPositionNearPointOfInterest();

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
                    agent.SetDestination(newTargetPosition);   
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
                agent.SetDestination(currentTargetPosition);   
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
        state = State.Alert;
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
                agent.SetDestination(investigationPoint);   
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
        state = State.FollowTarget;

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
                agent.SetDestination(currentTargetPosition);   
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
        Vector3 newPos =
            AiNavigationManager.instance.PointsOfInterest[Random.Range(0, AiNavigationManager.instance.PointsOfInterest.Count)].position + Random.insideUnitSphere * Random.Range(1, 5);
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
            if (onlyVisible && hc.VisibleHCs.Contains(hc.Enemies[i]) == false)
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
        state = State.Idle;
        yield return new WaitForSeconds(Random.Range(idleTimeMinMax.x, idleTimeMinMax.y));
        wanderCoroutine = StartCoroutine(WanderOverTime());
    }
    
    IEnumerator MoveTowardsTarget()
    {
        yield return new WaitForSeconds(updateRate);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!alive)
            return;
        
        if (attackCooldownCurrent > 0)
            return;
        
        if (other.gameObject.layer != 7)
            return;

        for (int i = 0; i < hc.Enemies.Count; i++)
        {
            if (hc.Enemies[i] == null || hc.Enemies[i].gameObject == null || hc.Enemies[i].Health < 0)
            {
                hc.RemoveEnemyAt(i);
                return;   
            }
            
            if (hc.Enemies[i].gameObject == other.gameObject)
            {
                attackCooldownCurrent = Random.Range(attackCooldownMinMax.x, attackCooldownMinMax.y);
                _attackManager.TryToAttack(false);
                return;
            }
        }
    }


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