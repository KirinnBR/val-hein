using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerCombat))]
public class PlayerCombatEditor : Editor
{
	SerializedProperty weapon, hitMarkers, attacks;

	private void OnEnable()
	{
		weapon = serializedObject.FindProperty("weapon");
		hitMarkers = serializedObject.FindProperty("hitMarkers");
		attacks = serializedObject.FindProperty("attacks");
		
	}


	public override void OnInspectorGUI()
	{

		serializedObject.Update();
		PlayerCombat p = target as PlayerCombat;
		//TODO SCRIPT THANG
		GUILayout.Space(25f);
		EditorGUILayout.LabelField(new GUIContent("<b>Combat Settings</b>"), new GUIStyle() { richText = true });
		GUILayout.Space(5f);
		p.maxSecondsToEndCombat = EditorGUILayout.FloatField("Max Seconds To End Combat", p.maxSecondsToEndCombat);
		List<string> layers = new List<string>();
		p.combatLayer = EditorGUILayout.LayerField("Combat Layer", p.combatLayer);
		p.comboValidationSeconds = EditorGUILayout.FloatField("Combo Validation Seconds", p.comboValidationSeconds);
		p.continuousDamage = EditorGUILayout.Toggle("Continuous Damage?", p.continuousDamage);
		if (p.continuousDamage)
			p.continuousDamageInterval = EditorGUILayout.FloatField("Continuous Damage Interval", p.continuousDamageInterval);
		p.enemyDetectionRadius = EditorGUILayout.FloatField("Enemy Detection Radius", p.enemyDetectionRadius);
		p.hasWeapon = EditorGUILayout.Toggle("Has Weapon?", p.hasWeapon);
		if (p.hasWeapon)
		{
			EditorGUILayout.ObjectField(weapon);
		}
		else
		{
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(hitMarkers, true);
			EditorGUILayout.PropertyField(attacks, true);
			if (EditorGUI.EndChangeCheck())
				serializedObject.ApplyModifiedProperties();
		}
	}


	private void OnSceneGUI()
	{
		PlayerCombat player = target as PlayerCombat;
		Handles.color = Color.yellow;
		Handles.DrawWireArc(player.transform.position, Vector3.up, player.transform.forward, 360, player.enemyDetectionRadius);
		Handles.color = Color.red - new Color(0, 0, 0, 0.5f);
	}

}
