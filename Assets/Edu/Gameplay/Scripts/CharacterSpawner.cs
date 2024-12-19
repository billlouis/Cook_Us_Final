using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CharacterSpawner : NetworkBehaviour
{
    [SerializeField] private CharacterDatabase characterDatabase;
    
    public override void OnNetworkSpawn(){
        if(!IsServer){
            return;
        }
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            
            var character = characterDatabase.GetCharacterById(GameMultiplayer.Instance.GetPlayerDataFromClientId(clientId).characterId);
            
            Debug.Log(GameMultiplayer.Instance.GetPlayerDataFromClientId(clientId));
            Debug.Log(GameMultiplayer.Instance.GetPlayerDataFromClientId(clientId).characterId);
            Debug.Log(characterDatabase.GetCharacterById(GameMultiplayer.Instance.GetPlayerDataFromClientId(clientId).characterId));
            Debug.Log(character);
            if(character != null){
                var spawnPos = new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
                var characterInstance = Instantiate(character.GameplayPrefab, spawnPos, Quaternion.identity);
                characterInstance.SpawnAsPlayerObject(clientId);
            }
        }
    }
}
