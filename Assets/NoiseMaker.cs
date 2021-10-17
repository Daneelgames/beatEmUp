using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseMaker : MonoBehaviour
{
    public bool makeStepNoise = false;
    public float walkStepNoiseDistance = 1;
    public float runStepNoiseDistance = 10;
    
    public void WalkStepNoise()
    {
        if (!makeStepNoise)
            return;
        
        SpawnController.Instance.MakeNoise(transform.position, walkStepNoiseDistance);
    }
    public void RunStepNoise()
    {
        if (!makeStepNoise)
            return;
        
        SpawnController.Instance.MakeNoise(transform.position, runStepNoiseDistance);
    }
}
