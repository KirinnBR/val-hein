using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NPC), true)]
public class NPCEditor : Editor
{
	NPC npc;

	private void OnEnable()
	{
		npc = target as NPC;
		
	}

	private void OnSceneGUI()
	{
		
		if (npc.ShowCombatGUI)
			DrawCombatGUI();
		else
			DrawNPCGUI();
	}

	private void DrawCombatGUI()
	{

	}

	private void DrawNPCGUI()
	{
		Vector3 dirAngleA = npc.DirFromAngle(-npc.normalVisionAngle / 2, false);
		Vector3 dirAngleB = npc.DirFromAngle(npc.normalVisionAngle / 2, false);
		Handles.color = Color.white;
		Handles.DrawWireArc(npc.transform.position, Vector3.up, Vector3.forward, 360, npc.wideDistanceVisionRadius);
		Handles.color = Color.red - new Color(0f, 0f, 0f, 0.6f);
		Handles.DrawSolidArc(npc.transform.position, Vector3.up, dirAngleB, 360 - npc.normalVisionAngle, npc.perifericVisionRadius);
		Handles.DrawSolidArc(npc.transform.position, Vector3.up, dirAngleA, npc.normalVisionAngle, npc.normalVisionRadius);

		if (npc is NPCArcher)
			DrawArcherGUI();
		if (npc is NPCPatroller)
			DrawPatrollerGUI();
	}

	private void DrawArcherGUI()
	{
		NPCArcher npcA = target as NPCArcher;
		Handles.color = Color.green;
		Handles.DrawWireArc(npcA.transform.position, Vector3.up, Vector3.forward, 360, npcA.attackRange);
		Handles.color = Color.yellow;
		Handles.DrawWireArc(npcA.transform.position, Vector3.up, Vector3.forward, 360, npcA.distanceToRetreat);
	}

	private void DrawPatrollerGUI()
	{
		NPCPatroller npcP = target as NPCPatroller;
		if (!EditorApplication.isPlaying)
			npcP.StartPos = npcP.transform.position;
		Handles.color = Color.green;
		List<Transform> patrolPoints;
		if (npcP.patrolPoints.Count > 0)
			patrolPoints = npcP.patrolPoints.FindAll(x => x != null);
		else
			patrolPoints = npcP.patrolPoints;
		
		var npcPos = npcP.StartPos;
		if (npcP.patrollingType == NPCPatroller.PatrollingType.Loop)
		{
			for (int i = 0; i < patrolPoints.Count; i++)
			{
				var point = patrolPoints[i];
				Handles.SphereHandleCap(0, point.position, Quaternion.identity, 0.5f, EventType.Repaint);
				if (i == 0)
				{
					if (!EditorApplication.isPlaying)
					{
						Handles.color = Color.red;
						Handles.DrawLine(npcPos, point.position);
						Handles.ArrowHandleCap(0, npcPos, Quaternion.LookRotation(point.position - npcPos), 2f, EventType.Repaint);
						Handles.color = Color.green;
					}
				}
				else
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
		else if (npcP.patrollingType == NPCPatroller.PatrollingType.Rewind)
		{
			for (int i = 0; i < patrolPoints.Count; i++)
			{
				var curPoint = patrolPoints[i];
				Handles.SphereHandleCap(0, curPoint.position, Quaternion.identity, 0.5f, EventType.Repaint);
				if (i == 0)
				{
					if (!EditorApplication.isPlaying)
					{
						Handles.color = Color.red;
						Handles.DrawLine(npcPos, curPoint.position);
						Handles.ArrowHandleCap(0, npcPos, Quaternion.LookRotation(curPoint.position - npcPos), 2f, EventType.Repaint);
						Handles.color = Color.green;
					}
				}
				else if (!patrolPoints[i - 1].Equals(curPoint))
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
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Editor Settings", EditorStyles.boldLabel);
		npc.ShowCombatGUI = EditorGUILayout.Toggle("Show Combat GUI?", npc.ShowCombatGUI);
	}

}
