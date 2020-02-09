using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0649
public class NPCPatroller : NPC
{
	#region Patrolling Settings

	[Header("Patrolling Settings")]

	public PatrollingType patrollingType;
	[Tooltip("The points to patrol.")]
	public List<Transform> patrolPoints;
	[Tooltip("The time to wait between patrol points.")]
	public int patrollingWaitTime = 5;
	protected int currentPatrolPoint = 0;
	protected bool onPatrol = false;

	private Coroutine patrolCoroutine = null;
	private Coroutine combatCoroutine = null;

	private Transform target;

	#endregion

	#region Common Methods

	// Start is called before the first frame update
	protected override void Start()
	{
		base.Start();
		agent.speed = speed.y;
	}

	// Update is called once per frame
	private void Update()
	{
		UpdateActualState();
		UpdateAnimator();
	}

	private void UpdateActualState()
	{
		if (visibleObjects.Count > 0)
		{
			if (target == null)
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

				if (target == null)
				{
					if (!onPatrol)
					{
						StartPatrol();
					}
				}

			}
			else if (!onCombat)
			{
				StartCombat();
			}
		}
		else
		{
			if (!onPatrol)
			{
				StartPatrol();
			}
		}
	}

	private void StartPatrol()
	{
		if (onCombat)
		{
			if (combatCoroutine != null)
				StopCoroutine(combatCoroutine);
			onCombat = false;
		}
		patrolCoroutine = StartCoroutine(Patrol());
	}

	private void StartCombat()
	{
		if (onPatrol)
		{
			if (patrolCoroutine != null)
				StopCoroutine(patrolCoroutine);
			onPatrol = false;
		}
		
		switch (npcType)
		{
			case NPCType.Human:
				combatCoroutine = StartCoroutine(HumanCombat());
				break;
			case NPCType.Beast:
				combatCoroutine = StartCoroutine(BeastCombat());
				break;
			case NPCType.Dummy:
				combatCoroutine = StartCoroutine(DummyCombat());
				break;
		}
	}

	private void UpdateAnimator()
	{
		if (onPatrol)
		{
			anim.SetFloat("Speed", agent.velocity.magnitude);
		}
		else if (onCombat)
		{
			anim.SetFloat("Speed", agent.velocity.magnitude);
			//Set bidimensional parameters, like:
			//anim.SetFloat("Speed X", agent.velocity.x);
			//anim.SetFloat("Speed Y", agent.velocity.y);
		}
	}

	#endregion

	#region Coroutines

	protected IEnumerator Patrol()
	{
		agent.speed = speed.y;

		onPatrol = true;

		if (patrolPoints.Count == 0) yield break;

		yield return new WaitUntil(() => IsCloseEnoughToPoint(agent.destination));
		yield return new WaitForSeconds(patrollingWaitTime);

		if (patrollingType == PatrollingType.Loop)
		{
			while (true)
			{
				agent.SetDestination(patrolPoints[currentPatrolPoint].position);
				yield return new WaitUntil(() => IsCloseEnoughToPoint(agent.destination));
				yield return new WaitForSeconds(patrollingWaitTime);
				currentPatrolPoint = currentPatrolPoint == patrolPoints.Count - 1 ? 0 : currentPatrolPoint + 1;
			}
		}
		else if (patrollingType == PatrollingType.Rewind)
		{
			bool rewinding = false;
			while (true)
			{
				agent.SetDestination(patrolPoints[currentPatrolPoint].position);

				yield return new WaitUntil(() => IsCloseEnoughToPoint(agent.destination));
				yield return new WaitForSeconds(patrollingWaitTime);

				if (!rewinding)
				{
					if (currentPatrolPoint == patrolPoints.Count - 1)
					{
						rewinding = true;
						currentPatrolPoint--;
					}
					else
					{
						currentPatrolPoint++;
					}
				}
				else
				{
					if (currentPatrolPoint == 0)
					{
						rewinding = false;
						currentPatrolPoint++;
					}
					else
					{
						currentPatrolPoint--;
					}
				}
			}
		}
	}

	private IEnumerator DummyCombat()
	{
		onCombat = true;

		Vector3 targetPos = target.position;

		if (Vector3.Distance(targetPos, transform.position) > wideDistanceVisionRadius) yield break;

		while (true)
		{
			targetPos = target.position;

			void MoveAgent(Vector3 point)
			{
				if (!IsAttacking)
				{
					agent.SetDestination(point);
				}

			}

			void SetDefend(bool value)
			{
				IsDefending = value;
				agent.speed = IsDefending ? speed.x : speed.z;
			}

			if (canAttack)
			{
				//While can attack, go near player and deal attack.
				agent.speed = speed.z;
				MoveAgent(targetPos);
				if (IsCloseEnoughToPoint(targetPos))
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
					SetDefend(true);
					MoveAgent(retreatPoint);
				}
				else if (Vector3.Distance(transform.position, targetPos) > normalVisionRadius)
				{
					SetDefend(false);
					MoveAgent(targetPos);
				}
				else
				{
					SetDefend(true);
					MoveAgent(transform.position);
				}
			}
			transform.LookAt(new Vector3(targetPos.x, transform.position.y, targetPos.z));
			yield return null;
		}
	}

	private IEnumerator BeastCombat()
	{
#pragma warning disable CS0162
		Debug.LogWarning("Beast Combat on development.");
		yield break;

		void MoveAgent(Vector3 point)
		{
			if (!IsAttacking)
				agent.SetDestination(point);
		}

		onCombat = true;

		while (true)
		{
			Vector3 targetPos = target.position;

			if (Vector3.Distance(targetPos, transform.position) > wideDistanceVisionRadius) yield break;

			if (canAttack)
			{
				//While can attack, go near player and deal attack.
				agent.speed = speed.z;
				MoveAgent(targetPos);
				if (IsCloseEnoughToPoint(targetPos))
					ProccessAttackAnimation();
			}
			else
			{
				//While can't attack, flank the target and raise guard, keeping certain distance from target (but not letting him go).
				Vector3 retreatPoint = transform.position - transform.forward.normalized * 2f;
				if (Vector3.Distance(transform.position, targetPos) < 4f)
				{
					agent.speed = speed.x;
					MoveAgent(retreatPoint);
				}
				else if (Vector3.Distance(transform.position, targetPos) > 5f)
				{
					agent.speed = speed.z;
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

	private IEnumerator HumanCombat()
	{
		Debug.LogWarning("Human Combat on development.");
		yield break;

		void MoveAgent(Vector3 point)
		{
			if (!IsAttacking)
				agent.SetDestination(point);
		}

		onCombat = true;

		while (true)
		{
			Vector3 targetPos = target.position;

			if (Vector3.Distance(targetPos, transform.position) > wideDistanceVisionRadius) yield break;

			if (canAttack)
			{
				//While can attack, go near player and deal attack.
				agent.speed = speed.z;
				MoveAgent(targetPos);
				if (IsCloseEnoughToPoint(targetPos))
					ProccessAttackAnimation();
			}
			else
			{
				//While can't attack, flank the target and raise guard, keeping certain distance from target (but not letting him go).
				Vector3 retreatPoint = transform.position - transform.forward.normalized * 2f;
				if (Vector3.Distance(transform.position, targetPos) < 4f)
				{
					agent.speed = speed.x;
					MoveAgent(retreatPoint);
				}
				else if (Vector3.Distance(transform.position, targetPos) > 5f)
				{
					agent.speed = speed.z;
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

	public enum PatrollingType
	{
		Loop, Rewind
	}

}
