using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0649
[RequireComponent(typeof(CharacterController))]
public class PlayerControllerSystem : MonoBehaviour
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
	[SerializeField]
	private float stoppingAcceleration = 10f;
	[Tooltip("Speed to turn to direction.")]
	public float turnSpeed = 10f;
	[SerializeField]
	[Tooltip("The maximum height when jumping.")]
	private float jumpHeight = 5f;
	[SerializeField]
	[Tooltip("The percentage of control the player has when on jump.")]
	[Range(0f, 1f)]
	private float jumpControl = 0.6f;
	[SerializeField]
	[Tooltip("The time, in seconds, it takes for the player to dodge.")]
	private float dodgeTime = 0.25f;
	[SerializeField]
	[Tooltip("The force of the dodge.")]
	private float dodgeForce = 10f;
	public bool IsJumping { get; private set; } = false;
	private bool canJump = true;
	public bool IsDodging { get; private set; } = false;
	public bool RotationBlocked { get; set; } = false;
	public bool MovementBlocked { get; set; } = false;
	public bool RunBlocked { get; set; } = false;

	private Vector3 motionHorizontal = Vector3.zero;
	private float motionVertical = 0f;
	private float angle = 0f;

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
	private bool isGrounded, wasGrounded, onSlope, knockingOnRoof;

	#endregion

	#region External Properties

	private Animator anim { get { return PlayerCenterControl.Instance.anim; } }
	private PlayerCombatSystem combat { get { return PlayerCenterControl.Instance.combat; } }
	private CameraBehaviour cam { get { return PlayerCenterControl.Instance.camera; } }
	private PlayerInputSystem input { get { return PlayerCenterControl.Instance.input; } }
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
		ProccessInput();
		ProccessAnimations();
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
		{
			motionVertical = 0f;
		}

		if (onSlope && !IsJumping && wasGrounded && isGrounded)
			controller.Move(Vector3.down * Gravity * 500f * Time.deltaTime);
		else
			controller.Move(Vector3.down * -motionVertical * Time.deltaTime);
	}

	private void ProccessInput()
	{
		Vector3 dir;
		if (RunBlocked)
			dir = (cam.Forward * input.Vertical + cam.Right * input.Horizontal).normalized * walkSpeed;
		else
			dir = (cam.Forward * input.Vertical + cam.Right * input.Horizontal).normalized * (input.Run ? runSpeed : walkSpeed);

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

		Move(dir);
	}

	private void Move(Vector3 dir)
	{

		if (IsJumping)
		{
			controller.Move(motionHorizontal * Time.deltaTime);
			Rotate(dir, jumpControl);
			return;
		}

		if (MovementBlocked)
		{
			motionHorizontal = Vector3.Lerp(motionHorizontal, Vector3.zero, stoppingAcceleration * Time.deltaTime);
			Rotate(dir);
			controller.Move(motionHorizontal * Time.deltaTime);
			return;
		}
		
		if (dir != Vector3.zero)
		{
			if (Rotate(dir))
				motionHorizontal = Vector3.Lerp(motionHorizontal, dir, acceleration * Time.deltaTime);
		}
		else
		{
			motionHorizontal = Vector3.Lerp(motionHorizontal, dir, stoppingAcceleration * Time.deltaTime);
			Rotate(dir);
		}
			

		if (motionHorizontal.magnitude <= .01f)
			motionHorizontal = Vector3.zero;

		controller.Move(motionHorizontal * Time.deltaTime);
	}

	/// <summary>
	/// Rotates the player towards direction.
	/// </summary>
	/// <param name="dir">The direction to rotate.</param>
	/// <param name="controlEfficiency">The percentage of eficiency of the rotation.</param>
	/// <returns>If the angle between the player rotation and the desired rotation is less than 90 degrees.</returns>
	private bool Rotate(Vector3 dir, float controlEfficiency = 1f)
	{
		if (RotationBlocked) return false;
		if (dir == Vector3.zero)
		{
			angle = 0f;
			return false;
		}

		Quaternion dirRot = Quaternion.LookRotation(dir);
		angle = Quaternion.Angle(dirRot, transform.rotation);

		if (angle < 1f)
			transform.rotation = dirRot;
		else
			transform.rotation = Quaternion.Slerp(transform.rotation, dirRot, turnSpeed * Time.deltaTime * controlEfficiency);
		return angle < 90f;
	}

	private void Jump()
	{
		if (!canJump)
		{
			return;
		}
		anim.SetTrigger("Jump");
		motionVertical = Mathf.Sqrt(2 * Gravity * jumpHeight);
		StartCoroutine(OnJump());
	}

	private void ProccessAnimations()
	{
		anim.SetFloat("Velocity", motionHorizontal.magnitude / runSpeed);
		anim.SetFloat("Velocity X", input.Horizontal, 0.3f, Time.deltaTime);
		anim.SetFloat("Velocity Z", input.Vertical, 0.3f, Time.deltaTime);
		anim.SetFloat("Angle", angle);
		anim.SetBool("Is Grounded", isGrounded);
	}

	private void CheckGrounded()
	{
		wasGrounded = isGrounded;
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
		combat.CanAttack = false;
		canJump = false;

		while (isGrounded)
		{
			yield return null;
			if (motionVertical <= 0)
				break;
		}
		yield return new WaitUntil(() => isGrounded);
		MovementBlocked = true;
		IsJumping = false;
		yield return new WaitForSeconds(0.3f);
		canJump = true;
		combat.CanAttack = true;
		MovementBlocked = false;
	}

	private IEnumerator OnDodge()
	{
		combat.CanAttack = false;
		MovementBlocked = true;
		RotationBlocked = true;
		IsDodging = true;
		float timeDodging = dodgeTime;
		Vector3 dir = motionHorizontal.normalized;
		if (input.Horizontal == 0 && input.Vertical == 0)
			dir = -transform.forward;
		Vector3 destination = dir * dodgeForce;
		anim.SetTrigger("Dodge");
		while (timeDodging >= 0)
		{
			Quaternion rot = Quaternion.LookRotation(motionHorizontal == Vector3.zero ? transform.forward : motionHorizontal);
			if (!combat.HasTarget)
				transform.rotation = Quaternion.Lerp(transform.rotation, rot, turnSpeed * Time.deltaTime);
			timeDodging -= Time.deltaTime;
			controller.SimpleMove(destination);
			yield return null;
		}
		combat.CanAttack = true;
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
