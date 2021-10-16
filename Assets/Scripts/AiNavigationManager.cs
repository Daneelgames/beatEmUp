using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiNavigationManager : MonoBehaviour
{
    [SerializeField] private List<Transform> pointsOfInterest;
    public static AiNavigationManager instance;

    private void Awake()
    {
        instance = this;
    }

    public List<Transform> PointsOfInterest => pointsOfInterest;
}
