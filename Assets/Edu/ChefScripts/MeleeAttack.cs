using System.Collections;
using UnityEngine;
using Unity.Netcode;
public class MeleeAttack : NetworkBehaviour
{
    public Animator animator; // Reference to the Animator for the attack animation
    public Collider fryingPanCollider; // Collider for detecting hits
    public int damage = 10; // Amount of damage dealt
    private NetworkVariable<bool> isHitting = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner // Ensure the server updates this
    );
    public override void OnNetworkSpawn()
    {
        // Subscribe to walking state changes for all clients
        isHitting.OnValueChanged += OnHittingStateChanged;
    }
    private void OnHittingStateChanged(bool previous, bool current)
    {
        animator.SetBool("isHitting", current);
    }
    void Start()
    {
        // Disable collider by default so it only triggers during the attack
        fryingPanCollider.enabled = false;
    }

    public void Attack()
    {
        if(!IsOwner) return;
        isHitting.Value = true;
    
        StartCoroutine(EnableCollider());
    }

    private IEnumerator EnableCollider()
    {
        fryingPanCollider.enabled = true;

        yield return new WaitForSeconds(1.15f); // Enable the collider during the hit moment
        fryingPanCollider.enabled = false;

        yield return new WaitForSeconds(1.0f); // Allow the attack animation to complete

        isHitting.Value = false;
    }

}
