using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#pragma warning disable CS0649
public class NPCPatroller : NPC, IDamageable
{
	#region Patrolling Settings
	[Header("Patrolling Settings")]
	[Tooltip("Speed when patrolling.")]
	public float patrollingSpeed = 5f;
	[Tooltip("The points to patrol.")]
	public Transform[] patrolPoints;
	[Tooltip("The time to wait between patrol points.")]
	public int patrollingWaitTime = 5;
	private int currentPatrolPoint = 0;
	private bool patrolling = false;
	#endregion

	// Start is called before the first frame update
	protected override void Start()
    {
		base.Start();
		agent.speed = patrollingSpeed;
    }

	// Update is called once per frame
	protected override void Update()
    {
		base.Update();
		if (visibleObjects.Count > 0)
		{
			foreach (var visibleObject in visibleObjects)
			{
				//Condicao para ver se o inimigo se interessa em atacar aquele objeto.
				//Atualmente, ele procura o primeiro objeto que seja um player e ataca.
				if (visibleObject.tag == "Player")
				{
					StartPursuit(visibleObject.transform);
				}
			}
		}
		else
		{
			if (!patrolling)
			{
				StartCoroutine("Patrol");
			}
		}
    }

	private void StartPursuit(Transform target)
	{
		StopPatrolCoroutine();
		transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
		agent.speed = battleSpeed;
		agent.SetDestination(target.position);
		if (IsCloseEnoughToTarget(target.position))
		{
			if (!attackOnCooldown)
			{
				Attack(target);
			}
		}
	}

	private void StopPatrolCoroutine()
	{
		agent.speed = patrollingSpeed;
		StopCoroutine("Patrol");
		patrolling = false;
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

	private IEnumerator Patrol()
	{
		agent.speed = patrollingSpeed;
		patrolling = true;
		if (patrolPoints.Length == 0) yield break;
		yield return new WaitUntil(() => IsCloseEnoughToDestination);
		yield return new WaitForSeconds(patrollingWaitTime);
		while (true)
		{
			agent.SetDestination(patrolPoints[currentPatrolPoint].position);
			yield return new WaitUntil(() => IsCloseEnoughToDestination);
			yield return new WaitForSeconds(patrollingWaitTime);
			currentPatrolPoint = currentPatrolPoint == patrolPoints.Length - 1 ? 0 : currentPatrolPoint + 1;
		}
	}

}
