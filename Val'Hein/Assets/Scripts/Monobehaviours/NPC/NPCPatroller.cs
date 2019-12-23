using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#pragma warning disable CS0649
public class NPCPatroller : NPC, IDamageable
{
	#region Patrolling Settings

	[Header("Patrolling Settings")]

	[Tooltip("Speed when patrolling.")]
	public float patrollingSpeed = 5f;
	[Tooltip("The points to patrol.")]
	public List<Transform> patrolPoints;
	[Tooltip("The time to wait between patrol points.")]
	public int patrollingWaitTime = 5;
	private int currentPatrolPoint = 0;
	private bool patrolling = false;
	private bool pursuing = false;

	private Coroutine patrolCoroutine = null;
	private Coroutine pursuitCoroutine = null;

	private Transform target;

	#endregion

	#region Common Methods

	// Start is called before the first frame update
	protected override void Start()
	{
		base.Start();
		agent.speed = patrollingSpeed;
	}

	// Update is called once per frame
	private void Update()
	{
		CheckActualState();
		UpdateAnimator();
	}

	private void CheckActualState()
	{
		if (visibleObjects.Count > 0)
		{
			foreach (var visibleObject in visibleObjects)
			{
				if (visibleObject.CompareTag("Player"))
				{
					target = visibleObject.transform;
					break;
				}
				else
				{
					target = null;
				}
			}

			if (target != null)
			{
				if (!pursuing)
				{
					StartPursuit();
				}
			}
			else if (!patrolling)
			{
				StartPatrol();
			}
		}
		else
		{
			if (!patrolling)
			{
				StartPatrol();
			}
		}
	}

	private void StartPatrol()
	{
		if (pursuing)
		{
			StopCoroutine(pursuitCoroutine);
			pursuing = false;
		}
		patrolCoroutine = StartCoroutine(Patrol());
		patrolling = true;
	}

	private void StartPursuit()
	{
		if (patrolling)
		{
			if (patrolCoroutine != null)
				StopCoroutine(patrolCoroutine);
			patrolling = false;
		}
		pursuitCoroutine = StartCoroutine(Pursuit());
		pursuing = true;
	}

	private void UpdateAnimator()
	{
		if (patrolling)
		{
			anim.SetFloat("Speed", agent.velocity.magnitude);
		}
		else
		{
			anim.SetFloat("Speed", agent.velocity.magnitude);
			//Set bidimensional parameters.
		}
	}

	#endregion

	#region Coroutines

	private IEnumerator Patrol()
	{
		agent.speed = patrollingSpeed;

		if (patrolPoints.Count == 0)
		{
			yield break;
		}

		yield return new WaitUntil(() => IsCloseEnoughToTarget(agent.destination));
		yield return new WaitForSeconds(patrollingWaitTime);
		while (true)
		{
			agent.SetDestination(patrolPoints[currentPatrolPoint].position);
			yield return new WaitUntil(() => IsCloseEnoughToTarget(agent.destination));
			yield return new WaitForSeconds(patrollingWaitTime);
			currentPatrolPoint = currentPatrolPoint == patrolPoints.Count - 1 ? 0 : currentPatrolPoint + 1;
		}
	}

	private IEnumerator Pursuit()
	{
		void MoveAgent(Vector3 point)
		{
			if (!IsAttacking)
				agent.SetDestination(point);
		}

		while (true)
		{
			Vector3 targetPos = target.position;
			if (canAttack)
			{
				//While can attack, go near player and deal attack.
				agent.speed = battleSpeed;
				MoveAgent(targetPos);
				if (IsCloseEnoughToTarget(targetPos))
				{
					ProccessAttackAnimation();
				}
			}
			else
			{
				//While can't attack, flank the target and raise guard, keeping certain distance from target (but not letting him go).
				Vector3 retreatPoint = transform.position - transform.forward.normalized * 2f;
				if (Vector3.Distance(transform.position, targetPos) < 4f)
				{
					agent.speed = battleSpeed / 2;
					MoveAgent(retreatPoint);
				}
				else if (Vector3.Distance(transform.position, targetPos) > 5f)
				{
					MoveAgent(targetPos);
				}
				else
				{
					MoveAgent(transform.position);
				}
			}
			transform.LookAt(new Vector3(targetPos.x, transform.position.y, targetPos.z));
			yield return null;
		}
		
	}

	#endregion

}
