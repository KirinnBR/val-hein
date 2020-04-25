using UnityEngine;
using System.Collections;
using Valhein.StateMachine;

public class PatrollingBehaviour : StateBehaviour
{
    //This is used to tell the state machine when to decide to leave the state.
    public override bool CanExit => patroller.hasTargets;

    private NPCPatroller patroller;

    //Called when the state is started.
    public override void OnStateEnter(FiniteStateMachine manager)
    {
        if (patroller == null)
            patroller = manager.GetComponent<NPCPatroller>();
        patroller.StartPatrol();
    }

    //Called every frame.
    public override void OnStateUpdate(FiniteStateMachine manager)
    {
        patroller.UpdatePatrol();
    }

    //Called when the state leaves.
    public override void OnStateExit(FiniteStateMachine manager)
    {
        patroller.StopAllCoroutines();
    }
}
