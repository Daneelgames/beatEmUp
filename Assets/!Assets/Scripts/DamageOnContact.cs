using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageOnContact : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float minVelocityToDamage = 0.5f;
    [SerializeField] private int dmg = 1000;
    private List<GameObject> damagedBodyPartsGameObjects = new List<GameObject>();


    [Tooltip("If -1 than time doesnt matter")]
    [SerializeField] private float dangerousTime = -1f;

    private bool dangerous = true;
    IEnumerator Start()
    {
        if (dangerousTime > -1)
        {
            dangerous = true;
            yield return new WaitForSeconds(dangerousTime);
            dangerous = false;
        }
        else
        {
            dangerous = true;
        }
        while (true)
        {
            yield return new WaitForSeconds(1f);
            damagedBodyPartsGameObjects.Clear();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!dangerous) return;
        if (other.gameObject.layer != 7) return;
        if (rb && rb.velocity.magnitude < minVelocityToDamage) return;
        if (damagedBodyPartsGameObjects.Contains(other.gameObject)) return;
        
        damagedBodyPartsGameObjects.Add(other.gameObject);
        var newPartToDamage = other.gameObject.GetComponent<BodyPart>();

        if (newPartToDamage)
        {
            newPartToDamage.HC.Damage(dmg, null, HealthController.DamageType.Explosive, true);
        }
    }
}
