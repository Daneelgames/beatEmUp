using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private List<HealthController> units = new List<HealthController>();
    public List<HealthController> Units => units;

    private void Awake()
    {
        Instance = this;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void AddUnit(HealthController hc)
    {
        if (units.Contains(hc) == false)
            units.Add(hc);
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
        Vector3 startVel = newRb.velocity;
        Vector3 startAngularVel = newRb.angularVelocity;
        while (t < time)
        {
            newRb.velocity = Vector3.Lerp(startVel, Vector3.zero, t/time);
            newRb.angularVelocity = Vector3.Lerp(startAngularVel, Vector3.zero, t/time);
            t += Time.deltaTime;
            yield return null;
        }
        newRb.velocity = Vector3.zero;
        SetLayerRecursively(newRb.gameObject, 6);
        if (destroy)
            Destroy(newRb);
    }
}
