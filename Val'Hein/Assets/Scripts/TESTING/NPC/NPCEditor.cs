using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NPC), true)]
public class NPCEditor : Editor
{
	private void OnSceneGUI()
	{
		NPC npc = target as NPC;
		Handles.color = Color.white;
		Handles.DrawWireArc(npc.transform.position, Vector3.up, Vector3.forward, 360, npc.visionRadius);
		Vector3 viewAngleA = npc.DirFromAngle(-npc.visionAngle / 2, false);
		Vector3 viewAngleB = npc.DirFromAngle(npc.visionAngle / 2, false);
		Handles.DrawLine(npc.transform.position, npc.transform.position + viewAngleA * npc.visionRadius);
		Handles.DrawLine(npc.transform.position, npc.transform.position + viewAngleB * npc.visionRadius);
		Handles.color = Color.red;
		Handles.DrawWireArc(npc.transform.position, Vector3.up, Vector3.forward, 360, npc.shortDistanceVisionRadius);
	}
}
