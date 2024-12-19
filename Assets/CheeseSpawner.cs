using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class CheeseSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject cheesePrefab; // Prefab for the cheese object
    [SerializeField] private Transform[] platePositions; // Array of predetermined plate positions
    [SerializeField] private int cheeseCount = 5; // Number of cheese to spawn

    private List<int> usedIndices = new List<int>();

    public override void OnNetworkSpawn()
    {
        if (IsServer) // Only the server can spawn objects in Netcode
        {
            SpawnCheese();
        }
    }

    private void SpawnCheese()
    {
        usedIndices.Clear(); // Clear previous indices in case of respawn

        for (int i = 0; i < cheeseCount; i++)
        {
            int randomIndex;
            
            // Ensure unique plate positions are chosen
            do
            {
                randomIndex = Random.Range(0, platePositions.Length);
            } while (usedIndices.Contains(randomIndex));

            usedIndices.Add(randomIndex);

            Vector3 spawnPosition = platePositions[randomIndex].position;
            
            // Instantiate and spawn cheese as a network object
            GameObject cheeseInstance = Instantiate(cheesePrefab, spawnPosition, Quaternion.identity);
            cheeseInstance.GetComponent<NetworkObject>().Spawn(); // Spawn it across the network
        }
    }
}
