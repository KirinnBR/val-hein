using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0649
public class NPCPatroller : HostileNPC
{
	#region Patrolling Settings

	[Header("Patrolling Settings")]

	[SerializeField]
	private PatrollingType m_patrollingType;
	public PatrollingType patrollingType => m_patrollingType;

	[Tooltip("The points to patrol.")]
	[SerializeField]
	private List<Transform> m_patrolPoints = new List<Transform>();
	public List<Transform> patrolPoints => m_patrolPoints;

	[SerializeField]
	private float patrollingWaitTime = 5f;

	//Patrolling state settings.
	private int currentPatrolPoint = 0;
	private bool canGoNext;
	private bool rewinding = false;

	#endregion

	protected override void Start()
	{
		base.Start();
	}

	protected override void Update()
	{
		base.Update();
		UpdateAnimator();
	}

	private void UpdateAnimator()
	{
		anim.SetFloat("Speed", agent.velocity.magnitude);
		//if (onCombat)
		//{
			//Set bidimensional parameters, like:
			//anim.SetFloat("Speed X", agent.velocity.x);
			//anim.SetFloat("Speed Y", agent.velocity.y);
		//}
	}

	public void StartPatrol()
	{
		agent.speed = speed.y;

		if (patrolPoints.Count == 0)
			return;

		canGoNext = true;
	}
	
	public void UpdatePatrol()
	{
		if (IsCloseEnoughToPoint(agent.destination))
		{
			if (canGoNext)
			{
				//agent.SetDestination(patrolPoints[currentPatrolPoint].position);
				StartCoroutine(WaitPatrollingTime());
			}
		}
	}

	private IEnumerator WaitPatrollingTime()
	{
		canGoNext = false;
		yield return new WaitUntil(() => IsCloseEnoughToPoint(agent.destination));
		float curTime = 0f;
		while (curTime <= patrollingWaitTime)
		{
			Quaternion curLook = patrolPoints[currentPatrolPoint].rotation;
			transform.rotation = Quaternion.Slerp(transform.rotation, curLook, 6f * Time.deltaTime);
			curTime += Time.deltaTime;
			yield return null;
		}

		if (patrollingType == PatrollingType.Loop)
		{
			currentPatrolPoint = currentPatrolPoint == patrolPoints.Count - 1 ? 0 : currentPatrolPoint + 1;
		}
		else
		{
			if (rewinding)
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
			else
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
		}
		canGoNext = true;
	}


	#region Coroutines

	/*
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

	*/

	/*
	private IEnumerator BeastCombat()
	{
		
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
		*/

	/*
	private IEnumerator HumanCombat()
	{
		
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
		*/

	#endregion

	public enum PatrollingType
	{
		Loop, Rewind
	}

}
