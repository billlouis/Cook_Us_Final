using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class CheeseGameManager : NetworkBehaviour
{
    public static CheeseGameManager Instance { get; private set; }

    public NetworkVariable<int> cheeseCollectedCount = new NetworkVariable<int>(0);
    [SerializeField] public int requiredCheese = 7;
   // public string nextSceneName = "NextScene";

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
            // Load the next scene for all clients
            //NetworkManager.Singleton.SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
        }
    }

    public int GetCurrentCheeseCount()
    {
        return cheeseCollectedCount.Value;
    }
}
