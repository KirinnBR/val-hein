using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(NPC), true)]
public class NPCEditor : Editor
{
	void OnSceneGUI()
	{
		NPC npc = target as NPC;
		Vector3 dirAngleA = npc.DirFromAngle(-npc.visionAngle / 2, false);
		Vector3 dirAngleB = npc.DirFromAngle(npc.visionAngle / 2, false);
		Handles.color = Color.white;
		Handles.DrawWireArc(npc.transform.position, Vector3.up, dirAngleB, 360 - npc.visionAngle, npc.visionRadius);
		Handles.color = Color.red - new Color(0f, 0f, 0f, 0.5f);
		Handles.DrawSolidArc(npc.transform.position, Vector3.up, Vector3.forward, 360, npc.shortDistanceVisionRadius);
		Handles.DrawSolidArc(npc.transform.position, Vector3.up, dirAngleA, npc.visionAngle, npc.visionRadius);
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
			NPCPatroller npcPatroller = target as NPCPatroller;
			Handles.color = Color.green;
			foreach (var point in npcPatroller.patrolPoints)
				if (point != null)
					Handles.SphereHandleCap(0, point.position, Quaternion.identity, 0.5f, EventType.Repaint);
		}
	}
}
