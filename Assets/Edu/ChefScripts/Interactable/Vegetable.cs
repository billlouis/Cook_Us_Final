using UnityEngine;
using Unity.Netcode;

public class Vegetable : NetworkBehaviour
{
    public float maxHealth = 100f;
    private float health;
    public NetworkVariable<bool> isParalyzed = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> isBeingHeld = new NetworkVariable<bool>(false);
    private Collider vegetableCollider;
    private Transform followTransform; // The point the vegetable should follow when picked up.
    private PlayerController pc;
    private void Start()
    {
        health = maxHealth;
        vegetableCollider = GetComponent<Collider>();
        pc = GetComponent<PlayerController>();
    }

    public bool CanBePickedUp()
    {
        return isParalyzed.Value && !isBeingHeld.Value;
    }

    public void OnPickedUp(Transform pickupPoint)
    {
        if (!IsServer) return;

        isBeingHeld.Value = true;
        followTransform = pickupPoint;

        // Disable collider for proper pickup handling
        if (vegetableCollider != null)
        {
            vegetableCollider.enabled = false; // Disable collider to prevent collision with the player
        }

        Debug.Log($"{gameObject.name} picked up by server.");
    }
    public void Revive()
    {
        if (!IsServer) return;

        // Reset the vegetable's state
        isParalyzed.Value = false;
        pc.currentHealth.Value = 10;

        // Reset appearance
        if (TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material.color = Color.white; // Or whatever your default color is
        }
        if (vegetableCollider != null)
        {
            vegetableCollider.enabled = true;
        }

        // Notify clients about the revival
        ReviveClientRpc();
    }
    [ClientRpc]
    private void ReviveClientRpc()
    {
        if (!IsServer)
        {
            if (vegetableCollider != null)
            {
                vegetableCollider.enabled = true;
            }
            // Update any client-side visual effects or states
            if (TryGetComponent<Renderer>(out var renderer))
            {
                renderer.material.color = Color.white; // Or whatever your default color is
            }
        }
    }
    public void OnDropped()
    {
        if (!IsServer) return;

        isBeingHeld.Value = false;
        followTransform = null;

        // Re-enable collider
        if (vegetableCollider != null)
        {
            vegetableCollider.enabled = true;
        }

        Debug.Log($"{gameObject.name} dropped by server.");
    }

    public void Paralyze()
    {
        if (!IsServer) return;

        isParalyzed.Value = true;

        // Change appearance to indicate paralysis
        if (TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material.color = Color.gray;
        }

        Debug.Log($"{gameObject.name} is paralyzed and can now be picked up.");
    }

    [ClientRpc]
    private void SyncPickupClientRpc()
    {
        if (IsServer) return;

        if (vegetableCollider != null)
        {
            vegetableCollider.enabled = false;
        }
    }

    [ClientRpc]
    private void SyncDropClientRpc()
    {
        if (IsServer) return;

        // Reset held state and detach from player
        isBeingHeld.Value = false;
        followTransform = null;

        // Re-enable collider
        if (vegetableCollider != null)
        {
            vegetableCollider.enabled = true;
        }

        // Ensure the vegetable is reset visually and mechanically
        transform.rotation = Quaternion.identity;

        if (TryGetComponent<CharacterController>(out var characterController))
        {
            characterController.enabled = false;
        }

        Debug.Log($"{gameObject.name} dropped on client and reset to default state.");
    }

    private void Update()
    {
        if (isBeingHeld.Value && followTransform != null)
        {
            // Server updates position
            if (IsServer)
            {
                transform.position = followTransform.position;
                transform.rotation = followTransform.rotation;

                // Sync with clients
                UpdatePositionClientRpc(transform.position, transform.rotation);
            }
        }
    }

    [ClientRpc]
    private void UpdatePositionClientRpc(Vector3 position, Quaternion rotation)
    {
        if (!IsServer)
        {
            transform.position = position;
            transform.rotation = rotation;
        }
    }

}