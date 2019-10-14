using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraBehaviour : MonoBehaviour
{
	[Header("Target-Follow Settings")]
	[SerializeField]
	private Transform target;
	[SerializeField]
	private float distance = 10f;
	[SerializeField]
	private float heightOffset = 1f;
	[SerializeField]
	private Vector2 angleBounds = new Vector2(0, 60);
	[SerializeField]
	private float mouseSensitivity = 180;

	private float heading = 0, tilt = 20;

    // Update is called once per frame
    void LateUpdate()
    {
		heading += Input.GetAxis("Mouse X") * Time.deltaTime * mouseSensitivity;
		tilt -= Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSensitivity;
		tilt = Mathf.Clamp(tilt, angleBounds.x, angleBounds.y);
		transform.rotation = Quaternion.Euler(tilt, heading, 0);
		transform.position = target.position - transform.forward * distance + Vector3.up * heightOffset;
	}
}
