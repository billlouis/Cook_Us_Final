using UnityEngine;
using Unity.Netcode;

public class Cheese : NetworkBehaviour
{
    public bool isCollected = false;
    public void CollectCheese(){
        if (!IsServer || isCollected) return;
        isCollected = true;

        GetComponent<NetworkObject>().Despawn();
    }
}
