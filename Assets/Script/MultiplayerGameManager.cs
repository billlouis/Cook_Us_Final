using Unity.Netcode;
using UnityEngine;

public class MultiplayerGameManager : NetworkBehaviour
{
    [SerializeField] private float gameDuration = 300f; // 5 minutes
    private NetworkVariable<float> timeRemaining = new NetworkVariable<float>();

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
        // Handle game end logic here
        Debug.Log("Time's up! Determine the winner.");
    }
}
