using UnityEngine;
using System.Collections;
using Valhein.StateMachine;

public class RunStateBehaviour : StateBehaviour
{
    [SerializeField]
    private Vector3 raycastOffset = Vector3.up;
    [SerializeField]
    private float searchWallCooldown = 0.3f;
    [SerializeField]
    private float angle = 120f;
    [SerializeField]
    private int numSamples = 10;

    //This is used to tell the state machine when to decide to leave the state.
    public override bool CanExit => false;

    private Vector3 runDir = Vector3.zero;
    private HostileNPC npc;
    private bool searchWall = true;
    private float cooldown = 0f, dist;
    private bool cooldownUp = false;

    public override void OnAwake(FiniteStateMachine stateMachine)
    {
        npc = stateMachine.GetComponent<HostileNPC>();
        dist = npc.agent.stoppingDistance + 1f;
    }

    //Called when the state is started.
    public override void OnStateEnter(FiniteStateMachine stateMachine)
    {
        float value = Random.value * 360f;
        SetDir(new Vector3(Mathf.Sin(value * Mathf.Deg2Rad), 0f, Mathf.Cos(value * Mathf.Deg2Rad)));
        searchWall = true;
    }

    //Called every frame.
    public override void OnStateUpdate(FiniteStateMachine stateMachine)
    {
        UpdateCooldown();
    }

    public override void OnStateFixedUpdate(FiniteStateMachine stateMachine)
    {
        npc.agent.SetDestination(npc.transform.position + runDir * dist);
        if (searchWall)
        {
            if (Physics.Raycast(npc.transform.position + raycastOffset, npc.transform.forward, npc.normalVisionRadius, npc.obstacleObjectsLayer, QueryTriggerInteraction.UseGlobal))
            {
                for (float i = angle / 2 - angle / numSamples; i >= 0; i -= angle / numSamples)
                {
                    Vector3 dirL = npc.DirFromAngle(i - angle / 2);
                    Vector3 dirR = npc.DirFromAngle(angle / 2 - i);
                    if (!Physics.Raycast(npc.transform.position + raycastOffset, dirR, npc.normalVisionRadius, npc.obstacleObjectsLayer, QueryTriggerInteraction.UseGlobal))
                    {
                        SetDir(dirR);
                        break;
                    }
                    else if (!Physics.Raycast(npc.transform.position + raycastOffset, dirL, npc.normalVisionRadius, npc.obstacleObjectsLayer, QueryTriggerInteraction.UseGlobal))
                    {
                        SetDir(dirL);
                        break;
                    }
                    else if (i == 0)
                    {
                        SetDir(-npc.transform.forward);
                    }
                }
            }
            else if (npc.agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathPartial)
            {
                SetDir(-npc.transform.forward);
            }
        }
    }

    private void SetDir(Vector3 dir)
    {
        runDir = dir;
        searchWall = false;
        cooldownUp = true;
    }

    private void UpdateCooldown()
    {
        if (cooldownUp)
        {
            if (cooldown >= searchWallCooldown)
            {
                searchWall = true;
                cooldown = 0f;
                cooldownUp = false;
            }
            else
            {
                cooldown += Time.deltaTime;
            }
        }
    }

    //Called when the state leaves.
    //public override void OnStateExit(StateMachineManager manager)
    //{
    //    
    //}
}
