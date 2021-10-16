using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiInput : MonoBehaviour
{
    [Header("Stats")] 
    [SerializeField] float updateRate = 0.5f;
    [SerializeField] private float stopDistance = 3;
    [SerializeField] private float looseTargetDistance = 20;
    [SerializeField] private bool simpleWalker = true;
    [SerializeField] private Vector2 idleTimeMinMax = new Vector2(5, 30);
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Vector2 attackCooldownMinMax = new Vector2(0.5f, 3f);
    private float attackCooldownCurrent = 0;

    private Vector3 currentTargetPosition;

    [Header("Links")]
    [SerializeField] private HealthController hc;
    [SerializeField] private AttackManager _attackManager;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator anim;

    private bool moving = false;
    private bool alive = true;
    
    enum State
    {
        Wander, Idle, FollowTarget
    }

    private State state = State.Wander;
    private static readonly int Moving = Animator.StringToHash("Moving");

    private Coroutine wanderCoroutine;
    private Coroutine idleCoroutine;
    private Coroutine followTargetCoroutine;
    
    // Start is called before the first frame update
    void Start()
    {
        currentTargetPosition = transform.position;
        
        Wander();
        
        if (simpleWalker)
            StartCoroutine(SimpleWalker());
    }

    public void Damaged(HealthController damager)
    {
        if (state != State.FollowTarget)
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

            if (PlayerInput.Instance.gameObject == damager.gameObject)
            {
                PlayerInput.Instance.HC.AddEnemy(hc);
            }
            
            followTargetCoroutine = StartCoroutine(FollowTarget());
        }
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
                    if (idleCoroutine != null)
                        StopCoroutine(idleCoroutine);
                    
                    idleCoroutine = StartCoroutine(Idle());

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
    IEnumerator FollowTarget()
    {
        state = State.FollowTarget;

        while (true)
        {
            currentTargetPosition = GetPositionOfClosestEnemy();
            
            if (_attackManager.CanMove)
            {
                agent.isStopped = false;
                agent.SetDestination(currentTargetPosition);   
            }
            else
                agent.isStopped = true;

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
    
    Vector3 GetPositionOfClosestEnemy()
    {
        Vector3 newPos = transform.position;
        
        float distance = 1000;
        float newDistance = 0;
        HealthController closestEnemy = null;
        for (int i = 0; i < hc.Enemies.Count; i++)
        {
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
    
    IEnumerator Idle()
    {
        state = State.Wander;
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
                _attackManager.TryToAttack();
                return;
            }
        }
    }

    public void Death()
    {
        StopAllCoroutines();
        alive = false;
    }
}