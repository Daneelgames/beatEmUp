using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Observable : MonoBehaviour
{
    [SerializeField] private HealthController _healthController;
    public HealthController HealthController
    {
        get => _healthController;
        set => _healthController = value;
    }
    [SerializeField] private Interactable interactable;
    public Interactable Interactable
    {
        get => interactable;
        set => interactable = value;
    }

    void Start()
    {
        GameManager.Instance.ObservablesInRunTime.Add(this);
    }

    [ContextMenu("FindLinks")]
    public void FindLinks()
    {
        HealthController = gameObject.GetComponent<HealthController>();
        Interactable = gameObject.GetComponent<Interactable>();
    }
}
