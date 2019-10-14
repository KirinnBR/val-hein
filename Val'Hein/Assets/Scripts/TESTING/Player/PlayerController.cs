using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0649 //Disable warnings in console
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
	[Header("Movement Settings")]
	[SerializeField]
	private CameraBehaviour cam;
	[SerializeField]
	private float strafeSpeed = 10f;
	[SerializeField]
	private float forwardSpeed = 10f;
	[SerializeField]
	private float backwardsSpeed = 10f;
	[SerializeField]
	private float turnSpeed = 5f;

	[Space]
	[Header("Physics")]
	[SerializeField]
	private float gravityForce = 20f;
	[SerializeField]
	private float jumpForce = 15.0f;

	[Space]
	[Header("Input Settings")]
	[SerializeField]
	private KeyCode keyToJump = KeyCode.Space;

	[Space]
	[Header("Advanced")]
	[SerializeField]
	private float distanceToGroundOffset = 0.2f;

	private float distanceToGround;
	private float verticalVelocity;
	private bool IsGrounded => Physics.Raycast(transform.position, Vector3.down, distanceToGround);
	private CharacterController controller;
	private Vector3 input = Vector3.zero;
	private Vector3 camR, camF;
	private Vector3 motion;
    // Start is called before the first frame update
    void Start()
    {
		controller = GetComponent<CharacterController>();
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
		if (IsGrounded || controller.isGrounded)
		{
			verticalVelocity = -gravityForce * Time.deltaTime;
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
		if (Input.GetKeyDown(keyToJump))
		{
			verticalVelocity = jumpForce;
		}
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
