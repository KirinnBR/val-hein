using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NPCPatroller), false)]
public class NPCPatrollerEditor : Editor
{
	private NPCPatroller npcPatroller;

	private void OnEnable()
	{
		npcPatroller = target as NPCPatroller;
	}

	private void OnSceneGUI()
	{
		NPCUtility.DrawHostileNPCGUI(npcPatroller);
		DrawPatrollerGUI();
	}

	private void DrawPatrollerGUI()
	{
		Handles.color = Color.green;
		List<Transform> patrolPoints;
		if (npcPatroller.patrolPoints.Count > 0)
			patrolPoints = npcPatroller.patrolPoints.FindAll(x => x != null);
		else
			patrolPoints = npcPatroller.patrolPoints;

		if (patrolPoints.Count == 1)
		{
			Handles.color = Color.blue;
			Handles.SphereHandleCap(0, patrolPoints[0].position, Quaternion.identity, 0.5f, EventType.Repaint);
			return;
		}

		if (npcPatroller.patrollingType == NPCPatroller.PatrollingType.Loop)
		{
			for (int i = 0; i < patrolPoints.Count; i++)
			{
				var point = patrolPoints[i];
				if (i == 0)
					Handles.color = Color.blue;
				else
					Handles.color = Color.green;
				Handles.SphereHandleCap(0, point.position, Quaternion.identity, 0.5f, EventType.Repaint);
				if (i != 0)
				{
					var pointPos = patrolPoints[i - 1].position;
					if (!patrolPoints[i - 1].Equals(patrolPoints[i]))
					{
						Handles.DrawLine(pointPos, patrolPoints[i].position);
						Handles.ArrowHandleCap(0, pointPos, Quaternion.LookRotation(patrolPoints[i].position - pointPos), 2f, EventType.Repaint);
					}
				}

				if (i == patrolPoints.Count - 1)
				{
					Handles.DrawLine(patrolPoints[patrolPoints.Count - 1].position, patrolPoints[0].position);
					Handles.ArrowHandleCap(0, patrolPoints[patrolPoints.Count - 1].position, Quaternion.LookRotation(patrolPoints[0].position - patrolPoints[patrolPoints.Count - 1].position), 2f, EventType.Repaint);
				}
			}
		}
		else if (npcPatroller.patrollingType == NPCPatroller.PatrollingType.Rewind)
		{
			for (int i = 0; i < patrolPoints.Count; i++)
			{
				var curPoint = patrolPoints[i];
				if (i == 0)
					Handles.color = Color.blue;
				else
					Handles.color = Color.green;
				Handles.SphereHandleCap(0, curPoint.position, Quaternion.identity, 0.5f, EventType.Repaint);
				if (i != 0 && !patrolPoints[i - 1].Equals(curPoint))
				{
					var pointPos = patrolPoints[i - 1].position;
					Handles.DrawLine(pointPos, curPoint.position);
					Handles.ArrowHandleCap(0, pointPos, Quaternion.LookRotation(curPoint.position - pointPos), 2f, EventType.Repaint);
				}
			}
			Handles.color = Color.red;
			for (int i = patrolPoints.Count - 1; i >= 0; i--)
			{
				var curPoint = patrolPoints[i];
				if (i == patrolPoints.Count - 1)
					Handles.ArrowHandleCap(0, curPoint.position, Quaternion.LookRotation(patrolPoints[i - 1].position -
						curPoint.position), 2f, EventType.Repaint);
				else if (i == 0)
					Handles.ArrowHandleCap(0, curPoint.position, Quaternion.LookRotation(patrolPoints[i + 1].position -
						curPoint.position), 1f, EventType.Repaint);
				else
					Handles.ArrowHandleCap(0, curPoint.position, Quaternion.LookRotation(patrolPoints[i - 1].position -
						curPoint.position), 2f, EventType.Repaint);
			}
		}
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		if (GUILayout.Button("Create Waypoint"))
			CreateWaypoint();
	}

	private void CreateWaypoint()
	{
		GameObject waypoints = GameObject.Find(npcPatroller.name + " Waypoints");
		if (waypoints == null)
		{
			waypoints = new GameObject(npcPatroller.name + " Waypoints");
			waypoints.transform.position = npcPatroller.transform.position;
		}
		int waypointsLength = waypoints.transform.childCount;
		GameObject waypointObj = new GameObject("Waypoint " + waypointsLength);

		npcPatroller.patrolPoints.Add(waypointObj.transform);
		waypointObj.transform.SetParent(waypoints.transform, false);
		waypointObj.transform.localPosition = Vector3.zero + Vector3.right * Random.value;
	}

}
