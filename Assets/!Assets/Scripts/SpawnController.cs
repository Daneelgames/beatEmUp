using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class SpawnController : MonoBehaviour
{
    [SerializeField] private List<Interactable> _interactables;
    [SerializeField] private List<GameObject> _interactablesGameObjects;

    public List<Interactable> Interactables => _interactables;
    public List<GameObject> InteractablesGameObjects => _interactablesGameObjects;

    public static SpawnController Instance;

    private List<AiInput> spawnedAiInputs = new List<AiInput>();
    private List<ActivateRigidbodyOnNoise> _activateRigidbodyOnNoises = new List<ActivateRigidbodyOnNoise>();

    private void Awake()
    {
        Instance = this;
    }

    public void AddAiInput(AiInput newAi)
    {
        spawnedAiInputs.Add(newAi);
    }

    public void AddActivateRigidbodyOnNoise(ActivateRigidbodyOnNoise newActivateRigidbodyOnNoise)
    {
        _activateRigidbodyOnNoises.Add(newActivateRigidbodyOnNoise);
    }

    public void MakeNoise(Vector3 noiseMakerPos, float maxDistance, HealthController noiseMaker)
    {
        for (int i = 0; i < spawnedAiInputs.Count; i++)
        {
            if (spawnedAiInputs[i].Hears == false || (noiseMaker && noiseMaker.AiInput.ally == spawnedAiInputs[i].ally))
                continue;
            
            float newDistance = Vector3.Distance(noiseMakerPos, spawnedAiInputs[i].transform.position);

            if (newDistance <= maxDistance)
            {
                StartCoroutine(spawnedAiInputs[i].HeardNoise(noiseMakerPos, newDistance));
            }
        }

        for (int i = 0; i < _activateRigidbodyOnNoises.Count; i++)
        {
            float newDistance = Vector3.Distance(noiseMakerPos, _activateRigidbodyOnNoises[i].transform.position);
            if (newDistance <= maxDistance && newDistance <= _activateRigidbodyOnNoises[i].minimumDistanceToActivate)
            {
                _activateRigidbodyOnNoises[i].ActivateRigidbody();
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
