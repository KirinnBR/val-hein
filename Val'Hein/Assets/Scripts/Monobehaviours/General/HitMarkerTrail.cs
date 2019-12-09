using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HitMarkerTrail : MonoBehaviour
{
	public float lifetime = 0.3f;
	private List<Vector3> previoustPoints = new List<Vector3>();
	private HitMarker marker;

	 private void Start()
	{
		marker = GetComponent<HitMarker>();
	}

	private void Update()
	{
		previoustPoints.Add(marker.transform.position);
		StartCoroutine(ClearPoint());
	}

	private IEnumerator ClearPoint()
	{
		yield return new WaitForSeconds(lifetime);
		previoustPoints.RemoveAt(0);
	}

	private void OnDrawGizmos()
	{
		Handles.color = Color.white;
		Handles.DrawAAPolyLine(previoustPoints.ToArray());
	}

}
