using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateRigidbodyOnNoise : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    void Start()
    {
        SpawnController.Instance.AddActivateRigidbodyOnNoise(this);
    }

    public void ActivateRigidbody()
    {
        rb.useGravity = true;
        rb.isKinematic = false;
    }
}
