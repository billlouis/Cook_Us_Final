using UnityEngine;
using Unity.Netcode;

public class Vegetable : NetworkBehaviour
{
    public float maxHealth = 100f;
    private NetworkVariable<float> health = new NetworkVariable<float>();
    public NetworkVariable<bool> isParalyzed = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> isBeingHeld = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> isInPan = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private Collider vegetableCollider;
    private Transform followTransform;
    private CharacterController characterController;
    private PlayerController pc;

    private void Start(){
        health.Value = maxHealth;
        vegetableCollider = GetComponent<Collider>();
        pc = GetComponent<PlayerController>();
    }

    public override void OnNetworkSpawn()
    {
        // if (IsServer)
        // {
        //     health.Value = maxHealth;
        // }

        isBeingHeld.OnValueChanged += OnBeingHeldChanged;
        isParalyzed.OnValueChanged += OnParalyzedChanged;
        isInPan.OnValueChanged += OnInPanChanged;

        // vegetableCollider = GetComponent<Collider>();
        characterController = GetComponent<CharacterController>();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        isBeingHeld.OnValueChanged -= OnBeingHeldChanged;
        isParalyzed.OnValueChanged -= OnParalyzedChanged;
        isInPan.OnValueChanged -= OnInPanChanged;
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

    private void OnBeingHeldChanged(bool previousValue, bool newValue)
    {
        if (vegetableCollider != null)
        {
            vegetableCollider.enabled = !newValue;
        }

        if (characterController != null)
        {
            characterController.enabled = !newValue;
        }

        UpdateVisualState();
    }

    private void OnParalyzedChanged(bool previousValue, bool newValue)
    {
        UpdateVisualState();
    }

    private void OnInPanChanged(bool previousValue, bool newValue)
    {
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        if (TryGetComponent<Renderer>(out var renderer))
        {
            if (isParalyzed.Value)
            {
                renderer.material.color = Color.gray;
            }
            else if (isBeingHeld.Value)
            {
                renderer.material.color = Color.yellow;
            }
            else if (isInPan.Value)
            {
                renderer.material.color = Color.red;
            }
            else
            {
                renderer.material.color = Color.white;
            }
        }
    }

    public bool CanBePickedUp()
    {
        return isParalyzed.Value && !isBeingHeld.Value && !isInPan.Value;
    }

    public void TakeDamage(float damage)
    {
        if (!IsServer) return;
        
        health.Value -= damage;
        if (health.Value <= 0 && !isParalyzed.Value)
        {
            Paralyze();
        }
    }

    public void OnPickedUp(Transform pickupPoint)
    {
        if (!IsServer) return;

        isBeingHeld.Value = true;
        isInPan.Value = false;
        followTransform = pickupPoint;

        if (vegetableCollider != null)
        {
            vegetableCollider.enabled = false;
        }

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        SyncPickupClientRpc();
        Debug.Log($"{gameObject.name} picked up by server.");
    }

    public void OnDropped()
    {
        if (!IsServer) return;

        // vegetableCollider.enabled = true;
        ResetState();
        SyncDropClientRpc();
        Debug.Log($"{gameObject.name} dropped by server.");
    }

    public void OnPlacedInPan(Vector3 panPosition, Quaternion panRotation)
    {
        if (!IsServer) return;

        transform.position = panPosition;
        transform.rotation = panRotation;

        ResetState();

        SyncPanPlacementClientRpc(panPosition, panRotation);
        Debug.Log($"{gameObject.name} placed in pan and reset to default state.");
    }

    private void ResetState()
    {
        isBeingHeld.Value = false;
        isInPan.Value = false;
        isParalyzed.Value = false;
        followTransform = null;
        health.Value = maxHealth;

        if (vegetableCollider != null)
        {
            vegetableCollider.enabled = true;
        }

        if (characterController != null)
        {
            characterController.enabled = true;
        }
    }

    public void Paralyze()
    {
        if (!IsServer) return;

        isParalyzed.Value = true;
        ParalyzeClientRpc();
        Debug.Log($"{gameObject.name} is paralyzed and can now be picked up.");
    }

    [ClientRpc]
    private void ParalyzeClientRpc()
    {
        if (IsServer) return;

        if (characterController != null)
        {
            characterController.enabled = false;
        }
    }

    [ClientRpc]
    private void SyncPickupClientRpc()
    {
        if (IsServer) return;

        if (vegetableCollider != null)
        {
            vegetableCollider.enabled = false;
        }

        if (characterController != null)
        {
            characterController.enabled = false;
        }
    }

    [ClientRpc]
    private void SyncDropClientRpc()
    {
        if (IsServer) return;

        if (vegetableCollider != null)
        {
            vegetableCollider.enabled = true;
        }

        if (characterController != null)
        {
            characterController.enabled = true;
        }

        transform.rotation = Quaternion.identity;
    }

    [ClientRpc]
    private void SyncPanPlacementClientRpc(Vector3 position, Quaternion rotation)
    {
        if (IsServer) return;

        transform.position = position;
        transform.rotation = rotation;

        if (vegetableCollider != null)
        {
            vegetableCollider.enabled = true;
        }

        if (characterController != null)
        {
            characterController.enabled = true;
        }

        UpdateVisualState();
    }

    private void Update()
    {
        if (isBeingHeld.Value && followTransform != null)
        {
            if (IsServer)
            {
                transform.position = followTransform.position;
                transform.rotation = followTransform.rotation;
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