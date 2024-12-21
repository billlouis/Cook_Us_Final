using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class CharacterColorSelectSingleUI : MonoBehaviour
{


    [SerializeField] private int characterId;
    [SerializeField] private Image image;
    //[SerializeField] private GameObject selectedGameObject;


    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => {
            GameMultiplayer.Instance.ChangePlayerCharacter(characterId);
        });
    }

    private void Start()
    {
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged += GameMultiplayer_OnPlayerDataNetworkListChanged;
        image.sprite = GameMultiplayer.Instance.GetPlayerCharacter(characterId);
        UpdateIsSelected();
    }

    private void GameMultiplayer_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        UpdateIsSelected();
    }

    private void UpdateIsSelected()
    {
        if (GameMultiplayer.Instance.GetPlayerData().characterId == characterId)
        {
            Debug.Log("selected" +  characterId);
            Color currentColor = image.color;
            currentColor.a = 0.5f;
            image.color = currentColor;
            Debug.Log(image.color);
        }
        else
        {
            Color currentColor = image.color;
            currentColor.a = 1;
            image.color = currentColor;
        }
    }

    private void OnDestroy()
    {
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged -= GameMultiplayer_OnPlayerDataNetworkListChanged;
    }
}