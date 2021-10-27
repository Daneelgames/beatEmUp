using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public bool drawFOV = false;
    
    [Header("Links")]
    [SerializeField] private HealthController hc;

    [SerializeField] private Transform eyesTransfom;

    [Header("Stats")] 
    [SerializeField] private int minVisibleBonesToSeeUnit = 4;
    public int MinVisibleBonesToSeeUnit => minVisibleBonesToSeeUnit;

    [SerializeField] private float resetVisibleUnitsCooldown = 10f;
    [SerializeField] private float updateDelay = 0.25f;
    [SerializeField] private float viewRadius;
    [SerializeField] private float sixSenseDistance = 2;
    [SerializeField] private float meshResolution;
    [SerializeField] private int edgeResolveIterations = 3;
    public float ViewRadius => viewRadius;

    [SerializeField] [Range(0,360)] private float viewAngle;
    public float ViewAngle => viewAngle;

    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask raycastUnitsAndObstaclesMask;

    private List<Transform> visibleTargets = new List<Transform>();
    public List<Transform> VisibleTargets => visibleTargets;

    [SerializeField] MeshFilter viewMeshFilter;
    [SerializeField] private Mesh viewMesh;

    [SerializeField] private float edgeDistanceThreshold;
    private bool alive = true;
    float resetVisibleUnitsCooldownCurrent = 0;
    private void Start()
    {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;
        StartCoroutine(FindTargetsWithDelay());
    }

    public void Death()
    {
        StopAllCoroutines();
        alive = false;
        viewMeshFilter.gameObject.SetActive(false);
    }

    IEnumerator FindTargetsWithDelay()
    {
        yield return new WaitForSeconds(0.1f * GameManager.Instance.Units.IndexOf(hc));
        
        resetVisibleUnitsCooldownCurrent = 0;
        while (true)
        {
            yield return new WaitForSeconds(updateDelay);
            resetVisibleUnitsCooldownCurrent += updateDelay;
            if (resetVisibleUnitsCooldownCurrent >= resetVisibleUnitsCooldown)
            {
                bool canReset = true;
                for (int i = 0; i < hc.Enemies.Count; i++)
                {
                    if (hc.VisibleHCs.Contains(hc.Enemies[i]))
                    {
                        canReset = false;
                        break;
                    }
                }

                if (canReset)
                {
                    hc.ResetVisibleUnits();
                }
                
                resetVisibleUnitsCooldownCurrent = 0;
            }
            StartCoroutine(FindVisibleTargets());
        }
    }

    public void DelayCooldown(float amount)
    {
        resetVisibleUnitsCooldownCurrent -= amount;
        
        if (resetVisibleUnitsCooldownCurrent <= 0)
            resetVisibleUnitsCooldownCurrent = 0;
    }

    private void LateUpdate()
    {
        if (!alive || !drawFOV)
            return;
        
        DrawFieldOfView();
    }

    IEnumerator FindVisibleTargets()
    {
        visibleTargets.Clear();
        
        Collider[] targetsInViewRadius = Physics.OverlapSphere(eyesTransfom.position, viewRadius, targetMask);

        int t = 0;
        
        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            
            if (target == transform)
            {
                continue;
            }
            
            //print("1");
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            float dstToTarget = Vector3.Distance(eyesTransfom.position, target.position);

            bool canRaycast = dstToTarget <= sixSenseDistance || Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2;

            // if in viewport
            if (canRaycast)
            {           
                RaycastHit hit;
                if (Physics.Raycast(eyesTransfom.position,  (target.position - eyesTransfom.position).normalized, out hit, dstToTarget, raycastUnitsAndObstaclesMask))
                {
                    if (hit.collider.transform == target)
                    {
                        visibleTargets.Add(target);
                    }
                }

                t++;
                if (t > 5)
                {
                    t = 0;
                    yield return null;   
                }
            }
        }

        targetsInViewRadius = null;
        StartCoroutine(hc.UpdateVisibleTargets(visibleTargets));
    }

    void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;

        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();
        
        for (int i = 0; i < stepCount; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);
            if (i > 0)
            {
                bool edgeDistanceThresholdExceeded =
                    Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDistanceThreshold;
                
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDistanceThresholdExceeded))
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointB);
                    }
                }
            }
            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];
        
        vertices[0]= Vector3.zero;

        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);
            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;   
            }
        }
        
        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);

            
            bool edgeDistanceThresholdExceeded =
                Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDistanceThreshold;
            
            if (newViewCast.hit == minViewCast.hit && !edgeDistanceThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }
    
    ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, dir, out hit, viewRadius, raycastUnitsAndObstaclesMask))
        {
            return new ViewCastInfo(true,hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
        }
    }
    
    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            // convert to global
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle)
        {
            hit = _hit;
            point = _point;
            dst = _dst;
            angle = _angle;
        }
    }

    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }
}