using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NPC), true)]
public class NPCEditor : Editor
{
	protected virtual void OnSceneGUI()
	{
		NPC npc = target as NPC;
		Vector3 dirAngleA = npc.DirFromAngle(-npc.visionAngle / 2, false);
		Vector3 dirAngleB = npc.DirFromAngle(npc.visionAngle / 2, false);
		Handles.color = Color.white;
		Handles.DrawWireArc(npc.transform.position, Vector3.up, dirAngleB, 360 - npc.visionAngle, npc.visionRadius);
		Handles.color = Color.red;
		Handles.DrawLine(npc.transform.position, npc.transform.position + dirAngleA * npc.visionRadius);
		Handles.DrawLine(npc.transform.position, npc.transform.position + dirAngleB * npc.visionRadius);
		Handles.DrawWireArc(npc.transform.position, Vector3.up, Vector3.forward, 360, npc.shortDistanceVisionRadius);
		Handles.DrawWireArc(npc.transform.position, Vector3.up, dirAngleA, npc.visionAngle, npc.visionRadius);
		if (npc is NPCArcher)
		{
			NPCArcher npcArcher = target as NPCArcher;
			Handles.color = Color.green;
			Handles.DrawWireArc(npcArcher.transform.position, Vector3.up, Vector3.forward, 360, npcArcher.attackRange);
			Handles.color = Color.yellow;
			Handles.DrawWireArc(npcArcher.transform.position, Vector3.up, Vector3.forward, 360, npcArcher.distanceToRetreat);
		}
	}
}
