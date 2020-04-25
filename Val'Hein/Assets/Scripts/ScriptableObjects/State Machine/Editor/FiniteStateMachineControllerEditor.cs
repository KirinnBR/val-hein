using UnityEditor;
using UnityCustomEditor = UnityEditor.Editor;
using UnityEngine;

namespace Valhein.StateMachine.Editor
{
    [CustomEditor(typeof(FiniteStateMachineController))]
    public class FiniteStateMachineControllerEditor : UnityCustomEditor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Editor"))
            {
                FiniteStateMachineControllerEditorWindow.Open(target as FiniteStateMachineController);
            }
        }
    }
}
