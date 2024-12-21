using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Cinemachine;
using System;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour 
{

    private Vegetable vege;
    private NetworkVariable<bool> canMove = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private int maxHealth = 100;
    public HealthBarScript healthBar;
    public GameObject hb;
    float mass = 2.0F;
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private float knockbackDuration = 0.3f; // How long the knockback lasts
    private float currentKnockbackTime = 0f;
    private Vector3 currentKnockbackVelocity = Vector3.zero;
    private bool isBeingKnockedBack = false;
    #region Variables : Movement
    private Vector2 _input;
    private CharacterController _characterController;
    private PlayerInput playerInput;
    [SerializeField] private float speed;
    private float _velocity;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private CinemachineVirtualCamera vc;
    [SerializeField] private AudioListener listener;
    #endregion

    #region Variables : Rotation
    private float _cameraRotationY;
    private float _cameraRotationX;
    [SerializeField] private float cameraRotationLimit = 45f;
    [SerializeField] private Transform cameraPivot;
    private Vector3 _direction;
    [SerializeField] private float rotationSpeed = 5f;
    #endregion

    #region Variables : Gravity
    private float _gravity = -9.81f;
    [SerializeField] private float gravityMultiplier = 1.0f;
    #endregion

    #region Variables : Jumping
    [SerializeField] private float jumpPower;
    private int _numberOfJumps;
    [SerializeField] private int maxNumberOfJumps = 2;
    #endregion

    [SerializeField] private Movement movement;

    #region Variables : Cheese
    private GameObject heldCheese; // Only one cheese at a time
    [SerializeField] private Transform cheeseAttachPoint;
    [SerializeField] private GameObject cheesePrefab;
    [SerializeField] private GameObject cheeseNetworkPrefab;

    private NetworkVariable<bool> hasCheese = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    #endregion

    #region Variables : Animation
    [SerializeField] private Animator animator;

    private NetworkVariable<bool> isJumping = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private bool isMoving;

    // Network variables for animation states
    private NetworkVariable<bool> isWalking = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> isRunning = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    #endregion

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            healthBar.SetMaxHealth (maxHealth);
            hb.gameObject.SetActive(true);
            listener.enabled = true;
            vc.Priority = 1;
            _mainCamera.gameObject.SetActive(true);
        }
        else
        {   
            hb.gameObject.SetActive(false);
            vc.Priority = 0;
            _mainCamera.gameObject.SetActive(false);
        }
        if (IsServer)
        {
            currentHealth.OnValueChanged += OnHealthChanged;
        }
        // Subscribe to the network variable changes for animation sync
        isWalking.OnValueChanged += OnWalkingStateChanged;
        isRunning.OnValueChanged += OnRunningStateChanged;
        isJumping.OnValueChanged += OnJumpingStateChanged;

        hasCheese.OnValueChanged += (previous, current) => {
            if (current) AttachCheese();
            else DetachCheese();
        };
    }
    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            currentHealth.OnValueChanged -= OnHealthChanged;
        }
    }
    private void OnHealthChanged(int previousValue, int newValue)
    {
        
        // Debug.Log("newVal" + newValue);
        // Debug.Log("prevVal:" + previousValue);
        // Debug.Log("hbv"+ healthBar.slider.value);
        // Debug.Log("currentHealth" + currentHealth);
        if (newValue <= 0)
        {
            HandleDeath();  
        }
    }
    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        playerInput = new();
        playerInput.Enable();
        vege = GetComponent<Vegetable>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (IsOwner)
        {
            if(!vege.isParalyzed.Value){
                ApplyRotation();
                ApplyGravity();
                
                // Handle knockback first
                if (isBeingKnockedBack)
                {
                    healthBar.SetHealth (currentHealth.Value);
                    ApplyKnockback();
                }
                
                // Only apply normal movement if not being knocked back
                if (!isBeingKnockedBack)
                {
                    ApplyMovement();
                }
                
                HandleCameraRotation();
                ApplyAnimationStates();
            }
        }
    }
    private void ApplyKnockback()
    {
        if (_characterController == null || !_characterController.enabled)
        {
            return; // Skip knockback if CharacterController is inactive
        }

        if (currentKnockbackTime < knockbackDuration)
        {
            float knockbackProgress = currentKnockbackTime / knockbackDuration;
            Vector3 knockbackMove = Vector3.Lerp(currentKnockbackVelocity, Vector3.zero, knockbackProgress);

            _characterController.Move(knockbackMove * Time.deltaTime);

            currentKnockbackTime += Time.deltaTime;
        }
        else
        {
            isBeingKnockedBack = false;
            currentKnockbackTime = 0f;
            currentKnockbackVelocity = Vector3.zero;
        }
    }
    private void ApplyAnimationStates()
    {
        if (isMoving)
        {
            isWalking.Value = true;
            isRunning.Value = movement.isSprinting;
        }
        else
        {
            isWalking.Value = false;
            isRunning.Value = false;
        }

        // Jumping animation state
        
    }

    private void OnWalkingStateChanged(bool previous, bool current)
    {
        animator.SetBool("isWalking", current);
    }

    private void OnRunningStateChanged(bool previous, bool current)
    {
        animator.SetBool("isRunning", current);
    }

    private void OnJumpingStateChanged(bool previous, bool current)
    {
        animator.SetBool("isJumping", current);
    }
    public void TakeDamage(int damage, Vector3 dir, float force)
    {
        
        if (IsServer)
        {
            
            currentHealth.Value -= damage;
            
            
            // Calculate knockback velocity
            dir.Normalize();
            Vector3 knockbackVel = dir * force;
            
            // Notify clients to apply knockback
            ApplyKnockbackClientRpc(knockbackVel);
        }
    }
    [ClientRpc]
    private void ApplyKnockbackClientRpc(Vector3 knockbackVel)
    {
        // Start knockback on all clients
        currentKnockbackVelocity = knockbackVel;
        currentKnockbackTime = 0f;
        isBeingKnockedBack = true;
    }
    private void HandleDeath()
    {
        // Handle player death (e.g., respawn or end game logic)
        Debug.Log($"{gameObject.name} has died!");

        // Notify all clients of the death
        HandleDeathClientRpc();
    }

    [ClientRpc]
    private void HandleDeathClientRpc()
    {
        // Find the vegetable associated with the health change
        if (TryGetComponent<Vegetable>(out var vegetable))
        {
            vegetable.Paralyze(); // Call the paralyze method on the vegetable
        }

        // Visual feedback for death can go here (e.g., play animation)
        Debug.Log($"{gameObject.name} is now paralyzed and can be picked up.");
    }


    [ServerRpc(RequireOwnership = false)]
    public void ApplyKnockbackServerRpc(int damage, Vector3 knockbackDirection, float knockbackForce)
    {
        // Only the server can handle this call, even if the owner is not the client
        TakeDamage(damage, knockbackDirection, knockbackForce);
    }
    public void CollectCheese()
    {
        if (IsServer && !hasCheese.Value)
        {
            Debug.Log("Collect Cheese");
            // Set cheese ownership on the server
            hasCheese.Value = true;
        }
    }

    public void DropCheese()
    {
        if (IsServer && hasCheese.Value)
        {
            // Update cheese ownership on the server
            hasCheese.Value = false;

            // Spawn and throw cheese networked object
            GameObject networkCheese = Instantiate(cheeseNetworkPrefab, cheeseAttachPoint.position, Quaternion.identity);
            NetworkObject networkObject = networkCheese.GetComponent<NetworkObject>();
            networkObject.Spawn();

            // Apply a force to the dropped cheese (server-side only)
            Rigidbody rb = networkCheese.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 throwForce = (transform.forward + Vector3.up) * 1.5f;
                rb.AddForce(throwForce, ForceMode.Impulse);
            }
        }
    }

    private void AttachCheese()
    {
        if (heldCheese == null)
        {
            // Attach visual cheese on each client
            heldCheese = Instantiate(cheesePrefab, cheeseAttachPoint.position, Quaternion.identity);
            heldCheese.transform.SetParent(cheeseAttachPoint, true);
        }
    }

    private void DetachCheese()
    {
        if (heldCheese != null)
        {
            Destroy(heldCheese);
            heldCheese = null;
        }
    }

    public bool HasCheese()
    {
        return heldCheese != null;
    }

    private void HandleCameraRotation()
    {
        // Get mouse input
        Vector2 mouseInput = Mouse.current.delta.ReadValue();

        // Rotate cameraPivot up/down based on the vertical mouse input
        _cameraRotationX -= mouseInput.y * rotationSpeed * Time.deltaTime;
        _cameraRotationX = Mathf.Clamp(_cameraRotationX, -cameraRotationLimit, cameraRotationLimit);
        cameraPivot.localRotation = Quaternion.Euler(_cameraRotationX, 0, 0);

        // Rotate player left/right based on horizontal mouse input
        _cameraRotationY += mouseInput.x * rotationSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0, _cameraRotationY, 0);
    }

    private void ApplyRotation()
    {
        if (_input.sqrMagnitude == 0) return;

        _direction = Quaternion.Euler(0.0f, _mainCamera.transform.eulerAngles.y, 0.0f) * new Vector3(_input.x, 0.0f, _input.y);
        var targetRotation = Quaternion.LookRotation(_direction, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void ApplyMovement()
    {
        if (_characterController == null || !_characterController.enabled || !IsOwner)
        {
            return; // Skip movement if CharacterController is inactive or not owned by this player
        }

        var targetSpeed = movement.isSprinting ? movement.speed * movement.multiplier : movement.speed;
        movement.currentSpeed = Mathf.MoveTowards(movement.currentSpeed, targetSpeed, movement.acceleration * Time.deltaTime);
        _characterController.Move(_direction * movement.currentSpeed * Time.deltaTime);
    }


    private void ApplyGravity()
    {
        if (IsGrounded() && _velocity < 0)
        {
            _velocity = -1.0f;
        }
        else
        {
            _velocity += _gravity * gravityMultiplier * Time.deltaTime;
        }
        _direction.y = _velocity;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (!IsGrounded() && _numberOfJumps >= maxNumberOfJumps) return;
        if(IsOwner){
        if (_numberOfJumps == 0)
        {
            isJumping.Value = true;
            StartCoroutine(WaitForLanding());
        }
        _numberOfJumps++;
        _velocity = jumpPower;}
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        movement.isSprinting = context.started || context.performed;
    }

    public void Move(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
        _direction = new Vector3(_input.x, 0.0f, _input.y);

        isMoving = _input.sqrMagnitude > 0;
    }

    public void RightClick(InputAction.CallbackContext context)
    {
        movement.isRightClicking = context.started || context.performed;
    }

    private IEnumerator WaitForLanding()
    {
        yield return new WaitUntil(() => !IsGrounded());
        yield return new WaitUntil(IsGrounded);

        isJumping.Value = false;
        _numberOfJumps = 0;
    }

    private bool IsGrounded() => _characterController.isGrounded;
}

[System.Serializable]
public struct Movement
{
    [HideInInspector] public bool isRightClicking;
    [HideInInspector] public bool isSprinting;
    [HideInInspector] public float currentSpeed;
    public float speed;
    public float multiplier;
    public float acceleration;
}