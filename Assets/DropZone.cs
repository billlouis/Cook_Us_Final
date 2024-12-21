using UnityEngine;
using Unity.Netcode;

public class DropZone : NetworkBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        var player = other.GetComponent<PlayerController>();
        Debug.Log("PCFound");
        if (player != null && player.HasCheese())
        {
            Debug.Log("hascheese");
            // Player drops cheese in the zone
            player.DropCheese();
            CheeseGameManager.Instance.AddCheeseCountServerRpc();

            // Notify all clients of the new progress
            UpdateProgressTextClientRpc(CheeseGameManager.Instance.cheeseCollectedCount.Value, CheeseGameManager.Instance.requiredCheese);
        }
    }

    [ClientRpc]
    private void UpdateProgressTextClientRpc(int current, int total)
    {
         NetworkGUIManager.Instance.UpdateProgressText($"{current}/{total} Cheese Collected");
    }
}
