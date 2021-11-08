using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AiNavigationManager : MonoBehaviour
{
    [SerializeField] private float maxDistanceForSearchingNewPoint = 50;
    
    [SerializeField] private List<HealthController> centerMobs = new List<HealthController>();
    [SerializeField] private List<Transform> pointsOfInterest;
    [SerializeField] private List<Transform> pointsOfInterestDesert;
    [SerializeField] private List<Transform> pointsOfInterestAllies;
    public static AiNavigationManager instance;

    private void Awake()
    {
        instance = this;
    }

    public List<Transform> PointsOfInterest => pointsOfInterest;
    public List<Transform> PointsOfInterestDesert => pointsOfInterestDesert;
    public List<Transform> PointsOfInterestAllies => pointsOfInterestAllies;

    public Vector3 GetPointOfInterestForUnit(HealthController hc)
    {
        List<Transform> tempPOIs;
        
        if (centerMobs.Contains(hc))
        {
            tempPOIs = new List<Transform>(PointsOfInterest);
             
        }
        else if (hc.AiInput.ally)
            tempPOIs = new List<Transform>(PointsOfInterestAllies);
        else
            tempPOIs = new List<Transform>(PointsOfInterestDesert);

        Vector3 resultPos = tempPOIs[Random.Range(0, tempPOIs.Count)].position;
        
        for (int i = tempPOIs.Count - 1; i >= 0; i--)
        {
            if (Vector3.Distance(tempPOIs[i].position, hc.transform.position) > maxDistanceForSearchingNewPoint)
            {
                if (tempPOIs.Count < 2)
                    return tempPOIs[0].position + Random.insideUnitSphere * Random.Range(1, 5);
                
                tempPOIs.RemoveAt(i);
            }
        }
        
        if (tempPOIs.Count > 0)
            resultPos = tempPOIs[Random.Range(0, tempPOIs.Count)].position + Random.insideUnitSphere * Random.Range(1, 5);

        return resultPos;
    }
}
