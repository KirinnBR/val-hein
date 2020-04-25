using UnityEngine;
using System.Collections;
using Valhein.StateMachine;

public class MovingBehaviour : StateBehaviour
{
    //This is used to tell the state machine when to decide to leave the state.
    //public override bool CanExit => true;

    private PlayerControllerSystem controller;
    private PlayerInputSystem input;

    //Called when the state is started.
    public override void OnStateEnter(FiniteStateMachine manager)
    {
        if (controller == null)
            controller = manager.GetComponent<PlayerControllerSystem>();
        if (input == null)
            input = manager.GetComponent<PlayerInputSystem>();
    }
    
    //Called every frame.
    public override void OnStateUpdate(FiniteStateMachine manager)
    {
        controller.Move();
        controller.UpdateAnimations();

        if (input.Jump)
        {
            manager.SetTrigger("Jump");
        }
        else if (input.Dodge)
        {
            manager.SetTrigger("Dodge");
        }
        else if (input.Attack)
        {
            manager.SetTrigger("Attack");
        }
    }
    
    //Called when the state leaves.
    //public override void OnStateExit(GameObject obj)
    //{
    //
    //}
}
