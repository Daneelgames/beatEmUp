using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneShotSound : MonoBehaviour
{
    [SerializeField] private float timeToDestroy = 1;
    [SerializeField] private AudioSource au;
    void Start()
    {
        au.pitch = Random.Range(0.75f, 1.1f);
        au.Play();
        
        Destroy(gameObject, timeToDestroy);
    }
}