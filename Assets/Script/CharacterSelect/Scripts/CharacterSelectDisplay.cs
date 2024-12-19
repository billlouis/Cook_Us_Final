using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using System;

public class CharacterSelectDisplay : NetworkBehaviour
{
    [SerializeField] private CharacterDatabase characterDatabase;
    [SerializeField] private Transform charactersHolder;
    [SerializeField] private CharacterSelectButton selectButtonPrefab;
    [SerializeField] private PlayerCard[] playerCards;
    [SerializeField] private GameObject characterInfoPanel;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private Button lockInbutton;

    private List<CharacterSelectButton> characterbuttons = new List<CharacterSelectButton>();

    private NetworkList<CharacterSelectState> players;

    private void Awake(){
        players = new NetworkList<CharacterSelectState>();
    }


    public override void OnNetworkSpawn(){
        if(IsClient){
            Character[] allCharacters = characterDatabase.GetAllCharacters();

            foreach(var character in allCharacters){
                var selectButtonInstance = Instantiate(selectButtonPrefab, charactersHolder);
                selectButtonInstance.SetCharacter(this, character);
                characterbuttons.Add(selectButtonInstance);
            }

            players.OnListChanged += HandlePlayerStateChanged;
        }

        if(IsServer){
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

            foreach(NetworkClient client in NetworkManager.Singleton.ConnectedClientsList){
                HandleClientConnected(client.ClientId);
            }
        }
    }

    public override void OnNetworkDespawn(){
        if(IsClient){
            players.OnListChanged -= HandlePlayerStateChanged; 
        }

        if(IsServer){
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }

    private void HandleClientConnected(ulong clientId){
        players.Add(new CharacterSelectState(clientId));
    }

    private void HandleClientDisconnected(ulong clientId){
        for (int i = 0; i < players.Count; i++)
        {
            if(players[i].ClientId == clientId){
                players.RemoveAt(i);
                break;
            }
        }
    }

    public void Select(Character character){
        for(int i =0; i< players.Count; i++){
            if(players[i].ClientId != NetworkManager.Singleton.LocalClientId){
                continue;
            }

            if(players[i].IsLockedIn){
                return;
            }

            if(players[i].CharacterId == character.Id){
                return;
            }

            if(IsCharacterTaken(character.Id, false)){
                return;
            }
        }


        characterNameText.text = character.DisplayName;

        characterInfoPanel.SetActive(true);
    
        SelectServerRpc(character.Id);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SelectServerRpc(int characterId, ServerRpcParams serverRpcParams = default){
        for(int i=0; i<players.Count; i++){
            if(players[i].ClientId != serverRpcParams.Receive.SenderClientId){
                continue;
            }

            if(!characterDatabase.IsValidCharacterId(characterId)){
                return;
            }

            if(IsCharacterTaken(characterId, true)){
                return;
            }

            players[i] = new CharacterSelectState(
                players[i].ClientId,
                characterId,
                players[i].IsLockedIn
            );
        }
    }

    public void LockIn() {
        LockInServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void LockInServerRpc(ServerRpcParams serverRpcParams = default){
        for(int i=0; i<players.Count; i++){
            if(players[i].ClientId != serverRpcParams.Receive.SenderClientId){
                continue;
            }

            if(!characterDatabase.IsValidCharacterId(players[i].CharacterId)){
                return;
            }

            if(IsCharacterTaken(players[i].CharacterId, true)){
                return;
            }

            players[i] = new CharacterSelectState(
                players[i].ClientId,
                players[i].CharacterId,
                true
            );
        }

        foreach(var player in players){
            if(!player.IsLockedIn){
                return;
            }
        }

        foreach (var player in players){
            ServerManager.Instance.SetCharacter(player.ClientId, player.CharacterId);
        }

        ServerManager.Instance.StartGame();
    }


    private void HandlePlayerStateChanged(NetworkListEvent<CharacterSelectState> changeEvent){
        for(int i = 0; i< playerCards.Length ; i++){
            if(players.Count > i){
                playerCards[i].UpdateDisplay(players[i]);
            } else {
                playerCards[i].DisableDisplay();
            }
        }

        foreach(var button in characterbuttons){
            if(button.IsDisabled){
                continue;
            }

            if(IsCharacterTaken(button.Character.Id, false)){
                button.SetDisabled();
            }
        }

        foreach(var player in players){
            if(player.ClientId != NetworkManager.Singleton.LocalClientId){
                continue;
            }

            if(player.IsLockedIn){
                lockInbutton.interactable = false;
                break;
            }

            if(IsCharacterTaken(player.CharacterId, false)){
                lockInbutton.interactable = false;
                break;
            }

            lockInbutton.interactable = true;

            break;
        }
    }

    private bool IsCharacterTaken(int characterId, bool checkAll){
        for(int i=0; i<players.Count; i++){
            if(!checkAll){
                if(players[i].ClientId == NetworkManager.Singleton.LocalClientId){
                    continue;
                }

                if(players[i].IsLockedIn && players[i].CharacterId == characterId){
                    return true;
                }
            }
        }

        return false;
    }
}
