using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0649
public class NPC : MonoBehaviour
{
	#region Object Detection Settings
	[Header("Object Detection Settings")]
	[Tooltip("The angle, in degrees, of the vision.")]
	[Range(0, 360)]
	public float visionAngle = 45f;
	[Tooltip("The distance, in meters, of the vision.")]
	public float visionRadius = 10f;
	[Tooltip("The distance, in meters, of the short-distance vision.")]
	public float shortDistanceVisionRadius = 3f;
	[Tooltip("The layer in which the objects to detect are.")]
	[SerializeField]
	private LayerMask detectionLayer;
	[Tooltip("The layer in which the obstacles are.")]
	[SerializeField]
	private LayerMask obstacleObjectsLayer;
	protected List<Transform> visibleObjects;
	#endregion

	protected virtual void Start()
	{
		visibleObjects = new List<Transform>();
	}

	protected void SearchObjects()
	{
		visibleObjects.Clear();
		var objectsInVisionRadius = Physics.OverlapSphere(transform.position, visionRadius, detectionLayer);
		if (objectsInVisionRadius.Length > 0)
		{
			for (int i = 0; i < objectsInVisionRadius.Length; i++)
			{
				Vector3 dirToTarget = (objectsInVisionRadius[i].transform.position - transform.position).normalized;
				if (Vector3.Angle(transform.forward, dirToTarget) < visionAngle / 2f)
				{
					if (!Physics.Linecast(transform.position, objectsInVisionRadius[i].transform.position, obstacleObjectsLayer))
					{

						visibleObjects.Add(objectsInVisionRadius[i].transform);
					}
				}
			}
		}
		var objectsInShortVisionRadius = Physics.OverlapSphere(transform.position, shortDistanceVisionRadius, detectionLayer);
		if (objectsInShortVisionRadius.Length > 0)
		{
			foreach (var _object in objectsInShortVisionRadius)
			{
				if (!visibleObjects.Contains(_object.transform))
				{
					visibleObjects.Add(_object.transform);
				}
			}
		}
	}

	public Vector3 DirFromAngle( float angle, bool isGlobal )
	{
		if (!isGlobal)
			angle += transform.eulerAngles.y;
		return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
	}

}
