using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnController : MonoBehaviour
{
    public static SpawnController Instance;

    private List<AiInput> spawnedAiInputs = new List<AiInput>();
    private void Awake()
    {
        Instance = this;
    }

    public void AddAiInput(AiInput newAi)
    {
        spawnedAiInputs.Add(newAi);
    }

    public void MakeNoise(Vector3 noiseMakerPos, float maxDistance)
    {
        for (int i = 0; i < spawnedAiInputs.Count; i++)
        {
            if (spawnedAiInputs[i].Hears == false)
                continue;
            
            float newDistance = Vector3.Distance(noiseMakerPos, spawnedAiInputs[i].transform.position);

            if (newDistance <= maxDistance)
            {
                spawnedAiInputs[i].HeardNoise(noiseMakerPos);
            }
        }
    }
}
