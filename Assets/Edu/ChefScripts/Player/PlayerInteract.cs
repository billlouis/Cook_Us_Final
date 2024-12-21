using UnityEngine;
using Unity.Netcode;

public class PlayerInteract : NetworkBehaviour
{
    private Camera cam;
    [SerializeField] private float distance = 3f;
    [SerializeField] private LayerMask mask;
    private PlayerUI playerUI;
    private InputManager inputManager;

    [SerializeField] private Transform pickupPoint; // Reference to where vegetables should be held
    private GameObject heldVegetable;

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

        if (heldVegetable == null)
        {
            // Raycast to find interactable vegetables
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
                        PickUpVegetableServerRpc(vegetable.NetworkObjectId);
                    }
                }
            }
        }
        else
        {
            // If holding a vegetable, allow dropping it
            playerUI.UpdateText("Press E to Drop");

            if (inputManager.onFoot.Interact.triggered)
            {
                DropVegetableServerRpc(heldVegetable.GetComponent<NetworkObject>().NetworkObjectId);
            }
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void PickUpVegetableServerRpc(ulong vegetableId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(vegetableId, out NetworkObject vegetableNetObj))
        {
            if (vegetableNetObj.TryGetComponent<Vegetable>(out var vegetable))
            {
                if (vegetable.CanBePickedUp())
                {
                    vegetable.OnPickedUp(pickupPoint); // Server handles pickup logic
                    heldVegetable = vegetable.gameObject;

                    Debug.Log($"Picked up vegetable: {vegetable.name}");
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DropVegetableServerRpc(ulong vegetableId)
    {
        if (heldVegetable == null) return;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(vegetableId, out NetworkObject vegetableNetObj))
        {
            if (vegetableNetObj.TryGetComponent<Vegetable>(out var vegetable))
            {
                vegetable.OnDropped(); // Server handles drop logic
                heldVegetable = null; // Clear the held vegetable reference

                Debug.Log($"Dropped vegetable: {vegetable.name}");
            }
        }
    }


}