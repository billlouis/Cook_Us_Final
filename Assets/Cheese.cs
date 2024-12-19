using UnityEngine;
using Unity.Netcode;

public class Cheese : NetworkBehaviour
{
    private bool isCollected = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer || isCollected) return;

        var player = collision.collider.GetComponent<PlayerController>();
        if (player != null && !isCollected && !player.HasCheese())
        {
            // Collect the cheese
            isCollected = true;
            
            // Notify the player to collect a visual-only cheese representation
            player.CollectCheese();
            
            // Despawn cheese from the network
            GetComponent<NetworkObject>().Despawn();
        }
    }
}
