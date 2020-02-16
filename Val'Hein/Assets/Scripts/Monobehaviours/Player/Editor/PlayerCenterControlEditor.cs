using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerCenterControl))]
public class PlayerCenterControlEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.HelpBox("These scripts require the following parameters in Animator: \n- Float: Velocity\n- Float: Velocity X\n- Float: Velocity Z\n" +
        "- Bool: Is Grounded\n- Trigger: Jump\n- Trigger: Attack\n- Int: Attack Index\n Bool: Is Targeting", MessageType.Info);
    }
}
