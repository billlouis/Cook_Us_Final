using System.Collections;
using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    public Animator animator; // Reference to the Animator for the attack animation
    public Collider fryingPanCollider; // Collider for detecting hits
    public int damage = 10; // Amount of damage dealt

    void Start()
    {
        // Disable collider by default so it only triggers during the attack
        fryingPanCollider.enabled = false;
    }

    public void Attack()
    {
        animator.SetBool("isHitting", true); // Set attacking state to true
        StartCoroutine(EnableCollider());
    }

    private IEnumerator EnableCollider()
    {
        fryingPanCollider.enabled = true;

        yield return new WaitForSeconds(1.15f); // Enable the collider during the hit moment
        fryingPanCollider.enabled = false;

        yield return new WaitForSeconds(1.0f); // Allow the attack animation to complete

        animator.SetBool("isHitting", false); // Reset attacking state to return to walking/idle
    }

}
