using UnityEngine;
using Unity.Netcode;

public class DropZone : NetworkBehaviour
{
    [SerializeField] private int requiredCheese = 5;
    private NetworkVariable<int> cheeseCollected = new NetworkVariable<int>(0);

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        var player = other.GetComponent<PlayerController>();
        if (player != null && player.HasCheese())
        {
            // Player drops cheese in the zone
            player.DropCheese();
            CheeseGameManager.Instance.AddCheeseCountServerRpc();
            // Update collected cheese count on the server
            cheeseCollected.Value++;

            // Notify all clients of the new progress
            UpdateProgressTextClientRpc(cheeseCollected.Value, requiredCheese);
        }
    }

    [ClientRpc]
    private void UpdateProgressTextClientRpc(int current, int total)
    {
        NetworkGUIManager.Instance.UpdateProgressText($"{current}/{total} Cheese Collected");
    }
}
