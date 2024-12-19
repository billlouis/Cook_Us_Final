using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using UnityEngine.SceneManagement;


public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button serverBtn;
    [SerializeField] private Button clientBtn;
    [SerializeField] private Button hostBtn;
    [SerializeField] private string GameplaySceneName;
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene(GameplaySceneName, LoadSceneMode.Single);
    }
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
        NetworkManager.Singleton.SceneManager.LoadScene(GameplaySceneName, LoadSceneMode.Single);
    }
}
