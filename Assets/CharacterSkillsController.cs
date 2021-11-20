using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterSkillsController : MonoBehaviour
{
    [SerializeField] private List<Skill> characterSkills = new List<Skill>();
    [SerializeField]
    private int selectedSkillIndex;
    
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
    
    public void SetSelectedSkill(int skillIndex)
    {
        selectedSkillIndex = skillIndex;
    }

    public void PerformSkill(Vector3 targetPos)
    {
        bool skillUsed = false;

        PerformingSkill = new Skill(CharacterSkills[selectedSkillIndex]);

        switch (PerformingSkill.skill)
        {
          case Skill.SkillType.DashAttack:
              if (dashAttackCoroutine == null)
              {
                  skillUsed = true;
                  dashAttackCoroutine = StartCoroutine(DashAttack(PerformingSkill, targetPos));
              }
              break;
        }
        if (skillUsed)
        {
            // deplete stamina
            
            hc.Energy -= PerformingSkill.energyCost;
            
            if (PartyInputManager.Instance.SelectedAllyUnits[0] == hc)
                PartyUi.Instance.UseEnergyFeedback();
            
            // skill cooldown
            StartCoroutine(SkillCooldown(selectedSkillIndex));
            SkillsUi.Instance.SkillCooldownUI(CharacterSkills[selectedSkillIndex]);
        }
        
        SkillsDatabaseManager.Instance.UnselectSkill();
    }

    IEnumerator SkillCooldown(int newSkillIndex)
    {
        var newSkill = characterSkills[newSkillIndex];
        float t = newSkill.skillCooldown;

        newSkill.OnCooldown = true;
        
        while (t > 0)
        {
            newSkill.uiFillAmount = 1 - t / newSkill.skillCooldown;
            t -= Time.deltaTime;
            yield return null;
        }
        newSkill.uiFillAmount = 1;
        newSkill.OnCooldown = false;
    }
    

    IEnumerator DashAttack(Skill newSkill, Vector3 targetPos)
    {
        //PerformingSkill = newSkill;
        
        if (hc.AiInput)
        {
            hc.AiInput.StopBehaviourCoroutines();
            hc.AiInput.SetAnimatorParameterBySkill(Dash, true);
            hc.AiInput.StopAgent(true);
        }
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
        if (agent)
            agent.enabled = false;
        
        transform.LookAt(targetPos, Vector3.up);
        
        Rigidbody rb = hc.Rb;
        if (rb)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.isKinematic = false;
        }
        
        while (t < timeResult)
        {
            t += Time.fixedDeltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, t / timeResult);
            yield return new WaitForFixedUpdate();
        }
        
        if (rb)
        {
            rb.velocity = Vector3.zero;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rb.isKinematic = true;
        }
        
        if (agent)
            agent.enabled = true;
        
        dashAttackCoroutine = null;
        
        if (hc.AiInput)
            hc.AiInput.SetAnimatorParameterBySkill(Dash, false);
        
        hc.BodyPartsManager.SetAllBodyPartsDangerous(false);
        PerformingSkill.skill = Skill.SkillType.Null;
    }
}