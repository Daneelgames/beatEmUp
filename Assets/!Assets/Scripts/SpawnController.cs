using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnController : MonoBehaviour
{
    [SerializeField] private List<Interactable> _interactables;
    [SerializeField] private List<GameObject> _interactablesGameObjects;

    public List<Interactable> Interactables => _interactables;
    public List<GameObject> InteractablesGameObjects => _interactablesGameObjects;

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
                StartCoroutine(spawnedAiInputs[i].HeardNoise(noiseMakerPos, newDistance));
            }
        }
    }

    public void AddInteractable(Interactable newInteractable)
    {
        if (Interactables.Contains(newInteractable) == false)
        {
            Interactables.Add(newInteractable);
            InteractablesGameObjects.Add(newInteractable.gameObject);
        }
    }
    
    public void InteractableDestroyed(Interactable destroyedInteractable)
    {
        if (Interactables.Contains(destroyedInteractable))
        {
            Interactables.Remove(destroyedInteractable);
            InteractablesGameObjects.Remove(destroyedInteractable.gameObject);
        }
    }

    public Interactable GetClosestInteractable(Vector3 newPos, float maxDistance)
    {
        Interactable closest = null;

        
        for (int i = 0; i < _interactables.Count; i++)
        {
            float newDistance = Vector3.Distance(newPos, _interactables[i].transform.position);
            if (newDistance <= maxDistance)
            {
                maxDistance = newDistance;
                closest = _interactables[i];
            }
        }
        return closest;
    }
}
