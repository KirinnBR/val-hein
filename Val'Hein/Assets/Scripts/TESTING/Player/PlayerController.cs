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
	private new CameraBehaviour camera;
	[SerializeField]
	[Tooltip("Movement Speed.")]
	private float speed = 10f;
	[SerializeField]
	[Tooltip("Acceleration, in meters per squared second.")]
	private float acceleration = 10f;
	[SerializeField]
	[Tooltip("Speed to turn to direction.")]
	private float turnSpeed = 5f;
	[SerializeField]
	[Range(0, 180)]
	[Tooltip("The maximum slope, in degrees, that the player can climb.")]
	private float slopeLimit = 45f;
	[SerializeField]
	[Tooltip("The maximum height, in meters, that the player can move up.")]
	private float stepOffset = 0.5f;
	[SerializeField]
	[Tooltip("The maximum height when jumping.")]
	private float jumpHeight = 2f;

	private bool CanMove { get; set; }
	private bool IsJumping { get; set; }
	private float origSlopeLimit;
	private CharacterController controller;
	private Vector3 motion;
	Vector3 motionHorizontal = Vector3.zero;
	Vector3 motionVertical = Vector3.zero;
	private float velocityY;

	#endregion
	[Space]
	#region Input Settings

	[Header("Input Settings")]

	[SerializeField]
	private KeyCode keyToJump = KeyCode.Space;

	private float inputHorizontal, inputVertical;
	private bool inputJump;

	#endregion
	[Space]
	#region Physics Settings

	[Header("Physics Settings")]

	[SerializeField]
	private float gravityForce = 12f;
	[SerializeField]
	private float distanceGroundCheck = 0.0f;
	[SerializeField]
	private float sphereRadius = 0.2f;
	[SerializeField]
	private LayerMask groundLayer;
	[SerializeField]
	private float slopeForce = 50f;

	private bool OnSlope { get; set; }
	private bool IsGrounded { get; set; }
	private float JumpVelocity => Mathf.Sqrt(2 * gravityForce * jumpHeight);

	#endregion

	private Animator anim;

	private void Start()
    {
		controller = GetComponent<CharacterController>();
		anim = GetComponent<Animator>();
		controller.slopeLimit = slopeLimit;
		controller.stepOffset = stepOffset;
		origSlopeLimit = slopeLimit;
		CanMove = true;
	}

	private void Update()
    {
		if (GameManager.Instance && GameManager.Instance.CurrentGameState != GameManager.GameState.Running) return;

		ApplyGravity();
		if (CanMove)
		{
			CalculateInput();
			Move();
		}
		ProccessAnimations();
    }

	private void FixedUpdate()
	{
		IsGrounded = controller.isGrounded;
		if (Physics.SphereCast(transform.position + Vector3.down * distanceGroundCheck, sphereRadius, Vector3.down, out RaycastHit groundHit, 1.0f, groundLayer, QueryTriggerInteraction.UseGlobal))
		{
			if (!IsGrounded)
				IsGrounded = true;
			OnSlope = groundHit.normal != Vector3.up;
		}
		else
			OnSlope = false;
	}

	public void EnableMovement(bool movement)
	{
		CanMove = movement;
	}

	private void ApplyGravity()
	{
		velocityY += -gravityForce * Time.deltaTime;
	}

	private void CalculateInput()
	{
		inputHorizontal = Input.GetAxisRaw("Horizontal");
		inputVertical = Input.GetAxisRaw("Vertical");
		inputJump = Input.GetKeyDown(keyToJump);
	}

	private void Move()
	{
		Vector3 dir = (camera.Forward * inputVertical + camera.Right * inputHorizontal).normalized * speed;

		if (inputJump && IsGrounded)
			Jump();

		motionHorizontal = Vector3.Lerp(motionHorizontal, dir, acceleration * Time.deltaTime);
		motionVertical = new Vector3(0, velocityY, 0);

		if (motionHorizontal != Vector3.zero)
		{
			Quaternion rot = Quaternion.LookRotation(motionHorizontal);
			transform.rotation = Quaternion.Lerp(transform.rotation, rot, turnSpeed * Time.deltaTime);
		}

		motion = motionHorizontal + motionVertical;
		controller.Move(motion * Time.deltaTime);

		if (controller.isGrounded)
		{
			velocityY = 0;
		}
	}

	private void LateUpdate()
	{
		ApplyExtraForce();
	}

	private void Jump()
	{
		anim.SetTrigger("Jump");
		velocityY = JumpVelocity;
		StartCoroutine(OnJump());
	}

	private void ApplyExtraForce()
	{
		if (OnSlope && !IsJumping)
			controller.Move(Vector3.down * gravityForce * slopeForce * Time.deltaTime);
	}

	private void ProccessAnimations()
	{
		Vector2 velocity = new Vector2(controller.velocity.x, controller.velocity.z);
		anim.SetFloat("Speed", velocity.magnitude);
		anim.SetBool("IsGrounded", IsGrounded);
	}

	private IEnumerator OnJump()
	{
		IsJumping = true;
		controller.slopeLimit = 90.0f;
		yield return new WaitWhile(() => velocityY != 0);
		controller.slopeLimit = origSlopeLimit;
		IsJumping = false;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position + Vector3.down * distanceGroundCheck + Vector3.down, sphereRadius);
	}

}
