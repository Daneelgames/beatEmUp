using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawThrowArc : MonoBehaviour
{
    [SerializeField] LineRenderer lineRenderer;

    // Number of points on the line
    public int numPoints = 10;

    // distance between those points on the line
    public float timeBetweenPoints = 1f;
    [SerializeField] private float yPower = 10;

    // The physics layers that will cause the line to stop being drawn
    public LayerMask CollidableLayers;

    private void OnValidate()
    {
        if (timeBetweenPoints <= 0)
            timeBetweenPoints = 0.01f;
        
        if (numPoints <= 0)
            numPoints = 1;
    }

    void Update()
    {
        if (PartyInputManager.Instance.SelectedAllyUnits.Count <= 0)
            return;
        
        lineRenderer.positionCount = (int)numPoints;
        List<Vector3> points = new List<Vector3>();
        Vector3 startingPosition = PartyInputManager.Instance.SelectedAllyUnits[0].transform.position + Vector3.up * 1.5f;
        Vector3 startingVelocity = (GameManager.Instance.MouseWorldGroundPosition() - PartyInputManager.Instance.SelectedAllyUnits[0].transform.position) + Vector3.up * yPower;
        for (float t = 0; t < numPoints; t += timeBetweenPoints)
        {
            Vector3 newPoint = startingPosition + t * startingVelocity;
            newPoint.y = startingPosition.y + startingVelocity.y * t + Physics.gravity.y / 2f * t * t;
            points.Add(newPoint);

            if(Physics.OverlapSphere(newPoint, 1, CollidableLayers).Length > 0)
            {
                lineRenderer.positionCount = points.Count;
                break;
            }
        }

        lineRenderer.SetPositions(points.ToArray());
    }
}
