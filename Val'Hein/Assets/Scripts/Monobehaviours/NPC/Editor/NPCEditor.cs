using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NPC), true)]
public class NPCEditor : Editor
{

	void OnValidate()
	{

	}

	private void OnSceneGUI()
	{
		NPC npc = target as NPC;
		Vector3 dirAngleA = npc.DirFromAngle(-npc.normalVisionAngle / 2, false);
		Vector3 dirAngleB = npc.DirFromAngle(npc.normalVisionAngle / 2, false);
		Handles.color = Color.white;
		Handles.DrawWireArc(npc.transform.position, Vector3.up, Vector3.forward, 360, npc.wideDistanceVisionRadius);
		Handles.color = Color.red - new Color(0f, 0f, 0f, 0.5f);
		Handles.DrawSolidArc(npc.transform.position, Vector3.up, Vector3.forward, 360, npc.perifericVisionRadius);
		Handles.DrawSolidArc(npc.transform.position, Vector3.up, dirAngleA, npc.normalVisionAngle, npc.normalVisionRadius);
		if (npc is NPCArcher)
		{
			NPCArcher npcArcher = target as NPCArcher;
			Handles.color = Color.green;
			Handles.DrawWireArc(npcArcher.transform.position, Vector3.up, Vector3.forward, 360, npcArcher.attackRange);
			Handles.color = Color.yellow;
			Handles.DrawWireArc(npcArcher.transform.position, Vector3.up, Vector3.forward, 360, npcArcher.distanceToRetreat);
		}
		if (npc is NPCPatroller)
		{
			NPCPatroller npcP = target as NPCPatroller;
			Handles.color = Color.green;
			var patrolPoints = npcP.patrolPoints.FindAll(x => x != null);
			var npcPos = npcP.transform.position;
			for (int i = 0; i < patrolPoints.Count; i++)
			{
				var point = patrolPoints[i];
				Handles.SphereHandleCap(0, point.position, Quaternion.identity, 0.5f, EventType.Repaint);
				if (i == 0)
				{
					Handles.DrawLine(npcPos, patrolPoints[i].position);
					Handles.ArrowHandleCap(0, npcPos, Quaternion.LookRotation(patrolPoints[i].position - npcPos), 2f, EventType.Repaint);
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
					Handles.DrawLine(patrolPoints[patrolPoints.Count - 1].position, npcPos);
					Handles.ArrowHandleCap(0, patrolPoints[patrolPoints.Count - 1].position, Quaternion.LookRotation(npcPos - patrolPoints[patrolPoints.Count - 1].position), 2f, EventType.Repaint);
				}
			}
		}
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		NPC npc = target as NPC;

		if (npc.perifericVisionRadius <= 0f)
			npc.perifericVisionRadius = 0f;
		else if (npc.perifericVisionRadius >= npc.normalVisionRadius)
			npc.perifericVisionRadius = npc.normalVisionRadius;

		if (npc.wideDistanceVisionRadius <= npc.normalVisionRadius)
			npc.wideDistanceVisionRadius = npc.normalVisionRadius;

		if (npc.normalVisionRadius <= 0f)
			npc.normalVisionRadius = 0f;
	}
}
