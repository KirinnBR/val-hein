using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(PlayerCombat))]
public class PlayerCombatEditor : Editor
{
	SerializedProperty weapon, hitMarkers, attacks, buttonToAttack, keyToTarget, hitMarkersManager;

	private void OnEnable()
	{
		weapon = serializedObject.FindProperty("weapon");
		hitMarkers = serializedObject.FindProperty("hitMarkers");
		attacks = serializedObject.FindProperty("attacks");
		buttonToAttack = serializedObject.FindProperty("buttonToAttack");
		keyToTarget = serializedObject.FindProperty("keyToTarget");
		hitMarkersManager = serializedObject.FindProperty("hitMarkersManager");
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
		LayerMask temp = EditorGUILayout.MaskField(new GUIContent() { text = "Combat Layer" }, InternalEditorUtility.LayerMaskToConcatenatedLayersMask(p.combatLayer), InternalEditorUtility.layers);
		p.combatLayer = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(temp);
		p.comboValidationSeconds = EditorGUILayout.FloatField("Combo Validation Seconds", p.comboValidationSeconds);
		p.enemyDetectionRadius = EditorGUILayout.FloatField("Enemy Detection Radius", p.enemyDetectionRadius);
		p.hasWeapon = EditorGUILayout.Toggle("Has Weapon?", p.hasWeapon);
		if (p.hasWeapon)
		{
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.ObjectField(weapon);
			if (EditorGUI.EndChangeCheck())
				serializedObject.ApplyModifiedProperties();
		}
		else
		{
			p.continuousDamage = EditorGUILayout.Toggle("Continuous Damage?", p.continuousDamage);
			if (p.continuousDamage)
				p.continuousDamageInterval = EditorGUILayout.FloatField("Continuous Damage Interval", p.continuousDamageInterval);
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(hitMarkers, true);
			EditorGUILayout.PropertyField(hitMarkersManager, true);
			if (EditorGUI.EndChangeCheck())
				serializedObject.ApplyModifiedProperties();
		}
		GUILayout.Space(5f);
		EditorGUILayout.LabelField(new GUIContent("<b>Combo Settings</b>"), new GUIStyle() { richText = true });
		GUILayout.Space(5f);
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(attacks, true);
		if (EditorGUI.EndChangeCheck())
			serializedObject.ApplyModifiedProperties();
		GUILayout.Space(5f);
		EditorGUILayout.LabelField(new GUIContent("<b>Input Settings</b>"), new GUIStyle() { richText = true });
		GUILayout.Space(5f);
		EditorGUILayout.PropertyField(buttonToAttack);
		EditorGUILayout.PropertyField(keyToTarget);
	}


	private void OnSceneGUI()
	{
		PlayerCombat player = target as PlayerCombat;
		Handles.color = Color.yellow;
		Handles.DrawWireArc(player.transform.position, Vector3.up, player.transform.forward, 360, player.enemyDetectionRadius);
		Handles.color = Color.red - new Color(0, 0, 0, 0.5f);
	}

}
