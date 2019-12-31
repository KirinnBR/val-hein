using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0649 //Disable warnings in console
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
	private float stepHeight = 0.5f;
	[SerializeField]
	[Tooltip("The maximum height when jumping.")]
	private float jumpHeight = 5f;
	[SerializeField]
	[Tooltip("The time, in seconds, it takes for the player to dodge.")]
	private float dodgeTime = 0.25f;
	[SerializeField]
	[Tooltip("The force of the dodge.")]
	private float dodgeForce = 10f;

	public bool CanMove { get; set; }
	public bool IsJumping { get; private set; }
	public bool IsDodging { get; private set; } = false;
	private bool IsValidKeepJump { get; set; } = true;

	public Vector3 motion { get { return motionHorizontal; } }
	private Vector3 motionHorizontal;
	private float motionVertical;

	#endregion
	
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
	
	#region Physics Settings

	[Header("Physics Settings")]

	[Tooltip("The force of the gravity.")]
	[SerializeField]
	private float gravityForce = 25f;
	[Tooltip("The height configuration of the ground-check sphere.")]
	[SerializeField]
	private float distanceGroundCheck = -1.05f;
	[SerializeField]
	[Tooltip("The radius of the ground-check sphere.")]
	private float groundCheckSphereRadius = 0.3f;
	[SerializeField]
	[Tooltip("The layer to search for ground detection.")]
	private LayerMask groundCheckLayer;

	private bool OnSlope { get; set; }
	private bool IsGrounded { get; set; }
	private bool WasGrounded { get; set; }
	private bool IsMoving => inputHorizontal != 0 || inputVertical != 0;
	private float JumpVelocity => Mathf.Sqrt(2 * gravityForce * jumpHeight);
	private bool KnockingOnRoof { get; set; }

	#endregion

	#region External Properties

	private Animator anim { get { return Player.Instance.anim; } }
	private PlayerCombat combat { get { return Player.Instance.playerCombat; } }
	private CameraBehaviour cam { get { return Player.Instance.playerCamera; } }
	private CharacterController controller;

	#endregion

	private void Start()
    {
		controller = GetComponent<CharacterController>();
		controller.slopeLimit = slopeLimit;
		controller.stepOffset = stepHeight;
		CanMove = true;
	}

	private void Update()
    {
		if (GameManager.IsInitialized && GameManager.Instance.CurrentGameState != GameManager.GameState.Running) return;
		CheckGrounded();
		ApplyGravity();
		GetInput();
		Move();
		ProccessAnimations();
		WasGrounded = IsGrounded;
	}


	
	private void FixedUpdate()
	{
		KnockingOnRoof = controller.collisionFlags == CollisionFlags.Above;
	}
	
	private void ApplyGravity()
	{
		if (motionVertical <= 200f)
			motionVertical += -gravityForce * Time.deltaTime;

		if (IsJumping)
		{
			if (KnockingOnRoof)
				motionVertical = 0f;
		}
		else if (WasGrounded && !IsGrounded)
			motionVertical = 0f;

		if (OnSlope && !IsJumping && WasGrounded && IsGrounded)
			controller.Move(Vector3.down * gravityForce * 500f * Time.deltaTime);
		else
			controller.Move(Vector3.up * motionVertical * Time.deltaTime);

		if (motionVertical <= 0)
			IsValidKeepJump = false;
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
		Vector3 dir = (cam.Forward * inputVertical + cam.Right * inputHorizontal).normalized * (inputRun ? runSpeed : walkSpeed);

		if (!IsMoving)
			if (motionHorizontal.magnitude <= .001f)
				motionHorizontal = Vector3.zero;

		if (IsGrounded)
		{
			if (inputJump)
				Jump();
			if (inputDodge && CanMove && !IsDodging)
				StartCoroutine(OnDodge());
		}

		motionHorizontal = Vector3.Lerp(motionHorizontal, dir, acceleration * Time.deltaTime);
		if (combat.IsAttacking)
			motionHorizontal = Vector3.zero;

		if (dir != Vector3.zero && CanMove && !combat.HasTarget)
		{
			Quaternion rot = Quaternion.LookRotation(dir);
			if (combat.IsAttacking)
				transform.rotation = Quaternion.Lerp(transform.rotation, rot, turnSpeed / 2 * Time.deltaTime);
			else
				transform.rotation = Quaternion.Lerp(transform.rotation, rot, turnSpeed * Time.deltaTime);
		}

		controller.Move(motionHorizontal * Time.deltaTime);
	}

	private void Jump()
	{
		anim.SetTrigger("Jump");
		motionVertical = JumpVelocity;
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
		var pos = transform.position + Vector3.down * distanceGroundCheck;
		IsGrounded = Physics.CheckSphere(pos, groundCheckSphereRadius, groundCheckLayer, QueryTriggerInteraction.UseGlobal);
		if (IsGrounded)
		{
			if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundCheckSphereRadius, groundCheckLayer, QueryTriggerInteraction.UseGlobal))
				OnSlope = hit.normal != Vector3.up;
			else
				OnSlope = false;
		}
	}

	private IEnumerator OnJump()
	{
		IsJumping = true;
		IsValidKeepJump = true;

		while (IsGrounded)
		{
			yield return null;
			if (!IsValidKeepJump)
				break;
		}
		yield return new WaitUntil(() => IsGrounded);

		IsJumping = false;
	}

	private IEnumerator OnDodge()
	{
		CanMove = false;
		IsDodging = true;
		float timeDodging = dodgeTime;
		Vector3 dir = motionHorizontal.normalized;
		if (inputHorizontal == 0 && inputVertical == 0)
			dir = -transform.forward;
		Vector3 destination = dir * dodgeForce;
		while (timeDodging >= 0)
		{
			Quaternion rot = Quaternion.LookRotation(motionHorizontal == Vector3.zero ? transform.forward : motionHorizontal);
			if(!combat.HasTarget)
				transform.rotation = Quaternion.Lerp(transform.rotation, rot, turnSpeed * Time.deltaTime);
			timeDodging -= Time.deltaTime;
			controller.SimpleMove(destination);
			yield return null;
		}
		CanMove = true;
		IsDodging = false;
	}

	public void SetMovementEnabled(bool enabled)
	{
		CanMove = enabled;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position + Vector3.down * distanceGroundCheck, groundCheckSphereRadius);
	}

}
