using System.Collections;
using UnityEngine;
using Unity.Netcode;
public class MeleeAttack : NetworkBehaviour
{
    public Animator animator;
    public Collider fryingPanCollider;
    public int damage = 10;
    private bool isAttacking = false; 

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
        if (!IsOwner) return;
        if (isAttacking) return; // Prevent attack spam
        
        isAttacking = true;
        isHitting.Value = true;
        StartCoroutine(EnableCollider());
    }

    private IEnumerator EnableCollider()
    {
        fryingPanCollider.enabled = true;

        yield return new WaitForSeconds(1.967f);
        fryingPanCollider.enabled = false;

        // yield return new WaitForSeconds(1.0f);

        isHitting.Value = false;
        isAttacking = false; // Reset attack state when animation is complete
    }

}
