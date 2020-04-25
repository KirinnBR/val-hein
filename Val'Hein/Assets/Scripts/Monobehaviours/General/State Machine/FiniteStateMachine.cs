using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Valhein.StateMachine
{
	public class FiniteStateMachine : MonoBehaviour
	{
		[SerializeField]
		private FiniteStateMachineController controller;

		#region Fields

		private State m_currentState;
		private State currentState
		{
			get
			{
				return m_currentState;
			}
			set
			{
				if (m_currentState != null && m_currentState.stateBehaviour != null)
				{
					m_currentState.stateBehaviour.OnStateExit(this);
				}
				m_currentState = value;
				m_currentState.stateBehaviour.OnStateEnter(this);
			}
		}

		#endregion

		#region Methods

		private void Awake()
		{
			if (controller.entryState == null)
			{
				currentState = controller.states[0];
			}
			else
			{
				currentState = controller.entryState;
			}

			foreach (var behaviour in controller.persistentBehaviours)
			{
				behaviour.OnAwake(this);
			}
			foreach (var state in controller.states)
			{
				state.stateBehaviour.OnAwake(this);
			}
		}

		private void Update()
		{
			currentState.stateBehaviour.OnStateUpdate(this);
			if (currentState.stateBehaviour.CanExit)
			{
				foreach (var transition in currentState.transitions)
				{
					if (string.IsNullOrEmpty(transition.trigger))
					{
						currentState = transition.GetTargetState(controller);
					}
				}
			}
			foreach (var behaviour in controller.persistentBehaviours)
			{
				behaviour.OnStateUpdate(this);
			}
		}

		private void FixedUpdate()
		{
			currentState.stateBehaviour.OnStateFixedUpdate(this);
		}

		public void SetTrigger(string name)
		{
			foreach (var transition in currentState.transitions)
			{
				if (string.Equals(transition.trigger, name))
				{
					currentState = transition.GetTargetState(controller);
					return;
				}
			}

			foreach (var transition in controller.anyStateTransitions)
			{
				if (string.Equals(transition.trigger, name))
				{
					currentState = transition.GetTargetState(controller);
					return;
				}
			}
		}

		#endregion



	}
}