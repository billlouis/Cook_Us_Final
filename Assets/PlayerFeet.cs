using UnityEngine;
using Unity.Netcode;

public class PlayerPickupZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if the collided object is tagged as "Cheese"
        if (other.CompareTag("Cheese"))
        {
            var cheese = other.GetComponent<Cheese>();
            
            var player = GetComponentInParent<PlayerController>();
            if (player != null && !player.HasCheese() && !cheese.isCollected)
            {
                Debug.Log("Picked up Cheese In Trigger");
                player.CollectCheese();
                cheese.CollectCheese();
            }
        }
    }
}

