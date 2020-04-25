using UnityEngine;

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
	[SerializeField]
	private LayerMask groundCheckLayer;

	private Vector3 motionHorizontal = Vector2.zero;
	private float motionVertical = 0f;
	private Vector3 motion;
	private float angle = 0f;

	private Vector3 dodgeStartPos, dodgeEndPos;

	#endregion

	#region Physics Settings

	[Header("Physics Settings")]
	
	[SerializeField]
	[Tooltip("The center of the Capsule.")]
	private Vector3 center = Vector3.zero;
	[SerializeField]
	[Tooltip("The height of the capsule.")]
	private float height = 0.8f;
	[SerializeField]
	private float radius = 0.2f;

	public float Gravity => 25f;
	private bool isGrounded = true, wasGrounded = true, onSlope = false;
	private bool isJumping = false;

	#endregion

	#region External Properties

	private PlayerInputSystem input { get { return PlayerCenterControl.Instance.input; } }
	private Animator anim { get { return PlayerCenterControl.Instance.anim; } }
	private CameraBehaviour cam { get { return PlayerCenterControl.Instance.camera; } }
	private CharacterController controller;

	#endregion

	public bool lockGravity { get; set; } = false;
	public bool lockMovement { get; set; } = false;
	public bool lockRotation { get; set; } = false;

	#region Common Methods

	private void Start()
	{
		controller = GetComponent<CharacterController>();
	}

	#endregion

	#region State Methods

	public void Move()
	{
		if (!lockGravity)
		{
			wasGrounded = isGrounded;
			isGrounded = Physics.CheckCapsule(transform.position + center + Vector3.up * height / 2, transform.position + center + Vector3.down * height / 2, radius, groundCheckLayer, QueryTriggerInteraction.UseGlobal);
			anim.SetBool("Is Grounded", isGrounded);
			if (isGrounded)
			{
				if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, height, groundCheckLayer, QueryTriggerInteraction.UseGlobal))
					onSlope = hit.normal != Vector3.up;
				else
					onSlope = false;

				if (motionVertical >= -Gravity)
					motionVertical -= Gravity * Time.deltaTime;
				if (onSlope && wasGrounded && !isJumping)
				{
					motionVertical = -Gravity * 500f;
				}
			}
			else
			{
				motionVertical -= Gravity * Time.deltaTime;

				if (wasGrounded && !isJumping)
				{
					motionVertical = 0f;
				}
			}
		}

		if (!lockMovement)
		{
			Vector3 dir = (cam.Forward * input.Vertical + cam.Right * input.Horizontal).normalized * (input.Run ? runSpeed : walkSpeed);

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

		}

		motion = new Vector3(motionHorizontal.x, motionVertical, motionHorizontal.z);

		controller.Move(motion * Time.deltaTime);
	}

	private bool Rotate(Vector3 dir, float controlEfficiency = 1f)
	{
		if (dir == Vector3.zero || lockRotation)
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

	public void Dodge()
	{
		Vector3 dir = motionHorizontal.normalized;
		if (input.Horizontal == 0 && input.Vertical == 0)
			dir = -transform.forward;
		dir *= dodgeDistance;
		dodgeStartPos = transform.position;
		dodgeEndPos = transform.position + dir;
	}

	public bool UpdateDodge(ref float timeDodging)
	{
		timeDodging += Time.deltaTime;
		if (timeDodging >= dodgeTime)
		{
			return false;
			//PlayerStateSystem.ExitState();
		}
		return true;
	}

	public void Jump()
	{
		isJumping = true;
		anim.SetTrigger("Jump");
		motionVertical = Mathf.Sqrt(2 * Gravity * jumpHeight);
	}

	public bool UpdateJump()
	{
		if (isGrounded && motionVertical <= 0)
		{
			isJumping = false;
			return false;
		}
		return true;
	}

	#endregion

	private void OnDrawGizmosSelected()
	{
		Util.DrawWireCapsule(transform.position + center, Quaternion.identity, radius, height, Color.green);
	}

}
