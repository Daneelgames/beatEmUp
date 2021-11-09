using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterSkillsController : MonoBehaviour
{
    [SerializeField] private List<Skill> characterSkills = new List<Skill>();
    private Skill currentSkill;
    
    [SerializeField] private HealthController hc;
    [SerializeField] private Collider dashCollider;
    
    public List<Skill> CharacterSkills
    {
        get => characterSkills;
        set => characterSkills = value;
    }

    public void SetCurrentSkill(Skill skill)
    {
        currentSkill = skill;
    }

    public void UseSkill(Vector3 targetPos)
    {
        bool skillUsed = false;
        
        var newSkill = currentSkill;
        SkillsDatabaseManager.Instance.UnselectSkill();
        
        switch (newSkill.skill)
        {
          case Skill.SkillType.DashAttack:
              if (dashAttackCoroutine == null)
              {
                  skillUsed = true;
                  dashAttackCoroutine = StartCoroutine(DashAttack(newSkill, targetPos));
              }
              break;
        }
        if (skillUsed)
        {
            // deplete stamina
        }
    }

    private Coroutine dashAttackCoroutine;
    IEnumerator DashAttack(Skill newSkill, Vector3 targetPos)
    {
        float t = 0;
        Vector3 startPos = transform.position;
        
        dashCollider.gameObject.SetActive(true);
        
        NavMeshAgent agent = hc.Agent;
        hc.AiInput.StopAgent(true);
        agent.enabled = false;
        
        /*
        Rigidbody rb = hc.Rb;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.isKinematic = false;
        */
        while (t < newSkill.actionTime)
        {
            //rb.velocity = (targetPos - startPos).normalized * newSkill.skillPower;
            t += Time.fixedDeltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, t / newSkill.actionTime);
            yield return new WaitForFixedUpdate();
        }
        
        /*
        rb.velocity = Vector3.zero;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        rb.isKinematic = true;*/
        agent.enabled = true;
        dashCollider.gameObject.SetActive(false);
        dashAttackCoroutine = null;
    }
}