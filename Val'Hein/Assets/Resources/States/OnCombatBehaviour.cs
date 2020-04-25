using UnityEngine;
using System.Collections;
using Valhein.StateMachine;

public class OnCombatBehaviour : StateBehaviour
{
    [SerializeField]
    private int m_courage = 100;
    [SerializeField]
    private float safeDistance = 10f;
    [SerializeField]
    private float waitTimeBetweenDistanceChecks = 5f;

    private int courage { get; set; }

    //This is used to tell the state machine when to decide to leave the state.
    public override bool CanExit => true;

    private bool choosePosition;

    private HostileNPC npc;

    public override void OnAwake(FiniteStateMachine stateMachine)
    {
        courage = m_courage;
        npc = stateMachine.GetComponent<HostileNPC>();
        npc.agent.speed = npc.speed.z;
        npc.OnTakeDamage.AddListener(TakeCourage);
    }

    //Called when the state is started.
    public override void OnStateEnter(FiniteStateMachine stateMachine)
    {
        npc.target = npc.visibleTargets[0];

        choosePosition = true;
    }

    private void TakeCourage()
    {
        courage -= 10;
    }

    public override void OnStateUpdate(FiniteStateMachine stateMachine)
    {
        npc.transform.LookAt(new Vector3(npc.target.position.x, npc.transform.position.y, npc.target.position.z));

        if (Vector3.Distance(npc.transform.position, npc.target.position) > npc.normalVisionRadius)
        {
            npc.agent.SetDestination(npc.target.position);
            return;
        }

        if (choosePosition)
        {
            choosePosition = false;
            npc.StartCoroutine(SetPosition());
        }

        if (npc.canAttack)
        {
            stateMachine.SetTrigger("Attack");
        }

        if (courage <= 10)
        {
            stateMachine.SetTrigger("Run");
        }
    }

    private IEnumerator SetPosition()
    {
        Vector3 destination = npc.DirFromAngle(Random.value * 360f) * safeDistance + npc.target.position;
        Debug.DrawLine(npc.transform.position, destination, Color.red, 10f, false);
        while (!npc.IsCloseEnoughToPoint(destination))
        {
            npc.agent.SetDestination(destination);
            yield return null;
        }
        yield return new WaitForSeconds(waitTimeBetweenDistanceChecks);
        choosePosition = true;
    }

    //Called when the state leaves.
    public override void OnStateExit(FiniteStateMachine stateMachine)
    {
        npc.StopAllCoroutines();
    }
}
