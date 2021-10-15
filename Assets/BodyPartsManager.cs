using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyPartsManager : MonoBehaviour
{
    public List<BodyPart> bodyParts;
    [SerializeField] private List<BodyPart> removableParts;

    public List<BodyPart> RemovableParts => removableParts;

    [SerializeField] private GameObject bloodSfx;
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

    public void RemovePart(bool attackerGetsPArt)
    {
        if (removableParts.Count <= 0)
            return;

        if (!attackerGetsPArt)
        {
            Instantiate(bloodSfx, transform.position, transform.rotation);
            var partToRemove = removableParts[Random.Range(0, removableParts.Count)];
            Vector3 partNewWorldPos = partToRemove.transform.position;
            Quaternion partNewWorldRot = partToRemove.transform.rotation;
            removableParts.Remove(partToRemove);
            partToRemove.transform.SetParent(null,true);
            partToRemove.transform.position = partNewWorldPos;
            partToRemove.transform.rotation = partNewWorldRot;
            partToRemove.transform.localScale = partToRemove.GlobalScaleAfterReparenting;
            
            var newRb = partToRemove.gameObject.AddComponent<Rigidbody>();
            newRb.isKinematic = false;
            newRb.useGravity = true;
            newRb.AddExplosionForce(100, transform.position - Vector3.up * 2 + Random.onUnitSphere * 1, 10);
            print(partNewWorldPos + "; partToRemove.transform.position " + partToRemove.transform.position );
        }
    }
}
