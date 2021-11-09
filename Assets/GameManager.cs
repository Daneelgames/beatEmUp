using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private List<HealthController> units = new List<HealthController>();
    public Camera mainCamera;
    public List<HealthController> Units => units;
    public int navMeshSampleIterations = 10;

    public LayerMask groundLayerMask;
    [SerializeField] private int drawHealthbarsDistance = 30;
    
    public bool simpleEnemiesAllies = true;

    [SerializeField] private List<Observable> _observablesInRunTime;
    public List<Observable> ObservablesInRunTime
    {
        get => _observablesInRunTime;
        set => _observablesInRunTime = value;
    }

    public int DrawHealthbarsDistance
    {
        get => drawHealthbarsDistance;
        set => drawHealthbarsDistance = value;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetButton("FastMode") && Time.timeScale < 3)
            Time.timeScale = 3;
        else if (Input.GetButton("SlowMode") && Time.timeScale > 0.5f)
            Time.timeScale = 0.1f;
        else if (Time.timeScale > 1.1f || Time.timeScale < 0.9f)
            Time.timeScale = 1;
        
        if (Input.GetButtonDown("Cancel"))
        {
            TogglePause();
        }
        
        if (Input.GetKey("g") && Input.GetKey("z") && Input.GetKeyDown("r"))
        {
            SceneManager.LoadScene(0);
        }
    }

    void TogglePause()
    {
        if (Time.timeScale < 0.5f)
            Time.timeScale = 1f;
        else
            Time.timeScale = 0f;
    }

    public void AddUnit(HealthController hc)
    {
        if (Units.Contains(hc) == false)
            Units.Add(hc);

        CharacterGenerator.Instance.GenerateUnit(hc);
    }
    
    public void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
   
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively( child.gameObject, newLayer );
        }
    }

    public IEnumerator FreezeRigidbodyOverTime(float delayBeforeStart, Rigidbody newRb, float time, bool destroy)
    {
        yield return new WaitForSeconds(delayBeforeStart);
        float t = 0;
        
        if (newRb == null)
            yield break;
        
        Vector3 startVel = newRb.velocity;
        Vector3 startAngularVel = newRb.angularVelocity;
        var grav = Physics.gravity;
        while (t < time)
        {
            newRb.velocity = Vector3.Lerp(startVel, Vector3.zero + grav, t/time);
            newRb.angularVelocity = Vector3.Lerp(startAngularVel, Vector3.zero, t/time);
            t += Time.deltaTime;
            yield return null;
        }
        newRb.velocity = Vector3.zero;
        newRb.angularVelocity = Vector3.zero;
        SetLayerRecursively(newRb.gameObject, 6);
        if (destroy)
            Destroy(newRb);
    }

    public HealthController GetClosestUnit(HealthController hcAttacker, bool onlyEnemies, float maxDistance)
    {
        float distance = maxDistance;
        float newDistance = 0;
        
        HealthController closestUnit = null;
        
        for (int i = 0; i < Units.Count; i++)
        {
            if (Units[i] == hcAttacker || Units[i].Health <= 0)
                continue;

            if (hcAttacker.Enemies.Contains(Units[i]) == false && onlyEnemies)
                continue;

            newDistance = Vector3.Distance(hcAttacker.transform.position, Units[i].transform.position);
            if (newDistance < distance)
            {
                distance = newDistance;
                closestUnit = Units[i];
            }
        }

        return closestUnit;
    }

    public Vector3 MouseWorldGroundPosition()
    {
        Vector3 mousePos = Vector3.zero;
        
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayerMask)) {
            mousePos = hit.point;
        }
        return mousePos;
    }

    public Vector3 GetClosestNavmeshPoint(Vector3 newPos)
    {
        Vector3 closestPosition = newPos;
        NavMeshHit hit;
        for (int i = 0; i < navMeshSampleIterations; i++)
        {
            if (NavMesh.SamplePosition(newPos, out hit, 5 + 5 * i, NavMesh.AllAreas))
            {
                closestPosition = hit.position;
                break;
            }
        }
        
        return closestPosition;
    }
    
}
