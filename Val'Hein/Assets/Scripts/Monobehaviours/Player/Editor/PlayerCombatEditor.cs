using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerCombat))]
public class PlayerCombatEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		PlayerCombat player = target as PlayerCombat;
		if (player.continuousDamage)
		{
			Rect aux = new Rect(15, 190 - EditorGUIUtility.singleLineHeight * 2.3f, 560, EditorGUIUtility.singleLineHeight);
			player.continuousDamageInterval = EditorGUI.FloatField(aux, new GUIContent("Continuous Damage Interval", "The delay, in seconds, it takes to check again if it is hitting something."), player.continuousDamageInterval);
		}
	}
}
