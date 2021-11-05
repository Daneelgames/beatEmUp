using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseMaker : MonoBehaviour
{
    public HealthController hc;
    
    public bool makeStepNoise = false;
    public float walkStepNoiseDistance = 1;
    public float runStepNoiseDistance = 10;
    public bool makeAttackNoise = false;
    public float attackNoiseDistance = 10;
    public bool makeShotNoise = false;
    public float shotNoiseDistance = 25;
    
    public bool shouts = false;
    public float shoutNoiseDistance = 20;
    
    public void WalkStepNoise()
    {
        if (!makeStepNoise)
            return;
        
        SpawnController.Instance.MakeNoise(transform.position, walkStepNoiseDistance, hc);
    }
    public void RunStepNoise()
    {
        if (!makeStepNoise)
            return;
        
        SpawnController.Instance.MakeNoise(transform.position, runStepNoiseDistance, hc);
    }

    public void AttackNoise()
    {
        if (!makeAttackNoise)
            return;
        
        SpawnController.Instance.MakeNoise(transform.position, attackNoiseDistance, hc);
    }
    public void ShotNoise()
    {
        if (!makeAttackNoise)
            return;
        
        SpawnController.Instance.MakeNoise(transform.position, shotNoiseDistance, hc);
    }
    
    public void ShoutNoise()
    {
        if (!shouts)
            return;
        
        SpawnController.Instance.MakeNoise(transform.position, shoutNoiseDistance, hc);
    }
}
