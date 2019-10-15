using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0649 //Disable warnings in console
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
	#region Movement Settings
	[Header("Movement Settings")]
	[SerializeField]
	[Tooltip("The reference camera to move.")]
	private CameraBehaviour cam;
	[SerializeField]
	[Tooltip("Speed when moving sideways.")]
	private float strafeSpeed = 10f;
	[SerializeField]
	[Tooltip("Speed when moving forward.")]
	private float forwardSpeed = 10f;
	[SerializeField]
	[Tooltip("Speed when moving backwards.")]
	private float backwardsSpeed = 10f;
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
	//private bool IsGrounded => Physics.Raycast(transform.position, Vector3.down, distanceToGround);
	private bool IsGrounded { get; set; }
	private CharacterController controller;
	private Vector3 input = Vector3.zero;
	private Vector3 camR, camF;
	private Vector3 motion;
    // Start is called before the first frame update
    void Start()
    {
		controller = GetComponent<CharacterController>();
		controller.slopeLimit = slopeLimit;
		controller.stepOffset = stepOffset;
		distanceToGround = controller.bounds.extents.y + distanceToGroundOffset;
	}

    // Update is called once per frame
    void Update()
    {
		ProccessGravity();
		CalculateInput();
		CalculateCamera();
		Move();
    }

	private void ProccessGravity()
	{
		IsGrounded = Physics.Raycast(transform.position, Vector3.down, distanceToGround);
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
		controller.Move(jumpVector * Time.deltaTime);
	}

	private void ProccessJump()
	{
		verticalVelocity = jumpForce;
	}

	private void CalculateInput()
	{
		input = new Vector3(Input.GetAxis("Horizontal") * strafeSpeed, 0, Input.GetAxis("Vertical"));
		if (input.z > 0)
			input.z *= forwardSpeed;
		else if (input.z < 0)
			input.z *= backwardsSpeed;
	}

	private void CalculateCamera()
	{
		camF = cam.transform.forward;
		camR = cam.transform.right;
		camF.y = 0;
		camR.y = 0;
		camF = camF.normalized;
		camR = camR.normalized;
	}

	private void Move()
	{
		motion = camR * input.x + camF * input.z;
		if (input.x != 0 || input.z != 0)
		{
			Quaternion rot = Quaternion.LookRotation(motion);
			transform.rotation = Quaternion.Lerp(transform.rotation, rot, turnSpeed * Time.deltaTime);
		}
		controller.Move(motion * Time.deltaTime);
	}

	

}
