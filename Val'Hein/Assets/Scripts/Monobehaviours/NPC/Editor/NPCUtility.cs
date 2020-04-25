using UnityEngine;
using UnityEditor;

public class NPCUtility
{
	public static void DrawHostileNPCGUI(HostileNPC npc)
	{
		Vector3 dirAngleA = npc.DirFromAngle(-npc.normalVisionAngle / 2);
		Vector3 dirAngleB = npc.DirFromAngle(npc.normalVisionAngle / 2);
		Handles.color = Color.yellow;
		Handles.DrawWireArc(npc.transform.position, Vector3.up, Vector3.forward, 360f, npc.wideDistanceVisionRadius);
		Handles.color = new Color(0f, 0f, 0.7f, 0.6f);
		Handles.DrawSolidArc(npc.transform.position, Vector3.up, dirAngleB, 360f - npc.normalVisionAngle, npc.perifericVisionRadius);
		Handles.DrawSolidArc(npc.transform.position, Vector3.up, dirAngleA, npc.normalVisionAngle, npc.normalVisionRadius);
		Handles.color = Color.white;
		Handles.DrawWireArc(npc.transform.position, Vector3.up, npc.transform.forward, 360f, npc.dropRange);
	}
}
