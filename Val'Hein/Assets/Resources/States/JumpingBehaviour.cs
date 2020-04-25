using UnityEngine;
using System.Collections;
using Valhein.StateMachine;

public class JumpingBehaviour : StateBehaviour
{
    //This is used to tell the state machine when to decide to leave the state.
    public override bool CanExit => !keepJump;

    private PlayerControllerSystem controller;
    private bool keepJump;

    //Called when the state is started.
    public override void OnStateEnter(FiniteStateMachine manager)
    {
        keepJump = true;
        if (controller == null)
            controller = manager.GetComponent<PlayerControllerSystem>();
        controller.Jump();
    }
    
    //Called every frame.
    public override void OnStateUpdate(FiniteStateMachine manager)
    {
        keepJump = controller.UpdateJump();
        controller.Move();
    }

}
