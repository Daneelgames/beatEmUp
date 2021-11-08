using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillsUi : MonoBehaviour
{
    public static SkillsUi Instance;

    
    public enum SkillsUiState
    {
        Null, AimDirectional, AimAOE
    }

    [SerializeField] private SkillsUiState state = SkillsUiState.Null;

    public SkillsUiState State
    {
        get => state;
        set => state = value;
    } 

    [SerializeField]


    private LineRenderer directionalSkillLineRenderer;
    private Vector3[] directionalSkillAimPositions = new Vector3[2];
    
    private void Awake()
    {
        Instance = this;
    }

    public void AimDirectionalSkill(HealthController caster, Skill directionalSkill)
    {
        if (updateDirectionalSkill != null)
            StopCoroutine(updateDirectionalSkill);
        
        State = SkillsUiState.AimDirectional;
        
        updateDirectionalSkill = StartCoroutine(UpdateDirectionalSkill(caster, directionalSkill));
    }

    public void StopAllAiming()
    {
        if (updateDirectionalSkill != null)
        {
            StopCoroutine(updateDirectionalSkill);
            directionalSkillLineRenderer.positionCount = 0;
        }
        //print("StopAllAiming");
        State = SkillsUiState.Null;
    }
    
    private Coroutine updateDirectionalSkill;

    IEnumerator UpdateDirectionalSkill(HealthController caster, Skill skill)
    {
        directionalSkillLineRenderer.positionCount = 2;
        while (true)
        {
            var targetPos = GameManager.Instance.MouseWorldGroundPosition();
            Vector3 direction = targetPos - caster.transform.position;
            Vector3 point_C = caster.transform.position + (direction.normalized * skill.maxDistance);
            
            directionalSkillAimPositions[0] = caster.transform.position;
            directionalSkillAimPositions[1] = point_C;
            directionalSkillLineRenderer.SetPositions(directionalSkillAimPositions);
            yield return null;
        }
    }
}
