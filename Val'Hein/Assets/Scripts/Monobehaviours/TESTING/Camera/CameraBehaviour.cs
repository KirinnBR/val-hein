using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0649
[RequireComponent(typeof(Camera))]
public class CameraBehaviour : MonoBehaviour
{
	#region Target-Follow Settings
	[Header("Target-Follow Settings")]

	[SerializeField]
	[Tooltip("The target to follow.")]
	private Transform target;
	[SerializeField]
	[Tooltip("The minimum and maximum distance between target and camera.")]
	private Vector2 minMaxDistance = new Vector2(1f, 10f);
	[Tooltip("The distance between the camera and the target.")]
	public float distance = 10f;
	[Tooltip("The sensitivity of the mouse scroll wheel.")]
	[SerializeField]
	private float mouseScrollWheelSensitivity = 1f;
	[SerializeField]
	[Tooltip("The height of the player to fit in the Y axis.")]
	private float heightOffset = 1f;
	[SerializeField]
	[Tooltip("The minimum and maximum angle that the camera can go.")]
	private Vector2 angleBounds = new Vector2(0, 60);
	[SerializeField]
	[Tooltip("The sensitivity of the mouse in the X axis.")]
	private float mouseSensitivityX = 180f;
	[SerializeField]
	[Tooltip("The sensitivity of the mouse in the Y axis.")]
	private float mouseSensitivityY = 180f;

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
		tilt = Mathf.Clamp(tilt, angleBounds.x, angleBounds.y);
		transform.rotation = Quaternion.Euler(tilt, heading, 0);
		transform.position = target.position - transform.forward * distance + Vector3.up * heightOffset;
		CalculateDirections();
	}

	private void Update()
	{
		mouseScroll = -Input.GetAxisRaw("Mouse ScrollWheel");
		distance += mouseScroll * mouseScrollWheelSensitivity;
		distance = Mathf.Clamp(distance, minMaxDistance.x, minMaxDistance.y);
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


}
