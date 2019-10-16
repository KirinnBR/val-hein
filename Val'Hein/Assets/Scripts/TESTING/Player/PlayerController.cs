using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0649 //Disable warnings in console
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
	#region Movement Settings
	[Header("Movement Settings")]
	[SerializeField]
	[Tooltip("The reference camera to move.")]
	private CameraBehaviour cam;
	[SerializeField]
	[Tooltip("Movement Speed.")]
	private float speed = 10f;
	[SerializeField]
	[Tooltip("Acceleration speed. 0 - No acceleration. 1 - Instant max speed on movement.")]
	[Range(0,1)]
	private float acceleration = 0.2f;
	[SerializeField]
	[Tooltip("Speed to turn to direction.")]
	private float turnSpeed = 5f;
	#endregion
	[Space]
	#region Physics
	[Header("Physics")]
	[SerializeField]
	[Tooltip("The force of gravity that affects the player")]
	private float gravityForce = 20f;
	[SerializeField]
	[Tooltip("The force of the jumping.")]
	private float jumpForce = 15.0f;
	[SerializeField]
	[Range(0, 180)]
	[Tooltip("The maximum slope, in degrees, that the player can climb.")]
	private float slopeLimit = 45f;
	[SerializeField]
	[Tooltip("The maximum height, in meters, that the player can move up.")]
	private float stepOffset = 0.5f;
	#endregion
	[Space]
	#region Input Settings
	[Header("Input Settings")]
	[SerializeField]
	private KeyCode keyToJump = KeyCode.Space;
	#endregion
	[Space]
	#region Advanced
	[Header("Advanced")]
	[SerializeField]
	[Tooltip("This should ONLY be changed by the creator of the script.")]
	private float distanceToGroundOffset = 0.2f;
	#endregion

	private float distanceToGround;
	private float verticalVelocity;
	private bool IsGrounded => Physics.Raycast(transform.position, Vector3.down, distanceToGround);
	public bool CanMove { get; private set; }
	private CharacterController controller;
	private float inputHorizontal, inputVertical;
	private Vector3 camRight, camForward;
	private Vector3 motion;
	private Animator anim;
    // Start is called before the first frame update
    void Start()
    {
		controller = GetComponent<CharacterController>();
		anim = GetComponent<Animator>();
		controller.slopeLimit = slopeLimit;
		controller.stepOffset = stepOffset;
		distanceToGround = controller.bounds.extents.y + distanceToGroundOffset;
		CanMove = true;
	}

    // Update is called once per frame
    void Update()
    {
		ProccessGravity();
		CalculateInput();
		CalculateCamera();
		if (CanMove)
		{
			Move();
		}
    }

	private void ProccessGravity()
	{
		if (IsGrounded)
		{
			if (Input.GetKeyDown(keyToJump))
				ProccessJump();
		}
		else
		{
			verticalVelocity -= gravityForce * Time.deltaTime;
		}
		Vector3 jumpVector = new Vector3(0, verticalVelocity, 0);
		anim.SetBool("IsGrounded", IsGrounded);
		controller.Move(jumpVector * Time.deltaTime);
	}

	private void ProccessJump()
	{
		verticalVelocity = jumpForce;
		anim.SetTrigger("Jump");
	}

	private void CalculateInput()
	{
		inputHorizontal = Input.GetAxisRaw("Horizontal");
		inputVertical = Input.GetAxisRaw("Vertical");
	}

	private void CalculateCamera()
	{
		camForward = cam.transform.forward;
		camRight = cam.transform.right;
		camForward.y = 0;
		camRight.y = 0;
		camForward = camForward.normalized;
		camRight = camRight.normalized;
	}

	private void Move()
	{
		motion = Vector3.Lerp(motion, Vector3.ClampMagnitude(camForward * inputVertical + camRight * inputHorizontal, 1f) * speed, acceleration);
		if (inputHorizontal != 0 || inputVertical != 0)
		{
			Quaternion rot = Quaternion.LookRotation(motion);
			transform.rotation = Quaternion.Lerp(transform.rotation, rot, turnSpeed * Time.deltaTime);
		}
		anim.SetFloat("Speed", motion.magnitude);
		controller.Move(motion * Time.deltaTime);
	}
	

}
