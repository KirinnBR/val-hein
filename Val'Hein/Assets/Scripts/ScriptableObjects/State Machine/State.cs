using System;
using System.Collections.Generic;
using UnityEngine;

namespace Valhein.StateMachine
{
    [Serializable]
    public class State
    {
        [SerializeField]
        private string m_name;

        [SerializeField]
        private List<StateTransition> m_transitions = new List<StateTransition>();

        [SerializeField]
        private StateBehaviour m_stateBehaviour;

        [SerializeField]
        private string m_id;

        public StateBehaviour stateBehaviour { get => m_stateBehaviour; set => m_stateBehaviour = value; }
        public List<StateTransition> transitions { get => m_transitions; set => m_transitions = value; }
        public string name { get => m_name; set => m_name = value; }
        public string id { get => m_id; set => m_id = value; }

        public void AssignStateBehaviour(StateBehaviour behaviour)
        {
            stateBehaviour = behaviour;
        }

    }
}