using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
public class PlayerInteractVeggie : NetworkBehaviour
{
    public Camera cam;
    [SerializeField] private float distance = 3f;
    [SerializeField] private LayerMask mask;
    private PlayerController pc;
    
    // Revival variables
    [SerializeField] private float reviveHoldTime = 3f;
    private float currentHoldTime = 0f;
    private bool isHolding = false;
    private Vegetable targetVegetable;

    [SerializeField] private float reviveProgress = 0f;
    void Start()
    {
        pc = GetComponent<PlayerController>();
        cam = pc._mainCamera;
        
    }
    public void Interact(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        
        // Handle the hold/release logic
        if (context.started)
        {
            isHolding = true;
        }
        else if (context.canceled)
        {
            isHolding = false;
            currentHoldTime = 0f;
            reviveProgress = 0f;
            targetVegetable = null;
        }
    }
    void Update()
    {
        if (!IsOwner) return;
        if(isHolding){
            if(pc.HasCheese()){
                pc.DropCheese(false);
            }
        }
            
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            Debug.DrawRay(ray.origin, ray.direction * distance, Color.red);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, distance, mask))
            {
                Vegetable vegetable = hitInfo.collider.GetComponentInParent<Vegetable>();
                if (vegetable != null)
                {
                    
                    if (vegetable.isParalyzed.Value)
                    {
                        if (isHolding)
                        {
                            
                            if (targetVegetable == null || targetVegetable != vegetable)
                            {
                                targetVegetable = vegetable;
                                currentHoldTime = 0f;
                            }

                            currentHoldTime += Time.deltaTime;
                            reviveProgress = currentHoldTime / reviveHoldTime;

                            // Optional: Debug visual feedback
                            Debug.Log($"Reviving: {reviveProgress * 100}%");

                            // Check if we've held long enough
                            if (currentHoldTime >= reviveHoldTime)
                            {
                                ReviveVegetableServerRpc(vegetable.NetworkObjectId);
                                currentHoldTime = 0f;
                                reviveProgress = 0f;
                                isHolding = false;
                                targetVegetable = null;
                            }
                        }
                    }
                }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void ReviveVegetableServerRpc(ulong vegetableId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(vegetableId, out NetworkObject vegetableNetObj))
        {
            if (vegetableNetObj.TryGetComponent<Vegetable>(out var vegetable))
            {
                // Call a new Revive method on the Vegetable
                vegetable.Revive();
                Debug.Log($"Revived vegetable: {vegetable.name}");
            }
        }
    }

}