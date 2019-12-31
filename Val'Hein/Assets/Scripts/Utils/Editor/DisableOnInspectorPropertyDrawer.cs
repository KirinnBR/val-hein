﻿using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(DisableOnInspectorAttribute))]
public class DisableOnInspectorPropertyDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		GUI.enabled = false;
		EditorGUI.PropertyField(position, property, label);
		GUI.enabled = true;
	}
}