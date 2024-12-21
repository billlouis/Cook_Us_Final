using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Linq;

public class CheeseGameManager : NetworkBehaviour
{
    public static CheeseGameManager Instance { get; private set; }

    public NetworkVariable<int> cheeseCollectedCount = new NetworkVariable<int>(0);
    [SerializeField] public int requiredCheese = 7;
    [SerializeField] private Loader.Scene nextScene = Loader.Scene.IngredientsWinScene;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddCheeseCountServerRpc()
    {
        cheeseCollectedCount.Value++;

        // Check if the required cheese has been collected
        if (cheeseCollectedCount.Value >= requiredCheese)
        {
            Debug.Log("You Win");
            cheeseCollectedCount.Value = 0;
            CleanupAndLoadWinSceneClientRpc();
        }
    }

    [ClientRpc]
    private void CleanupAndLoadWinSceneClientRpc()
    {
        GameObject networkSceneUI = GameObject.Find("NetworkSceneUI");
        if (networkSceneUI != null)
        {
            Destroy(networkSceneUI);
        }
        // Create a list of objects to despawn
        var objectsToDespawn = NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values.ToList();
        
        // Despawn each object
        foreach (NetworkObject networkObject in objectsToDespawn)
        {
            networkObject.Despawn();
        }

        // Load the win scene
        Loader.LoadNetwork(nextScene);
    }

    public int GetCurrentCheeseCount()
    {
        return cheeseCollectedCount.Value;
    }
}