using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0649
[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class CameraBehaviour : MonoBehaviour
{
	#region Target-Follow Settings
	[Header("Target-Follow Settings")]

	[SerializeField]
	[Tooltip("The target to follow.")]
	private Transform target;
	[Tooltip("The distance between the camera and the target.")]
	public float distance = 10f;
	[Tooltip("The sensitivity of the mouse scroll wheel.")]
	[SerializeField]
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

	#endregion
	[Space]
	#region Camera Collision Settings

	[Header("Camera Collision Settings")]

	[SerializeField]
	private LayerMask collisionLayer;

	#endregion
	[Space]
	#region Camera Focus Settings

	[Header("Camera Focus Settings")]

	[SerializeField]
	private float distanceFocused = 1f;
	[SerializeField]
	private float distanceTransitionSpeed = 5f;
	[SerializeField]
	private SideSelector cameraSide = SideSelector.Left;

	public Transform Focus { get; set; }

	#endregion

	private float heading = 0f, tilt = 20f;
	private float mouseScroll = 0f;
	private float currentDistance;
	

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
		if (Focus == null)
		{
			currentDistance = Mathf.Lerp(currentDistance, distance, distanceTransitionSpeed * Time.deltaTime);
			Move(target.position - transform.forward * currentDistance + playerOffset);
		}
		else
		{
			currentDistance = Mathf.Lerp(currentDistance, distanceFocused, distanceTransitionSpeed * Time.deltaTime);
			transform.LookAt(Focus.position);
			if (cameraSide == SideSelector.Left)
				Move(target.position - transform.forward * currentDistance + (playerOffset + (-transform.right / 2)));
			else
				Move(target.position - transform.forward * currentDistance + (playerOffset + (transform.right / 2)));
		}
		CalculateDirections();
	}
	
	private void Update()
	{
		if (Focus == null)
		{
			heading += Input.GetAxis("Mouse X") * Time.deltaTime * mouseSensitivityX;
			if (heading >= 360f)
				heading = 0f;
			if (heading <= -360f)
				heading = 0f;
			tilt -= Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSensitivityY;
			tilt = Mathf.Clamp(tilt, angleLimits.x, angleLimits.y);
			transform.rotation = Quaternion.Euler(tilt, heading, 0);
			mouseScroll = -Input.GetAxisRaw("Mouse ScrollWheel");
			distance += mouseScroll * mouseScrollWheelSensitivity;
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
			transform.position = hit.point + transform.forward;
		else
			transform.position = point;
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
		Gizmos.color = Color.green;
		Gizmos.DrawLine(transform.position, target.position + playerOffset);
		if (Focus != null)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(transform.position, Focus.transform.position);
		}
	}


}

public enum SideSelector : int
{
	Right, Left
}