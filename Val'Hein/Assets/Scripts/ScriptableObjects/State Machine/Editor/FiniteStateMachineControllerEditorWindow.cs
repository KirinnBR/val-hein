using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace Valhein.StateMachine.Editor
{
    public class FiniteStateMachineControllerEditorWindow : EditorWindow
    {
        private FiniteStateMachineController stateMachine;
        private State focusedState;
        private string newStateName, behaviourClassName;
        private bool waitingForStateBehaviourAssignment;
        private State stateToBeAssigned;
        private string classNameToBeAssigned;
        private Vector2 pos;

        public static void Open(FiniteStateMachineController stateMachine)
        {
            var window = GetWindow<FiniteStateMachineControllerEditorWindow>();
            window.titleContent.text = "State Machine Window";
            window.stateMachine = stateMachine;
        }

        private void OnEnable()
        {
            if (waitingForStateBehaviourAssignment)
            {
                waitingForStateBehaviourAssignment = false;
                CreateStatebehaviourCopy(classNameToBeAssigned, stateToBeAssigned);
            }
        }

        public void OnGUI()
        {
            if (stateMachine == null)
            {
                EditorGUILayout.HelpBox("Open a Finite State Machine asset.", MessageType.Warning);
                return;
            }

            pos = GUILayout.BeginScrollView(pos);
            GUILayout.BeginVertical();

            DrawStatesTab();

            DrawTransitionsTab();

            DrawAnyStateTransitionsTab();

            DrawBehavioursTab();

            GUILayout.EndVertical();

            GUILayout.EndScrollView();

            EditorUtility.SetDirty(stateMachine);
        }

        private void DrawStatesTab()
        {
            GUILayout.BeginVertical("box");

            for (int i = 0; i < stateMachine.states.Count; i++)
            {
                var state = stateMachine.states[i];
                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Entry State", GUILayout.MaxWidth(75)))
                {
                    stateMachine.entryStateID = state.id;
                }

                if (stateMachine.entryStateID != string.Empty && state.Equals(stateMachine.entryState))
                {
                    GUI.color = Color.red * 0.9f;
                }

                if (GUILayout.Button(state.name))
                {
                    focusedState = state;
                }

                GUI.color = Color.white;

                if (GUILayout.Button("X", GUILayout.MaxWidth(20)))
                {
                    stateMachine.states.Remove(state);
                    if (stateMachine.entryState.Equals(state))
                    {
                        stateMachine.entryStateID = string.Empty;
                    }
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 10f);

            newStateName = EditorGUILayout.TextField("State Name:", newStateName);

            if (GUILayout.Button("Add State"))
            {
                if (!string.IsNullOrEmpty(newStateName))
                {
                    stateMachine.states.Add(new State() { 
                        name = newStateName, 
                        stateBehaviour = null, 
                        transitions = new List<StateTransition>(), 
                        id = GUID.Generate().ToString() 
                    });
                    newStateName = string.Empty;
                }
            }

            GUILayout.EndVertical();
        }

        private void DrawTransitionsTab()
        {
            if (focusedState == null)
            {
                return;
            }

            GUILayout.BeginVertical("box");

            string[] names = new string[stateMachine.states.Count];
            for (int i = 0; i < stateMachine.states.Count; i++)
            {
                names[i] = stateMachine.states[i].name;
            }

            GUILayout.Label("Current State focused: " + focusedState.name);


            if (focusedState.transitions.Count > 0)
            {
                for (int i = 0; i < focusedState.transitions.Count; i++)
                {
                    var transition = focusedState.transitions[i];

                    GUILayout.BeginVertical("box");

                    if (GUILayout.Button("X"))
                    {
                        focusedState.transitions.Remove(focusedState.transitions[i]);
                        return;
                    }

                    GUILayout.BeginHorizontal();

                    var aux = GUILayout.SelectionGrid(stateMachine.states.IndexOf(stateMachine.FindStateByID(transition.targetStateID)), names, 1);
                    transition.targetStateID = stateMachine.FindStateByID(stateMachine.states[aux].id).id;

                    transition.trigger = EditorGUILayout.TextField("Trigger:", transition.trigger);

                    GUILayout.EndHorizontal();

                    GUILayout.EndVertical();
                }
            }

            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 10f);

            if (GUILayout.Button("Add Transition"))
            {
                focusedState.transitions.Add(new StateTransition() { targetStateID = stateMachine.states[0].id, trigger = string.Empty });
            }

            GUILayout.EndVertical();
        }

        private void DrawAnyStateTransitionsTab()
        {
            GUILayout.BeginVertical("box");

            string[] names = new string[stateMachine.states.Count];
            for (int i = 0; i < stateMachine.states.Count; i++)
            {
                names[i] = stateMachine.states[i].name;
            }

            for (int i = 0; i < stateMachine.anyStateTransitions.Count; i++)
            {
                var transition = stateMachine.anyStateTransitions[i];

                if (GUILayout.Button("X"))
                {
                    stateMachine.anyStateTransitions.Remove(transition);
                    return;
                }

                GUILayout.BeginHorizontal();

                var aux = GUILayout.SelectionGrid(stateMachine.states.IndexOf(stateMachine.FindStateByID(transition.targetStateID)), names, 1);

                transition.targetStateID = stateMachine.FindStateByID(stateMachine.states[aux].id).id;

                transition.trigger = EditorGUILayout.TextField("Trigger:", transition.trigger);

                GUILayout.EndHorizontal();

            }


            if (GUILayout.Button("Add Any State Transition"))
            {
                stateMachine.anyStateTransitions.Add(new StateTransition() { targetStateID = stateMachine.states[0].id, trigger = "Default" } );
            }

            GUILayout.EndVertical();
        }

        private void DrawBehavioursTab()
        {
            if (focusedState == null)
            {
                return;
            }

            GUILayout.BeginVertical("box");

            if (focusedState.stateBehaviour != null)
            {
                if (GUILayout.Button("X"))
                {
                    focusedState.stateBehaviour = null;
                    return;
                }

                SerializedObject obj = new SerializedObject(focusedState.stateBehaviour);
                SerializedProperty property = obj.GetIterator();

                property.NextVisible(true);
                GUI.enabled = false;
                EditorGUILayout.PropertyField(property, GUILayout.MaxWidth(500));
                GUI.enabled = true;

                
                while (property.NextVisible(true))
                {
                    EditorGUILayout.PropertyField(property, GUILayout.MaxWidth(500));
                }

                obj.ApplyModifiedProperties();
            }
            else
            {
                GUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 10f);

                behaviourClassName = EditorGUILayout.TextField("Behaviour name: ", behaviourClassName);
                if (GUILayout.Button("Add Behaviour"))
                {
                    if (!string.IsNullOrEmpty(behaviourClassName))
                    {
                        CreateStatebehaviourCopy(behaviourClassName, focusedState);
                    }
                }

                if (GUILayout.Button("Create Behaviour"))
                {
                    if (!string.IsNullOrEmpty(behaviourClassName))
                    {
                        waitingForStateBehaviourAssignment = true;
                        stateToBeAssigned = focusedState;
                        classNameToBeAssigned = behaviourClassName;
                        behaviourClassName = string.Empty;
                        GenerateStateBehaviourFile();
                    }
                }
            }
            GUILayout.EndVertical();
        }

        private void GenerateStateBehaviourFile()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources/States"))
                AssetDatabase.CreateFolder("Assets/Resources", "States");
            string copyPath = "Assets/Resources/States/" + behaviourClassName + ".cs";
            using(StreamWriter outfile = new StreamWriter(copyPath))
            {
                outfile.WriteLine("using System.Collections;");
                outfile.WriteLine("using System.Collections.Generic;");
                outfile.WriteLine("using UnityEngine;");
                outfile.WriteLine("using Valhein.StateMachine;");
                outfile.WriteLine("");
                outfile.WriteLine("public class " + behaviourClassName + " : StateBehaviour");
                outfile.WriteLine("{");
                outfile.WriteLine("    //This is used to tell the state machine when to decide to leave the state.");
                outfile.WriteLine("    //public override bool CanExit => false;");
                outfile.WriteLine("    ");
                outfile.WriteLine("    //Called when the state is started.");
                outfile.WriteLine("    //public override void OnStateEnter(FiniteStateMachine stateMachine)");
                outfile.WriteLine("    //{");
                outfile.WriteLine("    //   ");
                outfile.WriteLine("    //}");
                outfile.WriteLine("    ");
                outfile.WriteLine("    //Called every frame.");
                outfile.WriteLine("    //public override void OnStateUpdate(FiniteStateMachine stateMachine)");
                outfile.WriteLine("    //{");
                outfile.WriteLine("    //   ");
                outfile.WriteLine("    //}");
                outfile.WriteLine("    ");
                outfile.WriteLine("    //Called every fixed frame.");
                outfile.WriteLine("    //public override void OnStateFixedUpdate(FiniteStateMachine stateMachine)");
                outfile.WriteLine("    //{");
                outfile.WriteLine("    //   ");
                outfile.WriteLine("    //}");
                outfile.WriteLine("    ");
                outfile.WriteLine("    //Called when the state leaves.");
                outfile.WriteLine("    //public override void OnStateExit(FiniteStateMachine stateMachine)");
                outfile.WriteLine("    //{");
                outfile.WriteLine("    //    ");
                outfile.WriteLine("    //}");
                outfile.WriteLine("}");
            }
            AssetDatabase.Refresh();
        }

        private void CreateStatebehaviourCopy(string className, State state)
        {
            var aux = CreateInstance(className) as StateBehaviour;
            AssetDatabase.CreateAsset(aux, "Assets/Resources/States/References/" + GUID.Generate().ToString() + ".asset");
            state.stateBehaviour = aux;
            EditorUtility.SetDirty(aux);
        }

    }
}
