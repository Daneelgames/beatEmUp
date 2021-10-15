using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyPartsManager : MonoBehaviour
{
    public List<BodyPart> bodyParts;

    void Start()
    {
        if (bodyParts.Count == 0)
            InitBodyParts();
    }

    [ContextMenu("InitBodyParts")]
    public void InitBodyParts()
    {
        var newParts = GetComponentsInChildren<BodyPart>();
        bodyParts = new List<BodyPart>(newParts);
        AttackManager attackManager = GetComponent<AttackManager>();
        HealthController hc = GetComponent<HealthController>();
        for (int i = 0; i < bodyParts.Count; i++)
        {
            bodyParts[i].SetOwnBodyParts(bodyParts, attackManager, hc);
        }
    }

    public void SetAllPartsColliders()
    {
        for (var index = 0; index < bodyParts.Count; index++)
        {
            var bodyPart = bodyParts[index];
            bodyPart.Collider.isTrigger = false;
        }
    }
}
