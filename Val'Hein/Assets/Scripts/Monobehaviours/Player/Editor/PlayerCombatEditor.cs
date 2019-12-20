using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(PlayerCombat))]
public class PlayerCombatEditor : Editor
{
	private void OnSceneGUI()
	{
		PlayerCombat player = target as PlayerCombat;
		Handles.color = Color.yellow;
		Handles.DrawWireArc(player.transform.position, Vector3.up, player.transform.forward, 360, player.EnemyDetectionRadius);
		Handles.color = Color.red - new Color(0, 0, 0, 0.5f);
	}

}
