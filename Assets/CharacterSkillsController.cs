using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterSkillsController : MonoBehaviour
{
    [SerializeField] private List<Skill> characterSkills = new List<Skill>();
    private Skill currentSkill;
    [SerializeField] private HealthController hc;
    
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
        
        float dist = Vector3.Distance(startPos, targetPos);
        float timeRaw = Mathf.InverseLerp(newSkill.minDistance, newSkill.maxDistance, dist);
        float timeResult = timeRaw * newSkill.actionTime + newSkill.actionTimeMin;
        timeResult = Mathf.Clamp(timeResult, newSkill.actionTimeMin, newSkill.actionTime);
        
        print("dist: " + dist + "; rawTime: " + timeRaw + "; _time: " + timeResult);
        
        NavMeshAgent agent = hc.Agent;
        hc.AiInput.StopAgent(true);
        agent.enabled = false;
        
        Rigidbody rb = hc.Rb;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.isKinematic = false;

        while (t < timeResult)
        {
            t += Time.fixedDeltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, t / timeResult);
            yield return new WaitForFixedUpdate();
        }
        
        rb.velocity = Vector3.zero;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        rb.isKinematic = true;
        
        agent.enabled = true;
        dashAttackCoroutine = null;
    }
}