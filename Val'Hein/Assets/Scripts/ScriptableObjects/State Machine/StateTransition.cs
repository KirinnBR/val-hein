using System;
using UnityEngine;

namespace Valhein.StateMachine
{
    [Serializable]
    public class StateTransition
    {
        [SerializeField]
        private string m_targetStateID;
        [SerializeField]
        private string m_trigger;

        [NonSerialized]
        private State targetState;

        public string trigger { get => m_trigger; set => m_trigger = value; }
        public string targetStateID { get => m_targetStateID; set => m_targetStateID = value; }

        public State GetTargetState(FiniteStateMachineController machine)
        {
            if (targetState == null)
            {
                targetState = machine.FindStateByID(targetStateID);
            }
            return targetState;
        }

    }

}