using UnityEngine;
using System.Collections;
using Valhein.StateMachine;

public class AttackingBehaviour : StateBehaviour
{
    //This is used to tell the state machine when to decide to leave the state.
    public override bool CanExit => !keepAttack;

    private bool keepAttack;
    private PlayerCombatSystem combat;

    //Called when the state is started.
    public override void OnStateEnter(FiniteStateMachine stateMachine)
    {
        if (combat == null)
        {
            combat = stateMachine.GetComponent<PlayerCombatSystem>();
        }
        keepAttack = true;
        combat.ProccessAttackAnimation();
    }
    
    //Called every frame.
    public override void OnStateUpdate(FiniteStateMachine stateMachine)
    {
        if (combat.currentAttackFrame >= combat.currentAttack.animationLength)
        {
            combat.FinishAnimation();
            keepAttack = false;
        }
        else
            combat.UpdateAnimation();
    }
    
    //Called when the state leaves.
    //public override void OnStateExit(GameObject obj)
    //{
    //
    //}
}
