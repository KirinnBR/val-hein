using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0649
#pragma warning disable CS0414
[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class CameraBehaviour : MonoBehaviour
{
	#region Target-Follow Settings
	[Header("Target-Follow Settings")]

	[SerializeField]
	[Tooltip("The target to follow.")]
	private Transform target;
	[SerializeField]
	[Tooltip("The minimum and maximum distance that the camera can go.")]
	private Vector2 distanceLimits = new Vector2(1f, 10f);
	[SerializeField]
	[Tooltip("The distance between the camera and the target.")]
	private float distance = 5f;
	[SerializeField]
	[Tooltip("The sensitivity of the mouse scroll wheel.")]
	private float mouseScrollWheelSensitivity = 1f;
	[SerializeField]
	[Tooltip("Variable to control the focus on player.")]
	private Vector3 playerOffset = new Vector3(0f, 1f, 0f);
	[SerializeField]
	[Tooltip("The minimum and maximum angle that the camera can go.")]
	private Vector2 angleLimits = new Vector2(0f, 60f);
	[SerializeField]
	[Tooltip("The sensitivity of the mouse in the X axis.")]
	private float mouseSensitivityX = 180f;
	[SerializeField]
	[Tooltip("The sensitivity of the mouse in the Y axis.")]
	private float mouseSensitivityY = 180f;

	private float currentDistance;

	#endregion

	#region Camera Collision Settings

	[Header("Camera Collision Settings")]

	[SerializeField]
	private LayerMask collisionLayer;

	private float curTime = 0f;
	private float collisionPointDistance = 0f;
	private Vector3 collisionTransition = Vector3.zero;

	#endregion

	#region Camera Focus Settings

	[Header("Camera Focus Settings")]

	[SerializeField]
	private float focusHeight = 0f;
	[SerializeField]
	[Range(0f, 360f)]
	private float xMinimum = 70f;
	[SerializeField]
	private float secondaryTargetTransitionSpeed = 2f;

	public Transform SecondaryTarget { get; set; }
	private bool HasSecondaryTarget => SecondaryTarget != null;

	#endregion

	private float heading = 0f, tilt = 20f;
	private Vector3 forward, right;
	public Vector3 Forward { get { return forward; } }
	public Vector3 Right { get { return right; } }

	private void Start()
	{
		currentDistance = distance;
	}

	// Update is called once per frame
	private void LateUpdate()
    {
		if (target == null) return;

		if (!HasSecondaryTarget)
		{
			Move(target.position - transform.forward * currentDistance + playerOffset);
		}
		else
		{
			Move(target.position - transform.forward * currentDistance + (playerOffset + (transform.right / 2)));
			var look = Quaternion.LookRotation(new Vector3(SecondaryTarget.position.x, focusHeight, SecondaryTarget.position.z) - transform.position);
			var lookClamped = look.eulerAngles;
			if (lookClamped.x >= xMinimum)
				lookClamped.x = xMinimum;
			transform.eulerAngles = lookClamped;
		}
		CalculateDirections();
	}
	
	private void Update()
	{
		if (!HasSecondaryTarget)
		{
			heading += Input.GetAxisRaw("Mouse X") * Time.deltaTime * mouseSensitivityX;
			if (heading >= 360f || heading <= -360f)
				heading = 0f;
			tilt -= Input.GetAxisRaw("Mouse Y") * Time.deltaTime * mouseSensitivityY;
			tilt = Mathf.Clamp(tilt, angleLimits.x, angleLimits.y);
			transform.rotation = Quaternion.Euler(tilt, heading, 0);
			float mouseScroll = -Input.GetAxisRaw("Mouse ScrollWheel");
			distance += mouseScroll * mouseScrollWheelSensitivity;
			distance = Mathf.Clamp(distance, distanceLimits.x, distanceLimits.y);
			currentDistance = Mathf.Lerp(currentDistance, distance, 5f * Time.deltaTime);
			if (Mathf.Abs(distance - currentDistance) <= 0.01f)
				currentDistance = distance;
        }
		else
		{
			tilt = transform.eulerAngles.x;
			heading = transform.eulerAngles.y;
		}
	}

	private void Move(Vector3 point)
	{
		if (Physics.Linecast(target.position + playerOffset, point, out RaycastHit hit, collisionLayer, QueryTriggerInteraction.Ignore))
		{
			transform.position = hit.point;
			collisionPointDistance = Vector3.Distance(point, hit.point);
			curTime = 0f;
		}
		else
		{
			collisionTransition = Util.LerpVector(transform.forward * collisionPointDistance, Vector3.zero, 1f, ref curTime);
			transform.position = point + collisionTransition;
		}
	}

	private void CalculateDirections()
	{
		forward = transform.forward;
		right = transform.right;
		forward.y = 0;
		right.y = 0;
		forward = forward.normalized;
		right = right.normalized;
	}
	
	private void OnDrawGizmosSelected()
	{
		if (target == null) return;

		Gizmos.color = Color.green;
		Vector3 targetPos = target.position + playerOffset;
		Gizmos.DrawLine(transform.position, targetPos);

		Gizmos.color = Color.red;
		Gizmos.DrawLine(targetPos, targetPos + (transform.forward * (distanceLimits.y - currentDistance)));
		Gizmos.DrawLine(transform.position, transform.position + (transform.forward * (distanceLimits.x - currentDistance)));

		if (HasSecondaryTarget)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(transform.position, new Vector3(SecondaryTarget.position.x, focusHeight, SecondaryTarget.position.z));
		}
	}
}