using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0649
[RequireComponent(typeof(CharacterController))]
public class ControllerSystem : MonoBehaviour
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
	private float acceleration = 5f;
	[SerializeField]
	private float stoppingAcceleration = 10f;
	[Tooltip("Speed to turn to direction.")]
	public float turnSpeed = 10f;
	[SerializeField]
	[Tooltip("The maximum height when jumping.")]
	private float jumpHeight = 5f;
	[SerializeField]
	[Tooltip("The time, in seconds, it takes for the player to dodge.")]
	private float dodgeTime = 0.25f;
	[SerializeField]
	[Tooltip("The force of the dodge.")]
	private float dodgeForce = 10f;
	public bool IsJumping { get; private set; } = false;
	public bool IsDodging { get; private set; } = false;
	public bool RotationBlocked { get; set; } = false;
	public bool MovementBlocked { get; set; } = false;
	private bool hasRotationFinished = true;
	private bool isValidKeepJump = true;
	private Vector3 motionHorizontal = Vector3.zero;
	private float motionVertical = 0f;

	#endregion

	#region Physics Settings

	[Header("Physics Settings")]
	
	[SerializeField]
	[Tooltip("The center of the Capsule.")]
	private Vector3 center = Vector3.zero;
	[SerializeField]
	[Tooltip("The height of the capsule.")]
	private float height = 0.8f;

	private float Gravity => 25f;
	private bool MoveInput => input.Horizontal != 0 || input.Vertical != 0;
	private float JumpVelocity => Mathf.Sqrt(2 * Gravity * jumpHeight);
	private bool isGrounded, wasGrounded, onSlope, knockingOnRoof;

	#endregion

	#region External Properties

	private Animator anim { get { return PlayerCenterControl.Instance.anim; } }
	private CombatSystem combat { get { return PlayerCenterControl.Instance.combat; } }
	private CameraBehaviour cam { get { return PlayerCenterControl.Instance.playerCamera; } }
	private InputSystem input { get { return PlayerCenterControl.Instance.input; } }
	private LayerMask groundCheckLayer { get { return PlayerCenterControl.Instance.physicsCheckLayer; } }
	private CharacterController controller;

	#endregion

	private void Start()
	{
		controller = GetComponent<CharacterController>();
	}

	private void Update()
	{
		ApplyGravity();
		Move();
		ProccessAnimations();
		wasGrounded = isGrounded;
	}

	private void FixedUpdate()
	{
		CheckGrounded();
		knockingOnRoof = controller.collisionFlags == CollisionFlags.Above;
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
			controller.Move(Vector3.down * Gravity * 500f * Time.deltaTime);
		else
			controller.Move(Vector3.down * -motionVertical * Time.deltaTime);

		if (motionVertical <= 0)
			isValidKeepJump = false;
	}

	private void Move()
	{
		Vector3 dir = (cam.Forward * input.Vertical + cam.Right * input.Horizontal).normalized * (input.Run ? runSpeed : walkSpeed);

		if (MovementBlocked)
		{
			motionHorizontal = Vector3.Lerp(motionHorizontal, Vector3.zero, stoppingAcceleration * Time.deltaTime);
			Rotate(dir);
			controller.Move(motionHorizontal * Time.deltaTime);
			return;
		}
		
		if (isGrounded)
		{
			if (!IsDodging)
			{
				if (input.Jump)
					Jump();
				if (input.Dodge)
					StartCoroutine(OnDodge());
			}
		}

		if (MoveInput)
		{
			if (hasRotationFinished)
				motionHorizontal = Vector3.Lerp(motionHorizontal, dir, acceleration * Time.deltaTime);
			Rotate(dir);
		}
		else
			motionHorizontal = Vector3.Lerp(motionHorizontal, dir, stoppingAcceleration * Time.deltaTime);

		if (motionHorizontal.magnitude <= .01f)
			motionHorizontal = Vector3.zero;

		controller.Move(motionHorizontal * Time.deltaTime);
	}

	private void Rotate(Vector3 dir)
	{
		if (dir == Vector3.zero) return;
		if (RotationBlocked) return;

		Quaternion dirRot = Quaternion.LookRotation(dir);
		transform.rotation = Quaternion.Slerp(transform.rotation, dirRot, turnSpeed * Time.deltaTime);
		var angle = Quaternion.Angle(dirRot, transform.rotation);
		if (angle < 1f)
			transform.rotation = dirRot;
		hasRotationFinished = angle < 90f;
	}

	private void Jump()
	{
		anim.SetTrigger("Jump");
		motionVertical = JumpVelocity;
		StartCoroutine(OnJump());
	}

	private void ProccessAnimations()
	{
		var velocity = new Vector2(controller.velocity.x, controller.velocity.z);
		anim.SetFloat("Input", velocity.magnitude);
		anim.SetFloat("InputX", input.Horizontal);
		anim.SetFloat("InputZ", input.Vertical);
		anim.SetBool("IsGrounded", isGrounded);
	}

	private void CheckGrounded()
	{
		isGrounded = Physics.CheckCapsule(transform.position + center + (Vector3.up * (height / 2)), transform.position + center + (Vector3.down * (height / 2)), controller.radius, groundCheckLayer, QueryTriggerInteraction.UseGlobal);
		if (isGrounded)
		{
			if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, controller.height, groundCheckLayer, QueryTriggerInteraction.UseGlobal))
				onSlope = hit.normal != Vector3.up;
			else
				onSlope = false;
		}
	}
	

	private IEnumerator OnJump()
	{
		IsJumping = true;
		isValidKeepJump = true;

		while (isGrounded)
		{
			yield return null;
			if (!isValidKeepJump)
				break;
		}
		yield return new WaitUntil(() => isGrounded);

		IsJumping = false;
	}

	private IEnumerator OnDodge()
	{
		MovementBlocked = true;
		RotationBlocked = true;
		IsDodging = true;
		float timeDodging = dodgeTime;
		Vector3 dir = motionHorizontal.normalized;
		if (!MoveInput)
			dir = -transform.forward;
		Vector3 destination = dir * dodgeForce;
		while (timeDodging >= 0)
		{
			Quaternion rot = Quaternion.LookRotation(motionHorizontal == Vector3.zero ? transform.forward : motionHorizontal);
			if (!combat.HasTarget)
				transform.rotation = Quaternion.Lerp(transform.rotation, rot, turnSpeed * Time.deltaTime);
			timeDodging -= Time.deltaTime;
			controller.SimpleMove(destination);
			yield return null;
		}
		MovementBlocked = false;
		RotationBlocked = false;
		IsDodging = false;
	}

	private void OnDrawGizmosSelected()
	{
		if (UnityEditor.EditorApplication.isPlaying)
		{
			Util.DrawWireCapsule(transform.position + center, Quaternion.identity, controller.radius, height, Color.green);
		}
		else
		{
			var cc = GetComponent<CharacterController>();
			Util.DrawWireCapsule(transform.position + center, Quaternion.identity, cc.radius, height, Color.green);
		}
	}

}
