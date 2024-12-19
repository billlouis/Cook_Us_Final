using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using UnityEngine.SceneManagement;


public class NetworkGUIManager : MonoBehaviour
{
    public static NetworkGUIManager Instance;
    [SerializeField] private Text progressText;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Prevent duplicate instances
        }
      
    }
    public void UpdateProgressText(string texts){
        progressText.text = texts;
    }
}
