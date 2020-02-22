using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CombatSystem))]
public class CombatSystemEditor : Editor
{
	private void OnSceneGUI()
	{
		CombatSystem player = target as CombatSystem;
		Handles.color = Color.yellow;
		Handles.DrawWireArc(player.transform.position, Vector3.up, player.transform.forward, 360, player.EnemyDetectionRadius);
	}

}
