using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyPartsManager : MonoBehaviour
{
    [SerializeField] private BodyPart hipsBone;
    public BodyPart HipsBone => hipsBone;
    public Transform victimTargetTransform;

    public List<BodyPart> bodyParts;
    [SerializeField] private List<BodyPart> removableParts;

    public List<BodyPart> RemovableParts => removableParts;
    

    [SerializeField] private GameObject bloodSfx;

    [SerializeField] private GameObject victimTestGameObject;
    void Start()
    {
        if (bodyParts.Count == 0)
            InitBodyParts();

        if (victimTestGameObject != null)
            victimTestGameObject.name += " Test";
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
        for (var index = bodyParts.Count - 1; index >= 0; index--)
        {
            var bodyPart = bodyParts[index];
            if (bodyPart == null)
            {
                bodyParts.RemoveAt(index);
                continue;
            }
            bodyPart.Collider.isTrigger = false;
        }
    }

    public IEnumerator RemovePart(bool attackerGetsPArt)
    {
        if (removableParts.Count <= 0)
            yield break;

        if (!attackerGetsPArt)
        {
            Instantiate(bloodSfx, transform.position, transform.rotation);
            var partToRemove = removableParts[Random.Range(0, removableParts.Count)];
            Vector3 partNewWorldPos = partToRemove.transform.position;
            Quaternion partNewWorldRot = partToRemove.transform.rotation;
            Vector3 newScale = partToRemove.GlobalScaleAfterReparenting;

            GameObject partToDublicate = partToRemove.gameObject;
            removableParts.Remove(partToRemove);
            Destroy(partToRemove);
            
            GameObject newPart = Instantiate(partToDublicate, partToDublicate.transform.position, partToDublicate.transform.rotation);
            
            Destroy(partToDublicate);
            
            yield return new WaitForSeconds(0.1f);
            
            newPart.transform.position = partNewWorldPos;
            newPart.transform.rotation = partNewWorldRot;
            newPart.transform.localScale = newScale;
            
            var newRb = newPart.gameObject.AddComponent<Rigidbody>();
            newRb.isKinematic = false;
            newRb.useGravity = true;
            newRb.AddExplosionForce(100, transform.position - Vector3.up * 2 + Random.onUnitSphere * 1, 10);

            StartCoroutine(GameManager.Instance.FreezeRigidbodyOverTime(3, newRb, 3, true));
        }
    }

}
