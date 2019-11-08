using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCArcher : NPC
{
	#region Archer Combat Settings
	[Header("Archer Combat Settings")]
	[Tooltip("The minimum distance between the target and the NPC to retreat.")]
	public float distanceToRetreat = 5f;
	[Tooltip("The attack range of the NPC.")]
	public float attackRange = 15f;
	#endregion
	private Vector3 inicialPosition;
	private Quaternion inicialRotation;

    // Start is called before the first frame update
    protected override void Start()
    {
		base.Start();
		agent.stoppingDistance = attackRange - 1;
		inicialPosition = transform.position;
		inicialRotation = transform.rotation;
	}

	// Update is called once per frame
	protected override void Update()
    {
		base.Update();
		if (visibleObjects.Count > 0)
		{
			foreach (var visibleObject in visibleObjects)
			{
				if (visibleObject.tag == "Player")
				{
					StartPursuit(visibleObject);
				}
			}
		}
		else
		{
			agent.destination = inicialPosition;
			if (IsCloseEnoughToDestination)
			{
				transform.rotation = inicialRotation;
			}
		}
	}

	private void StartPursuit(Transform target)
	{
		agent.speed = battleSpeed;
		transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
		if (Vector3.Distance(transform.position, target.position) <= distanceToRetreat)
		{
			agent.stoppingDistance = 0;
			agent.destination = transform.position - transform.forward * distanceToRetreat;
		}
		else
		{
			agent.stoppingDistance = attackRange - 2;
			agent.SetDestination(target.position);
		}
		if (IsCloseEnoughToTarget(target.position))
		{
			if (!attackOnCooldown)
			{
				Attack(target);
			}
		}
	}

	private void UpdateCooldown()
	{
		if (attackOnCooldown)
		{
			currentCooldown -= Time.deltaTime;
			if (currentCooldown <= 0)
			{
				attackOnCooldown = false;
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		//Gizmos.color = Color.yellow;
		//Gizmos.DrawWireSphere(transform.position, distanceToRetreat);
		//Gizmos.color = Color.green;
		//Gizmos.DrawWireSphere(transform.position, attackRange - 1);
	}

}
