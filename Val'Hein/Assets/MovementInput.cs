//Copyright Filmstorm (C) 2018 - Movement Controller for Root Motion and built in IK solver
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script requires you to have setup your animator with 3 parameters, "InputMagnitude", "InputX", "InputZ"
//With a blend tree to control the inputmagnitude and allow blending between animations.
[RequireComponent(typeof(CharacterController))]
public class MovementInput : MonoBehaviour
{

    private float InputX; //Left and Right Inputs
    private float InputZ; //Forward and Back Inputs
    private Vector3 desiredMoveDirection; //Vector that holds desired Move Direction
    private bool blockRotationPlayer = false; //Block the rotation of the player?
    [Range(0,0.5f)] public float desiredRotationSpeed = 0.1f; //Speed of the players rotation
    public Animator anim; //Animator
    private float Speed; //Speed player is moving
    [Range(0,1f)] public float allowPlayerRotation = 0.1f; //Allow player rotation from inputs once past x
    public Camera cam; //Main camera (make sure tag is MainCamera)
    public CharacterController controller; //Character Controller, auto added on script addition

    public bool IsJumping { get; private set; }

    [Header("Animation Smoothing")]
    [Range(0, 1f)]
    public float HorizontalAnimSmoothTime = 0.2f; //InputX dampening
    [Range(0, 1f)]
    public float VerticalAnimTime = 0.2f; //InputZ dampening
    [Range(0, 1f)]
    public float StartAnimTime = 0.3f; //dampens the time of starting the player after input is pressed
    [Range(0, 1f)]
    public float StopAnimTime = 0.15f; //dampens the time of stopping the player after release of input

    #region Physics Settings

    [Header("Physics Settings")]

    [SerializeField]
    [Tooltip("The center of the Capsule.")]
    private Vector3 center = Vector3.zero;
    [SerializeField]
    [Tooltip("The height of the capsule.")]
    private float height = 0.8f;
    [SerializeField]
    [Tooltip("The maximum height when jumping.")]
    private float jumpHeight = 5f;

    private float Gravity => 25f;
    private float JumpVelocity => Mathf.Sqrt(2 * Gravity * jumpHeight);
    private bool isGrounded, wasGrounded, onSlope, knockingOnRoof;
    private bool isValidKeepJump;

    #endregion


    private float verticalVel; //Vertical velocity -- currently work in progress
    private Vector3 moveVector; //movement vector -- currently work in progress
    private float motionVertical;

    // Initialization of variables
    void Start()
    {
        anim = this.GetComponent<Animator>();
        cam = Camera.main;
        controller = this.GetComponent<CharacterController>();

        if (anim == null)
            Debug.LogError("We require " + transform.name + " game object to have an animator. This will allow for Foot IK to function");
    }


    // Update is called once per frame
    void Update()
    {
        InputMagnitude();
        ApplyGravity();
        wasGrounded = isGrounded;
    }

    private void CheckGrounded()
    {
        isGrounded = Physics.CheckCapsule(transform.position + center + (Vector3.up * (height / 2)), transform.position + center + (Vector3.down * (height / 2)), controller.radius, LayerMask.NameToLayer("Environment"), QueryTriggerInteraction.UseGlobal);
        if (isGrounded)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, controller.height, LayerMask.NameToLayer("Environment"), QueryTriggerInteraction.UseGlobal))
                onSlope = hit.normal != Vector3.up;
            else
                onSlope = false;
        }
    }

    private void FixedUpdate()
    {
        CheckGrounded();
    }

    private void ApplyGravity()
    {
        if (motionVertical >= -Gravity)
            motionVertical += -Gravity * Time.deltaTime;

        if (IsJumping)
        {
            if (knockingOnRoof)
                motionVertical = 0f;
        }
        else if (wasGrounded && !isGrounded)
            motionVertical = 0f;

        if (onSlope && !IsJumping && wasGrounded && isGrounded)
        {
            controller.Move(Vector3.down * Gravity * 500f * Time.deltaTime);
        }
        else
        {
            controller.Move(Vector3.down * -motionVertical * Time.deltaTime);
        }

        if (motionVertical <= 0)
            isValidKeepJump = false;
    }


    #region PlayerMovement
    void PlayerMoveAndRotation()
    {
        InputX = Input.GetAxis("Horizontal");
        InputZ = Input.GetAxis("Vertical");

        var camera = Camera.main;
        var forward = cam.transform.forward;
        var right = cam.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        desiredMoveDirection = forward * InputZ + right * InputX;
        
        if (blockRotationPlayer == false)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection), desiredRotationSpeed);
        }
    }

    void InputMagnitude()
    {
        //Calculate Input Vectors
        InputX = Input.GetAxis("Horizontal");
        InputZ = Input.GetAxis("Vertical");

        anim.SetFloat("InputZ", InputZ, VerticalAnimTime, Time.deltaTime * 2f);
        anim.SetFloat("InputX", InputX, HorizontalAnimSmoothTime, Time.deltaTime * 2f);

        //Calculate the Input Magnitude
        Speed = new Vector2(InputX, InputZ).sqrMagnitude;

        //Physically move player
        if (Speed > allowPlayerRotation)
        {
            anim.SetFloat("Input", Speed * 10f, StartAnimTime, Time.deltaTime);
            PlayerMoveAndRotation();
        }
        else if (Speed < allowPlayerRotation)
        {
            anim.SetFloat("Input", Speed * 10f, StopAnimTime, Time.deltaTime);
        }
    }

    #endregion




}





