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
	private bool pursuing = false;
	private bool canAttack = true;

	private Coroutine patrolCoroutine = null;
	private Coroutine pursuitCoroutine = null;
	private Coroutine setAttackCooldownCoroutine = null;

	private Transform target;

	#endregion

	#region Patroller Combat Settings

	[Header("Patroller Combat Settings")]

	[SerializeField]
	private float attackCooldown = 2f;

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
		anim.SetFloat("Speed", agent.velocity.magnitude);
	}

	#endregion

	#region Coroutines

	private IEnumerator Patrol()
	{
		agent.speed = patrollingSpeed;

		if (patrolPoints.Length == 0)
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
			currentPatrolPoint = currentPatrolPoint == patrolPoints.Length - 1 ? 0 : currentPatrolPoint + 1;
		}
	}

	private IEnumerator Pursuit()
	{
		while (true)
		{
			Vector3 p = target.position;
			agent.destination = p;
			transform.LookAt(new Vector3(p.x, transform.position.y, p.z));

			if (IsCloseEnoughToTarget(p))
			{
				if (canAttack)
				{
					ProccessAttackAnimation();
					setAttackCooldownCoroutine = StartCoroutine(SetAttackCooldown());
				}
			}


			yield return null;
		}
		
	}

	private IEnumerator SetAttackCooldown()
	{
		canAttack = false;
		yield return new WaitForSeconds(attackCooldown);
		canAttack = true;
	}

	#endregion

}
