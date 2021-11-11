using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillsUi : MonoBehaviour
{
    public static SkillsUi Instance;

    [Header("Links To Skill Images")] 
    [SerializeField] private List<Image> skillImages;
    
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
        if (updateDirectionalSkillCoroutine != null)
            StopCoroutine(updateDirectionalSkillCoroutine);
        
        updateDirectionalSkillCoroutine = null;
        
        if (caster == null)
            return;
        
        State = SkillsUiState.AimDirectional;
        updateDirectionalSkillCoroutine = StartCoroutine(UpdateDirectionalSkill(caster, directionalSkill));   
    }

    public void StopAllAiming()
    {
        if (updateDirectionalSkillCoroutine != null)
        {
            StopCoroutine(updateDirectionalSkillCoroutine);
        }

        updateDirectionalSkillCoroutine = null;
        directionalSkillLineRenderer.positionCount = 0; 
        State = SkillsUiState.Null;
    }
    
    private Coroutine updateDirectionalSkillCoroutine;

    IEnumerator UpdateDirectionalSkill(HealthController caster, Skill skill)
    {
        if (caster == null)
        {
            updateDirectionalSkillCoroutine = null;
            yield break;
        }
        directionalSkillLineRenderer.positionCount = 2;
        while (true)
        {
            directionalSkillLineRenderer.material.mainTextureOffset += Vector2.left * Time.deltaTime;
            var targetPos = GameManager.Instance.MouseWorldGroundPosition();
            float curDistance = Vector3.Distance(caster.transform.position, targetPos);
            curDistance = Mathf.Clamp(curDistance, skill.minDistance, skill.maxDistance);
            Vector3 direction = targetPos - caster.transform.position;
            Vector3 point_C = caster.transform.position + (direction.normalized * curDistance);
            
            directionalSkillAimPositions[0] = caster.transform.position;
            directionalSkillAimPositions[1] = new Vector3(point_C.x, 0, point_C.z);
            directionalSkillLineRenderer.SetPositions(directionalSkillAimPositions);
            yield return null;
        }
    }

    public Vector3 GetAimTargetPosition(Vector3 mouseTargetPos)
    {
        switch (State) 
        { 
            case SkillsUiState.AimDirectional: return directionalSkillAimPositions[1]; 
            case SkillsUiState.AimAOE: return mouseTargetPos;
            
            default: return mouseTargetPos;
        }
    }

    public void SkillCooldownUI(Skill skill)
    {
        StartCoroutine(SkillCooldownUIOverTime(skill));
    }

    IEnumerator SkillCooldownUIOverTime(Skill skill)
    {
        var skillImage = skillImages[0];
        var initColor = skillImage.color;
        skillImage.color = Color.black;
        while (skill.OnCooldown)
        {
            skillImage.fillAmount = skill.uiFillAmount;
            yield return null;
        }
        skillImage.color = initColor;
    }
}
