using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerCombatSystem))]
public class CombatSystemEditor : Editor
{
	private void OnSceneGUI()
	{
		PlayerCombatSystem player = target as PlayerCombatSystem;
		Handles.color = Color.yellow;
		Handles.DrawWireArc(player.transform.position, Vector3.up, player.transform.forward, 360, player.enemyDetectionRadius);
	}

}
