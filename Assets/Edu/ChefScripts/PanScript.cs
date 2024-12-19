using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanScript : MonoBehaviour
{
    // Start is called before the first frame update
    public int damage = 10;

    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Damageable"))
        {
            var damageable = other.GetComponent<Damageable>();
            if (damageable != null)
            {
                // Calculate knockback direction
                Vector3 knockbackDirection = (other.transform.position - transform.position).normalized;
                float knockbackForce = 5f; // Adjust as needed

                // Apply damage and knockback
                damageable.TakeDamage(damage, knockbackDirection, knockbackForce);
            }
        }
    }
}
