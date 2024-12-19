using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private ChefInput chefInput;
    public ChefInput.OnFootActions onFoot;

    private PlayerMotor motor;
    private PlayerLook look;
    private MeleeAttack meleeAttack;

    // Start is called before the first frame update
    void Awake()
    {
        chefInput = new ChefInput();
        onFoot = chefInput.OnFoot;
        motor = GetComponent<PlayerMotor>();
        look = GetComponent<PlayerLook>();
        meleeAttack = GetComponent<MeleeAttack>();

        onFoot.Jump.performed += ctx => motor.Jump();

        onFoot.Crouch.performed += ctx => motor.Crouch();
        onFoot.Sprint.performed += ctx => motor.Sprint();

        onFoot.Attack.performed += ctx => meleeAttack.Attack();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        motor.ProcessMove(onFoot.Movement.ReadValue<Vector2>());
    }
    
    void LateUpdate(){
        look.ProcessLook(onFoot.Look.ReadValue<Vector2>());
    }

    private void OnEnable(){
        onFoot.Enable();
    }

    private void OnDisable(){
        onFoot.Disable();
    }
}
