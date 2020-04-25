using UnityEngine;
using System.Collections;
using Valhein.StateMachine;
using System;

public class NPCPatrollerAttackBehaviour : StateBehaviour
{
    [SerializeField]
    private float time = 5f;

    //This is used to tell the state machine when to decide to leave the state.
    public override bool CanExit => exit;

    [NonSerialized]
    private HostileNPC npc = null;
    [NonSerialized]
    private bool exit = false;
    
    //Called when the state is started.
    public override void OnStateEnter(FiniteStateMachine stateMachine)
    {
        if (npc == null)
        {
            npc = stateMachine.GetComponent<HostileNPC>();
        }
        exit = false;
    }
    
    //Called every frame.
    public override void OnStateUpdate(FiniteStateMachine stateMachine)
    {
        if (npc.currentAttacksLimit == npc.currentAttackCombo)
        {
            npc.currentAttackCombo = 0;
            npc.SetAttacksPerCombo();
            npc.canAttack = false;
            exit = true;
            npc.Invoke("ResetAttack", 5f);
            return;
        }

        if (npc.animationReady)
        {
            npc.agent.SetDestination(npc.target.position);
            if (npc.IsCloseEnoughToPoint(npc.target.position))
            {
                npc.Attack();
            }
        }
        else
        {
            npc.UpdateAnimation();
            if (npc.currentAttackFrame == npc.attacks[npc.currentAttackIndex].animationLength)
            {
                npc.FinishAnimation();
            }
        }

        
    }
    
    //Called when the state leaves.
    //public override void OnStateExit(StateMachine stateMachine)
    //{
    //    
    //}
}
