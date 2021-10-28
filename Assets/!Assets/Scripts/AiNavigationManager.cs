using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AiNavigationManager : MonoBehaviour
{
    [SerializeField] private List<HealthController> centerMobs = new List<HealthController>();
    [SerializeField] private List<Transform> pointsOfInterest;
    [SerializeField] private List<Transform> pointsOfInterestDesert;
    public static AiNavigationManager instance;

    private void Awake()
    {
        instance = this;
    }

    public List<Transform> PointsOfInterest => pointsOfInterest;
    public List<Transform> PointsOfInterestDesert => pointsOfInterestDesert;

    public Vector3 GetPointOfInterestForUnit(HealthController hc)
    {

        if (centerMobs.Contains(hc))
            return PointsOfInterest[Random.Range(0, PointsOfInterest.Count)].position + Random.insideUnitSphere * Random.Range(1, 5);
        
        return PointsOfInterestDesert[Random.Range(0, PointsOfInterestDesert.Count)].position + Random.insideUnitSphere * Random.Range(1, 5);
    }
}
