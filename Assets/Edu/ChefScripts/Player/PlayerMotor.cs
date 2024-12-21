using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMotor : NetworkBehaviour
{
    private NetworkVariable<bool> isWalking = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner // Ensure the server updates this
    );

    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool isGrounded;
    private Animator animator;

    public float speed = 5f;
    public float gravity = -9.8f;
    public float jumpHeight = 3f;

    private bool crouching = false;
    private float crouchTimer = 1;
    private bool lerpCrouch = false;
    private bool sprinting = false;

    // Called when the object is spawned in the network
    public override void OnNetworkSpawn()
    {
        // Subscribe to walking state changes for all clients
        isWalking.OnValueChanged += OnWalkingStateChanged;
    }

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            isGrounded = controller.isGrounded;
        }
    }

    // Process movement input
    public void ProcessMove(Vector2 input)
    {
        if (!IsOwner) return;

        // Calculate movement direction
        Vector3 moveDirection = Vector3.zero;
        moveDirection.x = input.x;
        moveDirection.z = input.y;

        // Move the character
        controller.Move(transform.TransformDirection(moveDirection) * speed * Time.deltaTime);

        // Update the NetworkVariable for walking state
        bool walking = moveDirection.magnitude > 0;
        if (isWalking.Value != walking)
        {
            // Update the walking state on the server (so it propagates to all clients)
            isWalking.Value = walking;
        }

        // Handle gravity
        playerVelocity.y += gravity * Time.deltaTime;
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }
        controller.Move(playerVelocity * Time.deltaTime);
    }

    // Jump functionality
    public void Jump()
    {
        if (!IsOwner) return;

        if (isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravity);
        }
    }

    // Toggle crouch
    public void Crouch()
    {
        crouching = !crouching;
        crouchTimer = 0;
        lerpCrouch = true;
    }

    // Toggle sprint
    public void Sprint()
    {
        if (!IsOwner) return;

        sprinting = !sprinting;
        speed = sprinting ? 8 : 5;
    }

    // Callback for when the walking state changes
    private void OnWalkingStateChanged(bool previous, bool current)
    {
        animator.SetBool("isWalking", current);
    }
}
