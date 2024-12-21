using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PanScript : NetworkBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private float knockbackForce = 1f;

    private void OnTriggerEnter(Collider other)
    {
         // Ensure only the server handles interactions

        if (other.CompareTag("Damageable"))
        {
            var damageable = other.GetComponent<PlayerController>();
            if (damageable != null)
            {
                // Calculate knockback direction
                Vector3 knockbackDirection = (other.transform.position - transform.position).normalized;

                // Apply damage and knockback using the ServerRpc
                damageable.ApplyKnockbackServerRpc(damage, knockbackDirection, knockbackForce);
            }
        }
    }


    
}

