using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Valhein.StateMachine
{
	[CreateAssetMenu(fileName = "State Machine Controller", menuName = "State Machine Controller")]
	public class FiniteStateMachineController : ScriptableObject
	{
		#region Fields

		[SerializeField]
		private List<State> m_states = new List<State>();
		public List<State> states => m_states;

		[SerializeField]
		private List<StateTransition> m_anyStateTransitions;
		public List<StateTransition> anyStateTransitions { get => m_anyStateTransitions; }

		[SerializeField]
		private List<StateBehaviour> m_persistentBehaviours = new List<StateBehaviour>();
		public List<StateBehaviour> persistentBehaviours => m_persistentBehaviours;


		[SerializeField]
		private string m_entryStateID;
		public string entryStateID { get { return m_entryStateID; } set { m_entryStateID = value; } }
		public State entryState { get { return FindStateByID(entryStateID); } }


		#endregion

		#region Methods

		public State FindStateByID(string id)
		{
			return states.Find(x => string.Equals(x.id, id));
		}

		#endregion

	}
}
