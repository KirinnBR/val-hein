using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

#pragma warning disable CS0649
[RequireComponent(typeof(CharacterController))]
public class PlayerControllerSystem : MonoBehaviour
{
	#region Movement Settings

	[Header("Movement Settings")]

	[Tooltip("Movement speed when walking.")]
	[SerializeField]
	private float walkSpeed = 5f;
	[Tooltip("Movement speed when running.")]
	[SerializeField]
	private float runSpeed = 10f;
	[Tooltip("Acceleration, in meters per squared second.")]
	[SerializeField]
	private float acceleration = 10f;
	[Tooltip("Speed to turn to direction.")]
	[SerializeField]
	private float turnSpeed = 10f;
	public float TurnSpeed { get { return turnSpeed; } }

	[Tooltip("The maximum height when jumping.")]
	[SerializeField]
	private float jumpHeight = 5f;
	[Tooltip("The time, in seconds, it takes for the player to dodge.")]
	[SerializeField]
	private float dodgeTime = 0.25f;
	[Tooltip("The force of the dodge.")]
	[SerializeField]
	private float dodgeDistance = 10f;

	public bool DodgeBlocked { get; set; } = false;
	public bool JumpBlocked { get; set; } = false;
	public bool RotationBlocked { get; set; } = false;
	public bool MovementBlocked { get; set; } = false;
	public bool RunBlocked { get; set; } = false;

	private Vector3 motionHorizontal = Vector3.zero;
	private float motionVertical = 0f;
	private float angle = 0f;

	private Vector3 dodgeStartPos;

	#endregion

	#region Physics Settings

	[Header("Physics Settings")]
	
	[SerializeField]
	[Tooltip("The center of the Capsule.")]
	private Vector3 center = Vector3.zero;
	[SerializeField]
	[Tooltip("The height of the capsule.")]
	private float height = 0.8f;

	public float Gravity => 25f;
	public bool isGrounded, wasGrounded, onSlope;

	#endregion

	#region States References

	private BaseState<PlayerControllerSystem> currentState;
	public PlayerJumpingState jumpingState { get; } = new PlayerJumpingState();
	public PlayerMovingState movingState { get; } = new PlayerMovingState();
	public PlayerDodgingState dodgingState { get; } = new PlayerDodgingState();

	#endregion

	#region External Properties

	public PlayerInputSystem input { get { return PlayerCenterControl.Instance.input; } }
	private Animator anim { get { return PlayerCenterControl.Instance.anim; } }
	private CameraBehaviour cam { get { return PlayerCenterControl.Instance.camera; } }
	private PlayerCombatSystem combat { get { return PlayerCenterControl.Instance.combat; } }
	private LayerMask groundCheckLayer { get { return PlayerCenterControl.Instance.physicsCheckLayer; } }
	private CharacterController controller;

	#endregion

	#region Common Methods

	private void Start()
	{
		controller = GetComponent<CharacterController>();
		TransitionToState(movingState);
	}

	private void Update()
	{
		Profiler.BeginSample("UPDATING STATE");
		currentState.Update(this);
		Profiler.EndSample();
	}

	private void FixedUpdate()
	{
		CheckGrounded();
	}

	private void CheckGrounded()
	{
		wasGrounded = isGrounded;
		isGrounded = Physics.CheckCapsule(transform.position + center + (Vector3.up * (height / 2)), transform.position + center + (Vector3.down * (height / 2)), controller.radius, groundCheckLayer, QueryTriggerInteraction.UseGlobal);
		anim.SetBool("Is Grounded", isGrounded);
		if (isGrounded)
		{
			if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, controller.height, groundCheckLayer, QueryTriggerInteraction.UseGlobal))
				onSlope = hit.normal != Vector3.up;
			else
				onSlope = false;
		}
	}

	#endregion

	#region State Methods

	public void TransitionToState(BaseState<PlayerControllerSystem> newState)
	{
		currentState = newState;
		currentState.EnterState(this);
	}

	public void ProccessInput()
	{
		if (input.Jump && !JumpBlocked)
		{
			TransitionToState(jumpingState);
		}
		if (input.Dodge && !DodgeBlocked)
		{
			TransitionToState(dodgingState);
		}
	}

	public void ApplyGravity()
	{
		if (motionVertical >= -Gravity)
			motionVertical += -Gravity * Time.deltaTime;

		if (wasGrounded && !isGrounded)
		{
			motionVertical = 0f;
		}

		if (onSlope && wasGrounded && isGrounded)
			controller.Move(Vector3.down * Gravity * 500f * Time.deltaTime);
		else
			controller.Move(Vector3.down * -motionVertical * Time.deltaTime);
	}

	public void Move()
	{
		Vector3 dir;
		if (RunBlocked)
			dir = (cam.Forward * input.Vertical + cam.Right * input.Horizontal).normalized * walkSpeed;
		else
			dir = (cam.Forward * input.Vertical + cam.Right * input.Horizontal).normalized * (input.Run ? runSpeed : walkSpeed);

		if (MovementBlocked)
		{
			motionHorizontal = Vector3.Lerp(motionHorizontal, Vector3.zero, acceleration * Time.deltaTime);
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
			motionHorizontal = Vector3.Lerp(motionHorizontal, dir, acceleration * Time.deltaTime);
			Rotate(dir);
		}

		if (motionHorizontal.magnitude <= .01f)
			motionHorizontal = Vector3.zero;

		controller.Move(motionHorizontal * Time.deltaTime);
	}

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

	public void UpdateAnimations()
	{
		anim.SetFloat("Velocity", motionHorizontal.magnitude / runSpeed);
		anim.SetFloat("Velocity X", input.Horizontal, 0.3f, Time.deltaTime);
		anim.SetFloat("Velocity Z", input.Vertical, 0.3f, Time.deltaTime);
		anim.SetFloat("Angle", angle);
	}

	public Vector3 Dodge()
	{

		Vector3 dir = motionHorizontal.normalized;
		if (input.Horizontal == 0 && input.Vertical == 0)
			dir = -transform.forward;
		dir *= dodgeDistance;
		dodgeStartPos = transform.position;
		return dir;
	}

	public void UpdateDodge(Vector3 dir, ref float timeDodging)
	{
		if (Util.Lerp(dodgeStartPos, dodgeStartPos + dir, dodgeTime, ref timeDodging, controller))
		{
			TransitionToState(movingState);
		}
	}

	public void Jump()
	{
		combat.CanAttack = false;
		anim.SetTrigger("Jump");
		motionVertical = Mathf.Sqrt(2 * Gravity * jumpHeight);
	}

	public void UpdateJump()
	{
		Vector3 newDir = new Vector3(motionHorizontal.x, motionVertical, motionHorizontal.z);

		controller.Move(newDir * Time.deltaTime);

		if (isGrounded && motionVertical <= 0)
		{
			TransitionToState(movingState);
			combat.CanAttack = true;
		}

		motionVertical += -Gravity * Time.deltaTime;
	}

	#endregion

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
