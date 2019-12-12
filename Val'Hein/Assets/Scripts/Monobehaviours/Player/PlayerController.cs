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
	[Tooltip("Movement speed when walking.")]
	private float walkSpeed = 5f;
	[SerializeField]
	[Tooltip("Movement speed when running.")]
	private float runSpeed = 10f;
	[SerializeField]
	[Tooltip("Acceleration, in meters per squared second.")]
	private float acceleration = 10f;
	[Tooltip("Speed to turn to direction.")]
	public float turnSpeed = 10f;
	[SerializeField]
	[Range(0, 180)]
	[Tooltip("The maximum slope, in degrees, that the player can climb.")]
	private float slopeLimit = 50f;
	[SerializeField]
	[Tooltip("The maximum height, in meters, that the player can move up.")]
	private float stepOffset = 0.5f;
	[SerializeField]
	[Tooltip("The maximum height when jumping.")]
	private float jumpHeight = 3f;
	[SerializeField]
	[Tooltip("The time, in seconds, it takes for the player to dodge.")]
	private float dodgeTime = 0.5f;
	[SerializeField]
	private float dodgeSpeed = 2f;

	public bool CanMove { get; set; }
	public bool IsJumping { get; private set; }
	private CameraBehaviour Camera { get { return PlayerCenterControl.Instance.playerCamera; } }
	private bool IsValidKeepJump { get; set; } = true;
	private CharacterController controller;
	public Vector3 motionHorizontal = Vector3.zero;
	Vector3 motionVertical = Vector3.zero;
	private float velocityY;

	#endregion
	[Space]
	#region Input Settings

	[Header("Input Settings")]

	[SerializeField]
	private KeyCode runKey = KeyCode.LeftShift;
	[SerializeField]
	private bool invertRun = true;
	[SerializeField]
	private KeyCode jumpKey = KeyCode.Space;
	[SerializeField]
	private KeyCode dodgeKey = KeyCode.LeftControl;

	private float inputHorizontal, inputVertical;
	private bool inputJump, inputDodge, inputRun;

	#endregion
	[Space]
	#region Physics Settings

	[Header("Physics Settings")]

	[SerializeField]
	private float gravityForce = 25f;
	[SerializeField]
	private float distanceGroundCheck = -1.05f;
	[SerializeField]
	private float sphereRadius = 0.3f;
	[SerializeField]
	private LayerMask groundLayer;
	[SerializeField]
	private float slopeForce = 50f;

	private bool OnSlope { get; set; }
	private bool IsMoving => inputHorizontal != 0 || inputVertical != 0;
	public bool IsGrounded { get; private set; }
	private float JumpVelocity => Mathf.Sqrt(2 * gravityForce * jumpHeight);

	#endregion
	[Space]
	#region Advanced Settings

	[Header("Advanced Settings")]

	[SerializeField]
	private float minimumMagnitudeToStop = 0.2f;

	#endregion

	private Animator anim;
	private PlayerCombat Combat { get { return PlayerCenterControl.Instance.playerCombat; } }

	private void Start()
    {
		controller = GetComponent<CharacterController>();
		anim = GetComponent<Animator>();
		controller.slopeLimit = slopeLimit;
		controller.stepOffset = stepOffset;
		CanMove = true;
	}

	private void Update()
    {
		if (GameManager.IsInitialized && GameManager.Instance.CurrentGameState != GameManager.GameState.Running) return;
		ApplyGravity();
		GetInput();
		Move();
		ProccessAnimations();
    }

	private void FixedUpdate()
	{
		CheckGrounded();
	}

	private void ApplyGravity()
	{
		velocityY += -gravityForce * Time.deltaTime;
		motionVertical = new Vector3(0, velocityY, 0);

		//Knock on the ceiling.
		if (IsJumping && controller.collisionFlags == CollisionFlags.Above)
			velocityY -= 1f;

		controller.Move(motionVertical * Time.deltaTime);

		//Check if is valid to keep the jump.
		if (velocityY <= 0)
			IsValidKeepJump = false;

		//Check if player is grounded and not jumping.
		if (IsGrounded && !IsJumping)
			velocityY = 0;
	}

	private void GetInput()
	{
		if (CanMove)
		{
			inputHorizontal = Input.GetAxisRaw("Horizontal");
			inputVertical = Input.GetAxisRaw("Vertical");
			inputJump = Input.GetKeyDown(jumpKey);
			inputDodge = Input.GetKeyDown(dodgeKey);
			if (!IsJumping)
				inputRun = invertRun ? !Input.GetKey(runKey) : Input.GetKey(runKey);
		}
		else
		{
			inputHorizontal = 0.0f;
			inputVertical = 0.0f;
			inputJump = false;
			inputDodge = false;
			inputRun = false;
		}
	}

	private void Move()
	{

		Vector3 dir = (Camera.Forward * inputVertical + Camera.Right * inputHorizontal).normalized * (inputRun ? runSpeed : walkSpeed);

		if (!IsMoving)
			if (motionHorizontal.magnitude <= minimumMagnitudeToStop)
				motionHorizontal = Vector3.zero;

		if (OnSlope && !IsJumping)
			controller.Move(Vector3.down * gravityForce * slopeForce * Time.deltaTime);

		if (IsGrounded)
		{
			if (inputJump)
				Jump();
			if (CanMove && inputDodge)
				StartCoroutine(OnDodge());
		}

		motionHorizontal = Vector3.Lerp(motionHorizontal, dir, acceleration * Time.deltaTime);
		if (Combat.IsAttacking)
			motionHorizontal = Vector3.zero;

		if (dir != Vector3.zero && CanMove && !Combat.HasTarget)
		{
			Quaternion rot = Quaternion.LookRotation(dir);
			if (Combat.IsAttacking)
				transform.rotation = Quaternion.Lerp(transform.rotation, rot, turnSpeed / 2 * Time.deltaTime);
			else
				transform.rotation = Quaternion.Lerp(transform.rotation, rot, turnSpeed * Time.deltaTime);
		}

		controller.Move(motionHorizontal * Time.deltaTime);
	}

	private void Jump()
	{
		anim.SetTrigger("Jump");
		velocityY = JumpVelocity;
		StartCoroutine(OnJump());
	}

	private void ProccessAnimations()
	{
		Vector2 velocity = new Vector2(controller.velocity.x, controller.velocity.z);
		anim.SetFloat("Speed", velocity.magnitude);
		anim.SetBool("IsGrounded", IsGrounded);
	}

	private void CheckGrounded()
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

	private IEnumerator OnJump()
	{
		IsJumping = true;
		IsValidKeepJump = true;

		while (IsGrounded)
		{
			yield return new WaitForEndOfFrame();
			if (!IsValidKeepJump)
				break;
		}
		yield return new WaitUntil(() => IsGrounded);

		IsJumping = false;
	}

	private IEnumerator OnDodge()
	{
		CanMove = false;
		float timeDodging = dodgeTime;
		Vector3 dir = motionHorizontal.normalized;
		if (dir == Vector3.zero)
			dir = -transform.forward;
		Vector3 destination = dir * dodgeSpeed;
		while (timeDodging >= 0)
		{
			Quaternion rot = Quaternion.LookRotation(motionHorizontal == Vector3.zero ? transform.forward : motionHorizontal);
			if(!Combat.HasTarget)
				transform.rotation = Quaternion.Lerp(transform.rotation, rot, turnSpeed * Time.deltaTime);
			timeDodging -= Time.deltaTime;
			controller.SimpleMove(destination);
			yield return null;
		}
		CanMove = true;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position + Vector3.down + Vector3.down * distanceGroundCheck, sphereRadius);
	}
}
