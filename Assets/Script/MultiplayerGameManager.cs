using Unity.Netcode;
using UnityEngine;
using System.Linq;

public class MultiplayerGameManager : NetworkBehaviour
{
    [SerializeField] private float gameDuration = 300f;
    private NetworkVariable<float> timeRemaining = new NetworkVariable<float>();
    [SerializeField] private Loader.Scene nextScene = Loader.Scene.GameScene;
    
    [SerializeField] public TMPro.TextMeshProUGUI timerText;

    private void Start()
    {
        if (IsServer)
        {
            timeRemaining.Value = gameDuration;
        }
    }

    private void Update()
    {
        if (IsServer && timeRemaining.Value > 0)
        {
            timeRemaining.Value -= Time.deltaTime;

            if (timeRemaining.Value <= 0)
            {
                timeRemaining.Value = 0;
                EndGame();
            }
        }

        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(timeRemaining.Value / 60);
        int seconds = Mathf.FloorToInt(timeRemaining.Value % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void EndGame()
    {
        if (IsServer)
        {
            Debug.Log("Time's up! Cleaning up and loading results...");
            CleanupAndLoadResultsClientRpc();
        }
    }

    [ClientRpc]
    private void CleanupAndLoadResultsClientRpc()
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

        // Load the UI scene
        Loader.LoadNetwork(nextScene);
    }
}