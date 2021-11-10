using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterSkillsController : MonoBehaviour
{
    [SerializeField] private List<Skill> characterSkills = new List<Skill>();
    [SerializeField]
    private Skill selectedSkill;
    
    [SerializeField]
    private Skill performingSkill;
    public Skill PerformingSkill
    {
        get {return performingSkill;}
        set {performingSkill = value;}
    }

    public List<Skill> CharacterSkills { get => characterSkills; set => characterSkills = value; }
    
    private Coroutine dashAttackCoroutine;
    private static readonly int Dash = Animator.StringToHash("Dash");

    [SerializeField] private HealthController hc;
    
    [Header("Particles")]
    [SerializeField] ParticleSystem dashParticles;

    public void SetSelectedSkill(Skill skill)
    {
        selectedSkill = new Skill(skill);
    }

    public void PerformSkill(Vector3 targetPos)
    {
        bool skillUsed = false;
        
        var newSkill = selectedSkill;
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

    IEnumerator DashAttack(Skill newSkill, Vector3 targetPos)
    {
        PerformingSkill = newSkill;
        hc.AiInput.StopBehaviourCoroutines();
        hc.AiInput.SetAnimatorParameterBySkill(Dash, true);
        hc.BodyPartsManager.SetAllBodyPartsDangerous(true);
        hc.AttackManager.ClearDamaged();

        dashParticles.Play(true);
        
        float t = 0;
        Vector3 startPos = transform.position;
        
        float dist = Vector3.Distance(startPos, targetPos);
        float timeRaw = Mathf.InverseLerp(newSkill.minDistance, newSkill.maxDistance, dist);
        float timeResult = timeRaw * newSkill.actionTime + newSkill.actionTimeMin;
        timeResult = Mathf.Clamp(timeResult, newSkill.actionTimeMin, newSkill.actionTime);
        
        NavMeshAgent agent = hc.Agent;
        hc.AiInput.StopAgent(true);
        agent.enabled = false;
        
        transform.LookAt(targetPos, Vector3.up);
        
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
        
        hc.AiInput.SetAnimatorParameterBySkill(Dash, false);
        hc.BodyPartsManager.SetAllBodyPartsDangerous(false);
        PerformingSkill.skill = Skill.SkillType.Null;
    }
}