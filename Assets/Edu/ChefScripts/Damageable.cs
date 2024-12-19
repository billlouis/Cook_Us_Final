using UnityEngine;

public class Damageable : MonoBehaviour
{
    public int health = 20;
    private Rigidbody rb;
    public bool isParalyzed = false; // Track if the object is paralyzed

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void TakeDamage(int damage, Vector3 knockbackDirection, float knockbackForce)
    {
        if (isParalyzed) return; // If already paralyzed, ignore further damage

        health -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage! Health is now {health}.");

        // Apply knockback force
        if (rb != null)
        {
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
        }

        // Check if health is depleted
        if (health <= 0)
        {
            Paralyze();
        }
    }

    private void Paralyze()
    {
        isParalyzed = true;
        rb.isKinematic = true; // Optional: disable physics for easy pickup
        Debug.Log($"{gameObject.name} is now paralyzed and can be picked up.");
    }

    public void PickUp(Transform pickupPosition)
    {
        if (isParalyzed)
        {
            // Attach the object to the PickupPosition
            transform.SetParent(pickupPosition);
            transform.localPosition = Vector3.zero; // Reset position to match the PickupPosition
            transform.localRotation = Quaternion.identity; // Optional: Reset rotation if needed
            Debug.Log($"{gameObject.name} has been picked up and moved to the screen.");

            // Additional adjustments if needed, like disabling physics
            if (rb != null)
            {
                rb.isKinematic = true;
            }
        }
    }
}
