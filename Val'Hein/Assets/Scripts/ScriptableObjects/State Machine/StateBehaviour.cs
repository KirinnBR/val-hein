using UnityEngine;

namespace Valhein.StateMachine
{
    public abstract class StateBehaviour : ScriptableObject
    {
        public virtual void OnAwake(FiniteStateMachine stateMachine) { }
        public virtual void OnStateEnter(FiniteStateMachine stateMachine) {  }
        public virtual void OnStateUpdate(FiniteStateMachine stateMachine) { }
        public virtual void OnStateFixedUpdate(FiniteStateMachine stateMachine) { }
        public virtual void OnStateExit(FiniteStateMachine stateMachine) { }
        public virtual bool CanExit { get { return true; } }
    }
}
