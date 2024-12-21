using UnityEngine;
using Unity.Netcode;

public class PlayerInteract : NetworkBehaviour
{
    private Camera cam;
    [SerializeField] private float distance = 3f;
    [SerializeField] private LayerMask mask;
    [SerializeField] private LayerMask panMask;
    [SerializeField] private float panDetectionDistance = 3f;
    private PlayerUI playerUI;
    private InputManager inputManager;

    [SerializeField] private Transform pickupPoint;
    private Vector3 currentPanPosition;
    private Quaternion currentPanRotation;
    private NetworkVariable<ulong> heldVegetableId = new NetworkVariable<ulong>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private GameObject heldVegetable;

    private const string PAN_SPAWN_POINT_TAG = "PanSpawnPoint";

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        heldVegetableId.OnValueChanged += OnHeldVegetableChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        heldVegetableId.OnValueChanged -= OnHeldVegetableChanged;
    }

    private void OnHeldVegetableChanged(ulong previousValue, ulong newValue)
    {
        if (newValue == 0)
        {
            heldVegetable = null;
        }
        else if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(newValue, out NetworkObject netObj))
        {
            heldVegetable = netObj.gameObject;
        }
    }

    void Start()
    {
        cam = GetComponent<PlayerLook>().cam;
        playerUI = GetComponent<PlayerUI>();
        inputManager = GetComponent<InputManager>();
    }

    void Update()
    {
        if (!IsOwner) return;

        playerUI.UpdateText(string.Empty);
        HandleInteraction();
    }

    private void HandleInteraction()
    {
        if (heldVegetable == null)
        {
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            Debug.DrawRay(ray.origin, ray.direction * distance, Color.red);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, distance, mask))
            {
                Vegetable vegetable = hitInfo.collider.GetComponentInParent<Vegetable>();
                if (vegetable != null && vegetable.CanBePickedUp())
                {
                    playerUI.UpdateText("Press E to Pick Up");

                    if (inputManager.onFoot.Interact.triggered)
                    {
                        RequestPickUpVegetableServerRpc(vegetable.NetworkObjectId);
                    }
                }
            }
        }
        else
        {
            Ray panRay = new Ray(cam.transform.position, cam.transform.forward);
            Debug.DrawRay(panRay.origin, panRay.direction * panDetectionDistance, Color.blue);

            if (Physics.Raycast(panRay, out RaycastHit panHitInfo, panDetectionDistance, panMask))
            {
                Transform spawnPoint = FindPanSpawnPoint(panHitInfo.collider.gameObject);
                if (spawnPoint != null)
                {
                    currentPanPosition = spawnPoint.position;
                    currentPanRotation = spawnPoint.rotation;
                    playerUI.UpdateText("Press E to Place in Pan");
                    
                    if (inputManager.onFoot.Interact.triggered)
                    {
                        RequestPlaceInPanServerRpc(currentPanPosition, currentPanRotation);
                    }
                }
            }
            else
            {
                playerUI.UpdateText("Press E to Drop");
                
                if (inputManager.onFoot.Interact.triggered)
                {
                    RequestDropVegetableServerRpc();
                }
            }
        }
    }

    private Transform FindPanSpawnPoint(GameObject pan)
    {
        Transform spawnPoint = pan.transform.Find("SpawnPoint");
        if (spawnPoint != null)
        {
            return spawnPoint;
        }

        Transform[] allChildren = pan.GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child.CompareTag(PAN_SPAWN_POINT_TAG))
            {
                return child;
            }
        }

        Debug.LogWarning($"No spawn point found for pan {pan.name}");
        return null;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPickUpVegetableServerRpc(ulong vegetableId, ServerRpcParams rpcParams = default)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(vegetableId, out NetworkObject vegetableNetObj))
        {
            if (vegetableNetObj.TryGetComponent<Vegetable>(out var vegetable))
            {
                if (vegetable.CanBePickedUp())
                {
                    vegetable.OnPickedUp(pickupPoint);
                    heldVegetableId.Value = vegetableId;
                    ConfirmPickupClientRpc(vegetableId);
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestDropVegetableServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong currentVegetableId = heldVegetableId.Value;
        if (currentVegetableId != 0 && 
            NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(currentVegetableId, out NetworkObject vegetableNetObj))
        {
            if (vegetableNetObj.TryGetComponent<Vegetable>(out var vegetable))
            {
                vegetable.OnDropped();
                heldVegetableId.Value = 0;
                ConfirmDropClientRpc(currentVegetableId);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPlaceInPanServerRpc(Vector3 position, Quaternion rotation, ServerRpcParams rpcParams = default)
    {
        ulong currentVegetableId = heldVegetableId.Value;
        if (currentVegetableId != 0 && 
            NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(currentVegetableId, out NetworkObject vegetableNetObj))
        {
            if (vegetableNetObj.TryGetComponent<Vegetable>(out var vegetable))
            {
                vegetable.OnPlacedInPan(position, rotation);
                heldVegetableId.Value = 0;
                ConfirmPanPlacementClientRpc(currentVegetableId, position, rotation);
            }
        }
    }

    [ClientRpc]
    private void ConfirmPickupClientRpc(ulong vegetableId)
    {
        Debug.Log($"Pickup confirmed for vegetable: {vegetableId}");
    }

    [ClientRpc]
    private void ConfirmDropClientRpc(ulong vegetableId)
    {
        Debug.Log($"Drop confirmed for vegetable: {vegetableId}");
    }

    [ClientRpc]
    private void ConfirmPanPlacementClientRpc(ulong vegetableId, Vector3 position, Quaternion rotation)
    {
        Debug.Log($"Vegetable {vegetableId} placed in pan at position {position}");
    }
}