using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0649
[RequireComponent(typeof(UnityEngine.Camera))]
public class CameraBehaviour : MonoBehaviour
{
	#region Target-Follow Settings
	[Header("Target-Follow Settings")]

	[SerializeField]
	[Tooltip("The target to follow.")]
	private Transform target;
	[SerializeField]
	[Tooltip("The minimum and maximum distance between target and camera.")]
	private Vector2 distanceLimits = new Vector2(1f, 10f);
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

	private float heading = 0f, tilt = 20f;
	private float mouseScroll = 0f;

	private Vector3 forward, right;
	public Vector3 Forward { get { return forward; } }
	public Vector3 Right { get { return right; } }

	// Update is called once per frame
	private void LateUpdate()
    {
		heading += Input.GetAxis("Mouse X") * Time.deltaTime * mouseSensitivityX;
		tilt -= Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSensitivityY;
		tilt = Mathf.Clamp(tilt, angleLimits.x, angleLimits.y);
		transform.rotation = Quaternion.Euler(tilt, heading, 0);
		Move(target.position - transform.forward * distance + playerOffset);
		CalculateDirections();
	}

	private void Update()
	{
		mouseScroll = -Input.GetAxisRaw("Mouse ScrollWheel");
		distance += mouseScroll * mouseScrollWheelSensitivity;
		distance = Mathf.Clamp(distance, distanceLimits.x, distanceLimits.y);
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
	}


}
