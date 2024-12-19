using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Cinemachine;
[RequireComponent(typeof(CharacterController))]
public class PlayerControllerChef : NetworkBehaviour 
{
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
    [SerializeField] private MovementChef movement;
    

    #region Variables : Animation
    [SerializeField] private Animator animator;
    private bool isJumping;
    
    private bool isMoving;
    #endregion
    public override void OnNetworkSpawn(){
        if(IsOwner){
            listener.enabled = true;
            vc.Priority = 1;
        }
        else{
            vc.Priority = 0;
        }
    }
    private void Start(){
        _characterController = GetComponent<CharacterController>();
        //_mainCamera = Camera.main;
        playerInput = new();
        playerInput.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update(){
        if(IsOwner){
        ApplyRotation();
        ApplyGravity();
        ApplyMovement();
        HandleCameraRotation();
        ApplyAnimationStates();}
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

    private void ApplyAnimationStates() {
        // Handle Running
        if (isMoving) {
            if(movement.isSprinting)animator.SetBool("isRunning",true);
            else animator.SetBool("isRunning",false);
            animator.SetBool("isWalking", true);
        } else {
            animator.SetBool("isRunning",false);
            animator.SetBool("isWalking", false);
        }

        // Handle Jumping
        if (isJumping) {
            animator.SetBool("isJumping", true);
        } else {
            animator.SetBool("isJumping", false);
        }
    }
    private void ApplyRotation(){
        if(_input.sqrMagnitude == 0) return;
        _direction = Quaternion.Euler(0.0f, _mainCamera.transform.eulerAngles.y, 0.0f) * new Vector3(_input.x, 0.0f, _input.y);

        var targetRotation = Quaternion.LookRotation(_direction,Vector3.up);

        transform.rotation = Quaternion.RotateTowards(transform.rotation,targetRotation, rotationSpeed * Time.deltaTime);
    }
    private void ApplyMovement(){
        var targetSpeed = movement.isSprinting ? movement.speed * movement.multiplier : movement.speed;
        movement.currentSpeed = Mathf.MoveTowards(movement.currentSpeed, targetSpeed, movement.acceleration * Time.deltaTime); 
        _characterController.Move(_direction * movement.currentSpeed * Time.deltaTime);
    }

    private void ApplyGravity(){
        if(IsGrounded() && _velocity < 0){
            _velocity = -1.0f;
        }
        else{
            _velocity += _gravity * gravityMultiplier * Time.deltaTime;
        }
        _direction.y = _velocity;
    }

    public void Jump(InputAction.CallbackContext context){
        if(!context.started)return;
        if(!IsGrounded() && _numberOfJumps >= maxNumberOfJumps) return;
        if (_numberOfJumps == 0) 
        {
            isJumping = true; 
            StartCoroutine(WaitForLanding());
        }
        _numberOfJumps++;
        _velocity = jumpPower;

        //alternative, for the following jumps to be less powerful
        //_velocity = jumpPower / _numberOfJumps;
    }

    public void Sprint(InputAction.CallbackContext context){
        movement.isSprinting = context.started || context.performed;
    }

    public void Move(InputAction.CallbackContext context){
        _input = context.ReadValue<Vector2>();
        _direction = new Vector3(_input.x, 0.0f, _input.y);

        isMoving = _input.sqrMagnitude > 0;
    }    

    public void RightClick(InputAction.CallbackContext context){
        movement.isRightClicking = context.started || context.performed; 
    }

    private IEnumerator WaitForLanding(){
        yield return new WaitUntil(()=> !IsGrounded());
        yield return new WaitUntil(IsGrounded);
        
        isJumping = false;
        _numberOfJumps = 0;
    }
    private bool IsGrounded() => _characterController.isGrounded;
}

[System.Serializable]
public struct MovementChef{

    [HideInInspector] public bool isRightClicking;
    [HideInInspector] public bool isSprinting;
    [HideInInspector] public float currentSpeed;
    public float speed;
    public float multiplier;
    public float acceleration;


     
}