using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageOnContact : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float minVelocityToDamage = 0.5f;
    [SerializeField] private int dmg = 1000;
    private List<GameObject> damagedBodyPartsGameObjects = new List<GameObject>();

    IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            damagedBodyPartsGameObjects.Clear();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (rb.velocity.magnitude > minVelocityToDamage)
        {
            if (other.gameObject.layer != 7)
                return;
            
            if (damagedBodyPartsGameObjects.Contains(other.gameObject))
            {
                return;
            }
            damagedBodyPartsGameObjects.Add(other.gameObject);
            var newPartToDamage = other.gameObject.GetComponent<BodyPart>();

            if (newPartToDamage)
            {
                newPartToDamage.HC.Damage(dmg, null, HealthController.DamageType.Explosive);
            }
        }
    }
}
