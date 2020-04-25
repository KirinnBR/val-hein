using UnityEngine;
using Valhein.StateMachine;

public class DummyCombatBehaviour : StateBehaviour
{
    //TODO: Divide into lots of small states.
    /*
     [SerializeField]
    private int courage = 100;
    private int currentCourage { get; set; }

    //This is used to tell the state machine when to decide to leave the state.
    public override bool CanExit => false;

    private HostileNPC npc;

    //Called when the state is started.
    public override void OnStateEnter(StateMachine manager)
    {
        if (npc == null)
        {
            npc = manager.GetComponent<HostileNPC>();
            npc.OnTakeDamage += TakeCourage;
        }
        currentCourage = courage;
        npc.PrepareCombat();
    }

    private void TakeCourage()
    {
        currentCourage -= 10;
    }


    //Called every frame.
    public override void OnStateUpdate(StateMachine manager)
    {
        if (Vector3.Distance(npc.transform.position, npc.target.position) > npc.wideDistanceVisionRadius)
        {
            
        }

        //if (currentCourage < courage)
        //{
        //    currentCourage += npc.RaiseCourage();
        //    currentCourage = Mathf.Clamp(currentCourage, 0, courage);
        //}

        switch (currentCourage)
        {
            case int x when x >= courage * 0.8f:
                HostileCombat();
                break;
            case int x when x >= courage * 0.5f:
                SmartCombat();
                break;
            case int x when x >= courage * 0.2f:
                WarningCombat();
                break;
            default:
                Run();
                break;
        }

        //if (npc.IsCloseEnoughToPoint(npc.agent.destination))
        //{
        //    if (npc.canAttack)
        //    {
        //        npc.Attack();
        //    }
        //    else
        //    {
//
        //    }
        //}

        //npc.transform.rotation = Quaternion.Slerp(npc.transform.rotation, Quaternion.LookRotation(new Vector3(npc.target.position.x, npc.transform.position.y, npc.target.position.z) - npc.transform.position), 10f * Time.deltaTime);
        //npc.head.transform.rotation = Quaternion.LookRotation(npc.target.position + Vector3.up * 1.7f - npc.head.transform.position);
        //npc.agent.SetDestination(npc.target.position);
    }

    private void HostileCombat()
    {
        npc.agent.SetDestination(npc.target.position);
    }

    private void SmartCombat()
    {
        npc.agent.SetDestination(npc.target.position);
    }

    private void WarningCombat()
    {
        npc.agent.SetDestination(npc.target.position);
    }

    private void Run()
    {
        
    }

    


    //Called when the state leaves.
    public override void OnStateExit(StateMachine manager)
    {
        //npc.onTakeDamage -= TakeCourage;
    }
     */

}
